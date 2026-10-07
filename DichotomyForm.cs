using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SortingVisualizer.Services;
using SortingVisualizer.Visualization;

namespace SortingVisualizer
{
    public class DichotomyForm : Form
    {
        private TextBox txtA, txtB, txtEps, txtF;
        private Panel panelPlot;
        private Label lblStatus;
        private DataGridView dgvSteps;
        private MenuStrip menuStrip;
        private ToolStripMenuItem miCalculate, miStep, miAuto, miStop,
                                  miClear, miResetView, miExit;

        private FunctionParser _f;
        private double _a, _b, _eps;
        private PlotVisualizer _plot;

        private DichotomyResult _result;
        private int _currentStep = -1;

        private Timer _autoTimer;

        private bool _dragging = false;
        private Point _lastMouse;

        public DichotomyForm()
        {
            Text = "Метод дихотомии: поиск корня функции";
            Size = new Size(1200, 780);
            StartPosition = FormStartPosition.CenterParent;

            // ===== MenuStrip =====
            menuStrip = new MenuStrip();
            miCalculate = new ToolStripMenuItem("Рассчитать");
            miStep = new ToolStripMenuItem("Шаг вперёд");
            miAuto = new ToolStripMenuItem("Авто");
            miStop = new ToolStripMenuItem("Стоп");
            miClear = new ToolStripMenuItem("Очистить");
            miResetView = new ToolStripMenuItem("Сброс вида графика");
            miExit = new ToolStripMenuItem("Выход");

            menuStrip.Items.AddRange(new ToolStripItem[] {
                miCalculate, miStep, miAuto, miStop,
                miClear, miResetView, miExit });
            menuStrip.Location = new Point(0, 0);
            MainMenuStrip = menuStrip;

            miCalculate.Click += MiCalculate_Click;
            miStep.Click += (s, e) => DoOneStep();
            miAuto.Click += MiAuto_Click;
            miStop.Click += (s, e) => StopAuto();
            miClear.Click += MiClear_Click;
            miResetView.Click += (s, e) =>
            {
                _plot.ResetView();
                _plot.Draw();
                panelPlot.Invalidate();
            };
            miExit.Click += (s, e) => Close();

            // ===== Поля ввода =====
            var lblA = new Label { Text = "a:", Location = new Point(20, 40), Size = new Size(30, 22) };
            txtA = new TextBox { Text = "-5", Location = new Point(55, 38), Size = new Size(100, 22) };

            var lblB = new Label { Text = "b:", Location = new Point(175, 40), Size = new Size(30, 22) };
            txtB = new TextBox { Text = "5", Location = new Point(210, 38), Size = new Size(100, 22) };

            var lblEps = new Label { Text = "ε:", Location = new Point(330, 40), Size = new Size(30, 22) };
            txtEps = new TextBox { Text = "0,0001", Location = new Point(360, 38), Size = new Size(100, 22) };

            var lblF = new Label { Text = "f(x):", Location = new Point(480, 40), Size = new Size(40, 22) };
            txtF = new TextBox
            {
                Text = "x^2 + 2*x - 6",
                Location = new Point(525, 38),
                Size = new Size(500, 22)
            };

            var lblHint = new Label
            {
                Text = "Поддерживается: + - * / ^, sin cos tg ln lg exp sqrt abs, pi, e. " +
                       "Колесо мыши — зум, перетаскивание — сдвиг, двойной клик — сброс. " +
                       "«Рассчитать» — найти корень. «Шаг вперёд» / «Авто» — пошаговый просмотр.",
                Location = new Point(20, 70),
                Size = new Size(1150, 30),
                ForeColor = Color.Gray
            };

            // ===== Панель графика =====
            panelPlot = new Panel();
            panelPlot.Location = new Point(20, 105);
            panelPlot.Size = new Size(760, 590);
            panelPlot.BorderStyle = BorderStyle.FixedSingle;
            panelPlot.BackColor = Color.White;

            // ===== Таблица итераций =====
            dgvSteps = new DataGridView();
            dgvSteps.Location = new Point(795, 105);
            dgvSteps.Size = new Size(380, 560);
            dgvSteps.ReadOnly = true;
            dgvSteps.AllowUserToAddRows = false;
            dgvSteps.RowHeadersVisible = false;
            dgvSteps.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvSteps.Columns.Add("N", "№");
            dgvSteps.Columns.Add("A", "a");
            dgvSteps.Columns.Add("B", "b");
            dgvSteps.Columns.Add("C", "c");
            dgvSteps.Columns.Add("Fc", "f(c)");
            dgvSteps.Columns.Add("Keep", "Оставить");

            dgvSteps.Columns[0].FillWeight = 10;
            dgvSteps.Columns[1].FillWeight = 18;
            dgvSteps.Columns[2].FillWeight = 18;
            dgvSteps.Columns[3].FillWeight = 18;
            dgvSteps.Columns[4].FillWeight = 20;
            dgvSteps.Columns[5].FillWeight = 16;

            // ===== Статус =====
            lblStatus = new Label();
            lblStatus.Location = new Point(20, 705);
            lblStatus.Size = new Size(1155, 24);
            lblStatus.Text = "Введите параметры и нажмите «Рассчитать».";
            lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            Controls.AddRange(new Control[] {
                menuStrip,
                lblA, txtA, lblB, txtB, lblEps, txtEps,
                lblF, txtF, lblHint,
                panelPlot, dgvSteps, lblStatus });

            _plot = new PlotVisualizer(panelPlot);

            // ===== Мышиные события =====
            panelPlot.MouseWheel += PanelPlot_MouseWheel;
            panelPlot.MouseDown += PanelPlot_MouseDown;
            panelPlot.MouseMove += PanelPlot_MouseMove;
            panelPlot.MouseUp += PanelPlot_MouseUp;
            panelPlot.MouseDoubleClick += (s, e) =>
            {
                _plot.ResetView();
                _plot.Draw();
                panelPlot.Invalidate();
            };

            // ===== Таймер авто =====
            _autoTimer = new Timer();
            _autoTimer.Interval = 400;
            _autoTimer.Tick += (s, e) => DoOneStep();

            miStep.Enabled = false;
            miAuto.Enabled = false;
            miStop.Enabled = false;
        }

        // ===================== ЗУМ И ПАНОРАМА =====================

        private void PanelPlot_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0) return;
            double factor = e.Delta > 0 ? 0.85 : 1.0 / 0.85;
            _plot.ZoomAt(e.X, e.Y, factor);
            _plot.Draw();
            panelPlot.Invalidate();
        }

        private void PanelPlot_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _lastMouse = e.Location;
            }
        }

        private void PanelPlot_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            float dx = e.X - _lastMouse.X;
            float dy = e.Y - _lastMouse.Y;
            _plot.Pan(dx, dy);
            _lastMouse = e.Location;
            _plot.Draw();
            panelPlot.Invalidate();
        }

        private void PanelPlot_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        // ===================== РАСЧЁТ =====================

        private void MiCalculate_Click(object sender, EventArgs e)
        {
            StopAuto();

            if (!TryParseDouble(txtA.Text, out _a))
            {
                MessageBox.Show("a — не число.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!TryParseDouble(txtB.Text, out _b))
            {
                MessageBox.Show("b — не число.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!TryParseDouble(txtEps.Text, out _eps) || _eps <= 0)
            {
                MessageBox.Show("ε должно быть положительным числом.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_a >= _b)
            {
                MessageBox.Show("Должно быть a < b.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _f = new FunctionParser(txtF.Text);
                _f.Evaluate((_a + _b) / 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в формуле: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _result = DichotomySolver.Solve(_f, _a, _b, _eps);
            _currentStep = -1;

            double? rootToShow = _result.Success ? (double?)_result.Root : null;
            _plot.FitToData(_f, _a, _b, rootToShow);
            _plot.SetSegment(null, null, null);
            _plot.Draw();
            panelPlot.Invalidate();

            dgvSteps.Rows.Clear();

            if (_result.Success)
            {
                foreach (var st in _result.Steps)
                {
                    dgvSteps.Rows.Add(
                        st.Number,
                        st.A.ToString("F5"),
                        st.B.ToString("F5"),
                        st.C.ToString("F5"),
                        st.Fc.ToString("F5"),
                        st.Keep);
                }

                int digits = Math.Max(1, (int)Math.Ceiling(-Math.Log10(_eps)));
                lblStatus.Text = "Корень найден: x = " +
                                 _result.Root.ToString("F" + digits) +
                                 "   (итераций: " + _result.Iterations + ")";
                lblStatus.ForeColor = Color.DarkGreen;

                miStep.Enabled = _result.Steps.Count > 0;
                miAuto.Enabled = _result.Steps.Count > 0;
                miStop.Enabled = false;
            }
            else
            {
                lblStatus.Text = _result.Message;
                lblStatus.ForeColor = Color.DarkRed;
                MessageBox.Show(_result.Message, "Результат",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                miStep.Enabled = false;
                miAuto.Enabled = false;
                miStop.Enabled = false;
            }
        }

        // ===================== ПОШАГОВОЕ ВЫПОЛНЕНИЕ =====================

        private void DoOneStep()
        {
            if (_result == null || !_result.Success) return;
            if (_result.Steps.Count == 0) return;

            _currentStep++;

            if (_currentStep >= _result.Steps.Count)
            {
                StopAuto();

                lblStatus.Text = "Просмотр шагов завершён. Корень: x = " +
                                 _result.Root.ToString("F5");
                lblStatus.ForeColor = Color.DarkGreen;
                return;
            }

            var st = _result.Steps[_currentStep];

            _plot.SetSegment(st.A, st.B, st.C);
            _plot.Draw();
            panelPlot.Invalidate();

            // выделяем строку в таблице
            if (_currentStep < dgvSteps.Rows.Count)
            {
                dgvSteps.ClearSelection();
                dgvSteps.Rows[_currentStep].Selected = true;
                dgvSteps.FirstDisplayedScrollingRowIndex = _currentStep;
            }

            lblStatus.Text = "Шаг " + (_currentStep + 1) + " из " +
                             _result.Steps.Count + ":  " +
                             "a = " + st.A.ToString("F5") +
                             ", b = " + st.B.ToString("F5") +
                             ", c = " + st.C.ToString("F5") +
                             ", f(c) = " + st.Fc.ToString("F5") +
                             "  →  " + st.Keep;
            lblStatus.ForeColor = Color.Black;

            if (_currentStep >= _result.Steps.Count - 1)
                StopAuto();
        }

        private void MiAuto_Click(object sender, EventArgs e)
        {
            if (_result == null || !_result.Success) return;

            if (_currentStep >= _result.Steps.Count - 1)
                _currentStep = -1;

            _autoTimer.Start();
            miStop.Enabled = true;
            miAuto.Enabled = false;
            miStep.Enabled = false;
        }

        private void StopAuto()
        {
            _autoTimer.Stop();
            miStop.Enabled = false;
            if (_result != null && _result.Success && _result.Steps.Count > 0)
            {
                miAuto.Enabled = true;
                miStep.Enabled = true;
            }
        }

        // ===================== ОЧИСТКА =====================

        private void MiClear_Click(object sender, EventArgs e)
        {
            StopAuto();
            txtA.Text = "-5";
            txtB.Text = "5";
            txtEps.Text = "0,0001";
            txtF.Text = "x^2 + 2*x - 6";

            dgvSteps.Rows.Clear();
            _result = null;
            _currentStep = -1;
            _plot.SetSegment(null, null, null);
            panelPlot.BackgroundImage = null;
            panelPlot.Invalidate();

            lblStatus.Text = "Очищено.";
            lblStatus.ForeColor = Color.Black;

            miStep.Enabled = false;
            miAuto.Enabled = false;
            miStop.Enabled = false;
        }

        // ===================== УТИЛИТЫ =====================

        private static bool TryParseDouble(string s, out double v)
        {
            s = (s ?? "").Trim();
            if (s.IndexOf(' ') >= 0) { v = 0; return false; }
            s = s.Replace('.', ',');
            return double.TryParse(s, NumberStyles.Float,
                CultureInfo.CurrentCulture, out v);
        }
    }
}