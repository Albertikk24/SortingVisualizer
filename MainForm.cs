using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SortingVisualizer.Algorithms;
using SortingVisualizer.Services;
using SortingVisualizer.Visualization;

namespace SortingVisualizer
{
    public partial class MainForm : Form
    {
        private readonly SortVisualizer _visualizer;

        // Режим: true — каждая итерация, false — только финальный массив
        private bool _showEveryIteration = true;

        // Порог: если элементов больше — анимация выключается (защита от зависания)
        private const int AnimateThreshold = 500;

        private double[] _currentArray = new double[0];

        public MainForm()
        {
            InitializeComponent();
            _visualizer = new SortVisualizer(panelViz);
            UpdateToggleCaption();

            // Подписки на события — ПОСЛЕ InitializeComponent,
            // чтобы не падал дизайнер WinForms.
            nudDelay.ValueChanged += NudDelay_ValueChanged;
            nudSkip.ValueChanged += NudSkip_ValueChanged;
            panelViz.Paint += PanelViz_Paint;
        }

        private void NudDelay_ValueChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Задержка между кадрами: " + nudDelay.Value + " мс";
        }

        private void NudSkip_ValueChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Пропускать шагов: " + nudSkip.Value;
        }

        private void PanelViz_Paint(object sender, PaintEventArgs e)
        {
            if (panelViz == null) return;
            if (panelViz.BackgroundImage != null)
                e.Graphics.DrawImageUnscaled(panelViz.BackgroundImage, 0, 0);
        }

        // ================== ЗАГРУЗКА ==================

        private void MiLoadExcel_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel / CSV|*.xlsx;*.xlsm;*.csv;*.txt|All files|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var data = ExcelLoader.Load(ofd.FileName);
                    SetData(data);
                    lblStatus.Text = "Загружено " + data.Length + " значений из файла.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка чтения: " + ex.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MiLoadGoogle_Click(object sender, EventArgs e)
        {
            string url = Prompt.ShowDialog(
                "Введите публичную ссылку на Google Sheets:", "Google Sheets");
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                var data = GoogleSheetsLoader.LoadFromUrl(url);
                SetData(data);
                lblStatus.Text = "Загружено " + data.Length + " значений из Google Sheets.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MiGenerate_Click(object sender, EventArgs e)
        {
            using (var dlg = new GenerateDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var data = DataGenerator.Generate(
                        dlg.Count, dlg.MinValue, dlg.MaxValue, dlg.Decimals);
                    SetData(data);
                    lblStatus.Text = "Сгенерировано " + data.Length + " значений.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка генерации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SetData(double[] data)
        {
            _currentArray = data;
            dgvInput.Rows.Clear();
            for (int i = 0; i < data.Length; i++)
                dgvInput.Rows.Add(data[i].ToString(CultureInfo.CurrentCulture));

            _visualizer.Draw(data);
            panelViz.Invalidate();
        }

        // ================== РАСЧЁТ ==================

        private async void MiCalculate_Click(object sender, EventArgs e)
        {
            // 1. Чтение данных из DataGridView
            double[] input;
            string err;
            if (!TryReadGrid(out input, out err))
            {
                MessageBox.Show(err, "Некорректные данные",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (input.Length == 0)
            {
                MessageBox.Show("Нет данных для сортировки.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Лимит BOGO
            int bogoLimit;
            if (!int.TryParse(txtBogoLimit.Text.Trim(), out bogoLimit) || bogoLimit <= 0)
            {
                MessageBox.Show("Лимит итераций BOGO должен быть положительным целым числом.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Выбранные алгоритмы
            var algorithms = new List<ISortAlgorithm>();
            if (cbBubble.Checked) algorithms.Add(new BubbleSort());
            if (cbInsertion.Checked) algorithms.Add(new InsertionSort());
            if (cbShaker.Checked) algorithms.Add(new ShakerSort());
            if (cbQuick.Checked) algorithms.Add(new QuickSort());
            if (cbBogo.Checked) algorithms.Add(new BogoSort());

            if (algorithms.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один алгоритм.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ascending = rbAscending.Checked;

            // 4. Скорость — читаем прямо с формы в момент запуска
            int delayMs = (int)nudDelay.Value;
            int skipSteps = Math.Max(1, (int)nudSkip.Value);

            SetUiEnabled(false);
            dgvStats.Rows.Clear();

            try
            {
                for (int idx = 0; idx < algorithms.Count; idx++)
                {
                    var algo = algorithms[idx];
                    var copy = (double[])input.Clone();
                    var sw = Stopwatch.StartNew();
                    string status = "OK";

                    lblStatus.Text = "Сортировка: " + algo.Name + "...";
                    Application.DoEvents();

                    bool animate = _showEveryIteration && copy.Length <= AnimateThreshold;

                    try
                    {
                        var localAlgo = algo;
                        var localCopy = copy;
                        var localAscending = ascending;
                        var localBogoLimit = bogoLimit;
                        var localAnimate = animate;
                        var localSkip = skipSteps;
                        var localDelay = delayMs;

                        await Task.Run(() =>
                        {
                            int stepCounter = 0;

                            Action<double[], int, int> onStep = (arr, a, b) =>
                            {
                                if (!localAnimate) return;

                                stepCounter++;
                                if (stepCounter % localSkip != 0) return;

                                if (panelViz.IsHandleCreated)
                                {
                                    try
                                    {
                                        panelViz.BeginInvoke(new Action(() =>
                                        {
                                            _visualizer.Draw(arr, a, b);
                                            panelViz.Invalidate();
                                        }));
                                    }
                                    catch (InvalidOperationException) { }
                                }

                                if (localDelay > 0)
                                    Thread.Sleep(localDelay);
                            };

                            localAlgo.Sort(localCopy, localAscending, onStep, localBogoLimit);
                        });
                    }
                    catch (Exception ex)
                    {
                        status = "Ошибка: " + ex.Message;
                    }

                    sw.Stop();

                    dgvStats.Rows.Add(
                        algo.Name,
                        input.Length.ToString(),
                        sw.Elapsed.TotalMilliseconds.ToString("F2"),
                        status);

                    _visualizer.Draw(copy);
                    panelViz.Invalidate();

                    lblStatus.Text = algo.Name + ": " +
                        sw.Elapsed.TotalMilliseconds.ToString("F2") + " мс (" + status + ")";

                    if (animate)
                        await Task.Delay(400);
                }

                var success = new List<KeyValuePair<string, double>>();
                for (int i = 0; i < dgvStats.Rows.Count; i++)
                {
                    var row = dgvStats.Rows[i];
                    if (row.Cells[3].Value != null && (string)row.Cells[3].Value == "OK")
                    {
                        double t = double.Parse((string)row.Cells[2].Value);
                        success.Add(new KeyValuePair<string, double>(
                            (string)row.Cells[0].Value, t));
                    }
                }

                if (success.Count > 1)
                {
                    var fastest = success.OrderBy(x => x.Value).First();
                    lblStatus.Text = "Быстрее всех: " + fastest.Key + " (" +
                        fastest.Value.ToString("F2") + " мс)";
                }
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private void SetUiEnabled(bool enabled)
        {
            menuStrip.Enabled = enabled;
            dgvInput.Enabled = enabled;
            txtBogoLimit.Enabled = enabled;
            if (grpAlgorithms != null) grpAlgorithms.Enabled = enabled;
            if (grpDirection != null) grpDirection.Enabled = enabled;
            if (grpBogo != null) grpBogo.Enabled = enabled;
            if (grpSpeed != null) grpSpeed.Enabled = enabled;
        }

        private bool TryReadGrid(out double[] data, out string error)
        {
            error = null;
            var list = new List<double>();

            for (int i = 0; i < dgvInput.Rows.Count; i++)
            {
                var row = dgvInput.Rows[i];
                if (row.IsNewRow) continue;

                var cell = row.Cells[0].Value;
                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;

                string s = cell.ToString().Trim().Replace('.', ',');
                double v;
                if (double.TryParse(s, NumberStyles.Any,
                        CultureInfo.CurrentCulture, out v))
                    list.Add(v);
                else
                {
                    error = "Значение \"" + cell + "\" в строке " +
                            (row.Index + 1) + " не является числом.";
                    data = null;
                    return false;
                }
            }

            data = list.ToArray();
            return true;
        }

        // ================== ПРОЧЕЕ ==================

        private void MiClear_Click(object sender, EventArgs e)
        {
            dgvInput.Rows.Clear();
            dgvStats.Rows.Clear();
            _currentArray = new double[0];
            panelViz.BackgroundImage = null;
            panelViz.Invalidate();
            lblStatus.Text = "Очищено.";
        }

        private void MiToggleView_Click(object sender, EventArgs e)
        {
            _showEveryIteration = !_showEveryIteration;
            UpdateToggleCaption();
        }

        private void UpdateToggleCaption()
        {
            miToggleView.Text = _showEveryIteration
                ? "Режим: каждая итерация"
                : "Режим: только финальный массив";
        }

        private void MiExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}