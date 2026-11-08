using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SortingVisualizer.Services;
using SortingVisualizer.Visualization;

namespace SortingVisualizer
{
    public class NewtonForm : Form
    {
        private TextBox txtA, txtB, txtEps, txtF, txtStartX;
        private Panel panelPlot;
        private Label lblStatus;
        private DataGridView dgvSteps;
        private MenuStrip menuStrip;
        private ToolStripMenuItem miCalculate, miStep, miAuto, miStop, miClear, miResetView, miExit;

        private RadioButton rbMin, rbMax, rbRoot;
        private GroupBox grpMode;

        private FunctionParser _f;
        private double _a, _b, _eps, _startX;
        private PlotVisualizer _plot;

        private NewtonResult _result;
        private int _currentStep = -1;

        private Timer _autoTimer;
        private bool _dragging = false;
        private Point _lastMouse;

        public NewtonForm()
        {
            Text = "Метод Ньютона: поиск экстремумов и корней";
            Size = new Size(1200, 840);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1200, 840); // Запрещаем делать меньше, чтобы не ломалось

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
                miCalculate, miStep, miAuto, miStop, miClear, miResetView, miExit });
            menuStrip.Location = new Point(0, 0);
            MainMenuStrip = menuStrip;

            miCalculate.Click += MiCalculate_Click;
            miStep.Click += (s, e) => DoOneStep();
            miAuto.Click += MiAuto_Click;
            miStop.Click += (s, e) => StopAuto();
            miClear.Click += MiClear_Click;
            miResetView.Click += (s, e) => { _plot.ResetView(); _plot.Draw(); panelPlot.Invalidate(); };
            miExit.Click += (s, e) => Close();

            // ===== Поля ввода (Верхний ряд) =====
            int yRow = 45; // Единая высота для всех полей
            int xOffset = 20; // Начальный отступ слева

            var lblA = new Label { Text = "a:", Location = new Point(xOffset, yRow), Size = new Size(20, 22) };
            txtA = new TextBox { Text = "-5", Location = new Point(xOffset + 25, yRow), Size = new Size(70, 22) };

            var lblB = new Label { Text = "b:", Location = new Point(xOffset + 110, yRow), Size = new Size(20, 22) };
            txtB = new TextBox { Text = "5", Location = new Point(xOffset + 135, yRow), Size = new Size(70, 22) };

            var lblEps = new Label { Text = "ε:", Location = new Point(xOffset + 220, yRow), Size = new Size(20, 22) };
            txtEps = new TextBox { Text = "0,0001", Location = new Point(xOffset + 245, yRow), Size = new Size(70, 22) };

            var lblStart = new Label { Text = "x₀:", Location = new Point(xOffset + 330, yRow), Size = new Size(25, 22) };
            txtStartX = new TextBox { Text = "3", Location = new Point(xOffset + 360, yRow), Size = new Size(70, 22) };

            var lblF = new Label { Text = "f(x):", Location = new Point(xOffset + 450, yRow), Size = new Size(35, 22) };
            txtF = new TextBox { Text = "x^2 + 2*x - 6", Location = new Point(xOffset + 490, yRow), Size = new Size(530, 22) };

            // ===== Режимы =====
            grpMode = new GroupBox { Text = "Что искать", Location = new Point(20, 85), Size = new Size(400, 60) };
            rbMin = new RadioButton { Text = "Минимум", Location = new Point(15, 25), Size = new Size(100, 22), Checked = true };
            rbMax = new RadioButton { Text = "Максимум", Location = new Point(130, 25), Size = new Size(110, 22) };
            rbRoot = new RadioButton { Text = "Корень", Location = new Point(255, 25), Size = new Size(100, 22) };
            grpMode.Controls.AddRange(new Control[] { rbMin, rbMax, rbRoot });

            var lblHint = new Label
            {
                Text = "Поддерживается: + - * / ^, sin cos tg ln lg exp sqrt abs, pi, e. " +
                       "Метод Ньютона очень чувствителен к выбору x₀.",
                Location = new Point(430, 100),
                Size = new Size(730, 40),
                ForeColor = Color.Gray
            };

            // ===== Панель графика =====
            panelPlot = new Panel();
            panelPlot.Location = new Point(20, 155); // Опустили ниже
            panelPlot.Size = new Size(760, 570);     // Немного уменьшили по высоте
            panelPlot.BorderStyle = BorderStyle.FixedSingle;
            panelPlot.BackColor = Color.White;

            // ===== Таблица =====
            dgvSteps = new DataGridView();
            dgvSteps.Location = new Point(795, 155); // Выровняли по высоте с графиком
            dgvSteps.Size = new Size(380, 570);
            dgvSteps.ReadOnly = true;
            dgvSteps.AllowUserToAddRows = false;
            dgvSteps.RowHeadersVisible = false;
            dgvSteps.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvSteps.Columns.Add("N", "№");
            dgvSteps.Columns.Add("X", "x");
            dgvSteps.Columns.Add("Fx", "f(x)");
            dgvSteps.Columns.Add("D1", "f'(x)");
            dgvSteps.Columns.Add("NextX", "След. x");

            dgvSteps.Columns[0].FillWeight = 10;
            dgvSteps.Columns[1].FillWeight = 20;
            dgvSteps.Columns[2].FillWeight = 20;
            dgvSteps.Columns[3].FillWeight = 20;
            dgvSteps.Columns[4].FillWeight = 30;

            // ===== Статус =====
            lblStatus = new Label();
            lblStatus.Location = new Point(20, 740); // В самом низу
            lblStatus.Size = new Size(1155, 40);
            lblStatus.Text = "Введите параметры, x₀, выберите режим и нажмите «Рассчитать».";
            lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            Controls.AddRange(new Control[] {
                menuStrip,
                lblA, txtA, lblB, txtB, lblEps, txtEps, lblStart, txtStartX, lblF, txtF,
                grpMode, lblHint, panelPlot, dgvSteps, lblStatus });

            _plot = new PlotVisualizer(panelPlot);

            // ===== Мышь =====
            panelPlot.MouseWheel += PanelPlot_MouseWheel;
            panelPlot.MouseDown += PanelPlot_MouseDown;
            panelPlot.MouseMove += PanelPlot_MouseMove;
            panelPlot.MouseUp += PanelPlot_MouseUp;
            panelPlot.MouseDoubleClick += (s, e) => { _plot.ResetView(); _plot.Draw(); panelPlot.Invalidate(); };

            // ===== Авто =====
            _autoTimer = new Timer();
            _autoTimer.Interval = 500;
            _autoTimer.Tick += (s, e) => DoOneStep();

            miStep.Enabled = false;
            miAuto.Enabled = false;
            miStop.Enabled = false;
        }

        private OptimizeMode CurrentMode()
        {
            if (rbMax.Checked) return OptimizeMode.Maximum;
            if (rbRoot.Checked) return OptimizeMode.Root;
            return OptimizeMode.Minimum;
        }

        private void PanelPlot_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0) return;
            _plot.ZoomAt(e.X, e.Y, e.Delta > 0 ? 0.85 : 1.0 / 0.85);
            _plot.Draw(); panelPlot.Invalidate();
        }
        private void PanelPlot_MouseDown(object sender, MouseEventArgs e)
        { if (e.Button == MouseButtons.Left) { _dragging = true; _lastMouse = e.Location; } }
        private void PanelPlot_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _plot.Pan(e.X - _lastMouse.X, e.Y - _lastMouse.Y);
            _lastMouse = e.Location; _plot.Draw(); panelPlot.Invalidate();
        }
        private void PanelPlot_MouseUp(object sender, MouseEventArgs e) { _dragging = false; }

        private void MiCalculate_Click(object sender, EventArgs e)
        {
            StopAuto();

            if (!TryParseDouble(txtA.Text, out _a)) { MessageBox.Show("a — не число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!TryParseDouble(txtB.Text, out _b)) { MessageBox.Show("b — не число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!TryParseDouble(txtEps.Text, out _eps) || _eps <= 0) { MessageBox.Show("ε должно быть положительным.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!TryParseDouble(txtStartX.Text, out _startX)) { MessageBox.Show("x₀ — не число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_a >= _b) { MessageBox.Show("a должно быть меньше b.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_startX <= _a || _startX >= _b) { MessageBox.Show("x₀ должен быть внутри интервала (a, b).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                _f = new FunctionParser(txtF.Text);
                _f.Evaluate((_a + _b) / 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в формуле: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var mode = CurrentMode();
            _result = NewtonSolver.Solve(_f, _a, _b, _eps, mode, _startX);
            _currentStep = -1;

            double? rootToShow = _result.Success ? (double?)_result.Xmin : null;
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
                        st.X.ToString("F5"),
                        st.Fx.ToString("F5"),
                        st.Derivative.ToString("F5"),
                        st.NextX.ToString("F5"));
                }

                lblStatus.Text = _result.Message;
                lblStatus.ForeColor = Color.DarkGreen;
                miStep.Enabled = _result.Steps.Count > 0;
                miAuto.Enabled = _result.Steps.Count > 0;
            }
            else
            {
                lblStatus.Text = _result.Message;
                lblStatus.ForeColor = Color.DarkRed;
                MessageBox.Show(_result.Message, "Ошибка метода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                miStep.Enabled = false;
                miAuto.Enabled = false;
            }
            miStop.Enabled = false;
        }

        private void DoOneStep()
        {
            if (_result == null || !_result.Success || _result.Steps.Count == 0) return;

            _currentStep++;
            if (_currentStep >= _result.Steps.Count)
            {
                StopAuto();
                lblStatus.Text = "Просмотр завершен.";
                return;
            }

            var st = _result.Steps[_currentStep];

            _plot.SetSegment(_a, _b, st.X);
            _plot.Draw();
            panelPlot.Invalidate();

            if (_currentStep < dgvSteps.Rows.Count)
            {
                dgvSteps.ClearSelection();
                dgvSteps.Rows[_currentStep].Selected = true;
                dgvSteps.FirstDisplayedScrollingRowIndex = _currentStep;
            }

            lblStatus.Text = $"Шаг {_currentStep + 1}: x = {st.X:F5}, f(x) = {st.Fx:F5} → следующий x = {st.NextX:F5}";
            lblStatus.ForeColor = Color.Black;

            if (_currentStep >= _result.Steps.Count - 1) StopAuto();
        }

        private void MiAuto_Click(object sender, EventArgs e)
        {
            if (_result == null || !_result.Success) return;
            if (_currentStep >= _result.Steps.Count - 1) _currentStep = -1;
            _autoTimer.Start();
            miStop.Enabled = true; miAuto.Enabled = false; miStep.Enabled = false;
        }

        private void StopAuto()
        {
            _autoTimer.Stop();
            miStop.Enabled = false;
            if (_result != null && _result.Success && _result.Steps.Count > 0)
            {
                miAuto.Enabled = true; miStep.Enabled = true;
            }
        }

        private void MiClear_Click(object sender, EventArgs e)
        {
            StopAuto();
            txtA.Text = "-5"; txtB.Text = "5"; txtEps.Text = "0,0001"; txtStartX.Text = "3"; txtF.Text = "x^2 + 2*x - 6";
            rbMin.Checked = true;
            dgvSteps.Rows.Clear(); _result = null; _currentStep = -1;
            _plot.SetSegment(null, null, null);
            panelPlot.BackgroundImage = null; panelPlot.Invalidate();
            lblStatus.Text = "Очищено."; lblStatus.ForeColor = Color.Black;
            miStep.Enabled = false; miAuto.Enabled = false; miStop.Enabled = false;
        }

        private static bool TryParseDouble(string s, out double v)
        {
            s = (s ?? "").Trim();
            if (s.IndexOf(' ') >= 0) { v = 0; return false; }
            s = s.Replace('.', ',');
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out v);
        }
    }
}