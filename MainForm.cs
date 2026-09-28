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

        private bool _showEveryIteration = true;
        private const int AnimateThreshold = 500;

        private double[] _currentArray = new double[0];

        public MainForm()
        {
            InitializeComponent();
            _visualizer = new SortVisualizer(panelViz);
            UpdateToggleCaption();

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

        // ================== КАДР ВИЗУАЛИЗАЦИИ ==================

        private class Frame
        {
            public double[] Data;
            public int A;
            public int B;
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

            // 4. Настройки визуализации
            int delayMs = (int)nudDelay.Value;
            int skipSteps = Math.Max(1, (int)nudSkip.Value);

            SetUiEnabled(false);
            dgvStats.Rows.Clear();

            try
            {
                for (int idx = 0; idx < algorithms.Count; idx++)
                {
                    var algo = algorithms[idx];

                    // ==== ЭТАП 1: СЧИТАЕМ (в фоне, без пауз) ====
                    lblStatus.Text = "Сортировка: " + algo.Name + " (вычисление)...";
                    Application.DoEvents();

                    var copy = (double[])input.Clone();
                    var frames = new List<Frame>();

                    bool recordFrames = _showEveryIteration && copy.Length <= AnimateThreshold;
                    int frameCounter = 0;
                    int stepCount = 0;

                    var sw = Stopwatch.StartNew();

                    try
                    {
                        var localAlgo = algo;
                        var localCopy = copy;
                        var localAscending = ascending;
                        var localBogoLimit = bogoLimit;
                        var localRecord = recordFrames;
                        var localSkip = skipSteps;
                        var localFrames = frames;

                        await Task.Run(() =>
                        {
                            Action<double[], int, int> onStep = (arr, a, b) =>
                            {
                                Interlocked.Increment(ref stepCount);

                                if (!localRecord) return;

                                frameCounter++;
                                if (frameCounter % localSkip != 0) return;

                                localFrames.Add(new Frame
                                {
                                    Data = (double[])arr.Clone(),
                                    A = a,
                                    B = b
                                });
                            };

                            localAlgo.Sort(localCopy, localAscending, onStep, localBogoLimit);
                        });
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        dgvStats.Rows.Add(
                            algo.Name,
                            input.Length.ToString(),
                            stepCount.ToString(),
                            sw.Elapsed.TotalMilliseconds.ToString("F2"),
                            "Ошибка: " + ex.Message);
                        continue;
                    }

                    sw.Stop();

                    // ==== Записываем ЧЕСТНОЕ время (без визуализации) ====
                    dgvStats.Rows.Add(
                        algo.Name,
                        input.Length.ToString(),
                        stepCount.ToString(),
                        sw.Elapsed.TotalMilliseconds.ToString("F2"),
                        "OK");

                    // ==== ЭТАП 2: ПРОИГРЫВАЕМ кадры ====
                    if (recordFrames && frames.Count > 0)
                    {
                        lblStatus.Text = "Визуализация: " + algo.Name +
                                         " (" + frames.Count + " кадров)...";
                        Application.DoEvents();

                        for (int f = 0; f < frames.Count; f++)
                        {
                            var frame = frames[f];
                            _visualizer.Draw(frame.Data, frame.A, frame.B);
                            panelViz.Invalidate();
                            panelViz.Update();

                            if (delayMs > 0)
                                await Task.Delay(delayMs);
                        }
                    }

                    // Финальный кадр — всегда
                    _visualizer.Draw(copy);
                    panelViz.Invalidate();
                    panelViz.Update();

                    lblStatus.Text = algo.Name + ": " +
                        sw.Elapsed.TotalMilliseconds.ToString("F2") + " мс, " +
                        stepCount + " итераций";

                    if (recordFrames && frames.Count > 0)
                        await Task.Delay(300);
                }

                // ==== Итог: кто быстрее ====
                var success = new List<KeyValuePair<string, double>>();
                for (int i = 0; i < dgvStats.Rows.Count; i++)
                {
                    var row = dgvStats.Rows[i];
                    if (row.Cells[4].Value != null && (string)row.Cells[4].Value == "OK")
                    {
                        double t = double.Parse((string)row.Cells[3].Value);
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