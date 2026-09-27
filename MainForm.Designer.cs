namespace SortingVisualizer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miLoadExcel;
        private System.Windows.Forms.ToolStripMenuItem miLoadGoogle;
        private System.Windows.Forms.ToolStripMenuItem miGenerate;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem miCalculate;
        private System.Windows.Forms.ToolStripMenuItem miClear;
        private System.Windows.Forms.ToolStripMenuItem miToggleView;

        private System.Windows.Forms.DataGridView dgvInput;
        private System.Windows.Forms.Panel panelViz;

        private System.Windows.Forms.GroupBox grpAlgorithms;
        private System.Windows.Forms.CheckBox cbBubble;
        private System.Windows.Forms.CheckBox cbInsertion;
        private System.Windows.Forms.CheckBox cbShaker;
        private System.Windows.Forms.CheckBox cbQuick;
        private System.Windows.Forms.CheckBox cbBogo;

        private System.Windows.Forms.GroupBox grpDirection;
        private System.Windows.Forms.RadioButton rbAscending;
        private System.Windows.Forms.RadioButton rbDescending;

        private System.Windows.Forms.GroupBox grpBogo;
        private System.Windows.Forms.Label lblBogoLimit;
        private System.Windows.Forms.TextBox txtBogoLimit;

        private System.Windows.Forms.GroupBox grpSpeed;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown nudDelay;
        private System.Windows.Forms.Label lblDelayHint;
        private System.Windows.Forms.Label lblSkip;
        private System.Windows.Forms.NumericUpDown nudSkip;
        private System.Windows.Forms.Label lblSkipHint;

        private System.Windows.Forms.DataGridView dgvStats;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miLoadExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.miLoadGoogle = new System.Windows.Forms.ToolStripMenuItem();
            this.miGenerate = new System.Windows.Forms.ToolStripMenuItem();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.miCalculate = new System.Windows.Forms.ToolStripMenuItem();
            this.miClear = new System.Windows.Forms.ToolStripMenuItem();
            this.miToggleView = new System.Windows.Forms.ToolStripMenuItem();

            this.dgvInput = new System.Windows.Forms.DataGridView();
            this.panelViz = new System.Windows.Forms.Panel();

            this.grpAlgorithms = new System.Windows.Forms.GroupBox();
            this.cbBubble = new System.Windows.Forms.CheckBox();
            this.cbInsertion = new System.Windows.Forms.CheckBox();
            this.cbShaker = new System.Windows.Forms.CheckBox();
            this.cbQuick = new System.Windows.Forms.CheckBox();
            this.cbBogo = new System.Windows.Forms.CheckBox();

            this.grpDirection = new System.Windows.Forms.GroupBox();
            this.rbAscending = new System.Windows.Forms.RadioButton();
            this.rbDescending = new System.Windows.Forms.RadioButton();

            this.grpBogo = new System.Windows.Forms.GroupBox();
            this.lblBogoLimit = new System.Windows.Forms.Label();
            this.txtBogoLimit = new System.Windows.Forms.TextBox();

            this.grpSpeed = new System.Windows.Forms.GroupBox();
            this.lblDelay = new System.Windows.Forms.Label();
            this.nudDelay = new System.Windows.Forms.NumericUpDown();
            this.lblDelayHint = new System.Windows.Forms.Label();
            this.lblSkip = new System.Windows.Forms.Label();
            this.nudSkip = new System.Windows.Forms.NumericUpDown();
            this.lblSkipHint = new System.Windows.Forms.Label();

            this.dgvStats = new System.Windows.Forms.DataGridView();
            this.lblStatus = new System.Windows.Forms.Label();

            // ================= menuStrip =================
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miFile, this.miCalculate, this.miClear, this.miToggleView, this.miExit });
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(960, 24);
            this.menuStrip.TabIndex = 0;

            this.miFile.Text = "Файл";
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miLoadExcel, this.miLoadGoogle, this.miGenerate });

            this.miLoadExcel.Text = "Загрузить из Excel / CSV...";
            this.miLoadExcel.Click += new System.EventHandler(this.MiLoadExcel_Click);

            this.miLoadGoogle.Text = "Загрузить из Google Sheets...";
            this.miLoadGoogle.Click += new System.EventHandler(this.MiLoadGoogle_Click);

            this.miGenerate.Text = "Сгенерировать данные...";
            this.miGenerate.Click += new System.EventHandler(this.MiGenerate_Click);

            this.miCalculate.Text = "Рассчитать";
            this.miCalculate.Click += new System.EventHandler(this.MiCalculate_Click);

            this.miClear.Text = "Очистить";
            this.miClear.Click += new System.EventHandler(this.MiClear_Click);

            this.miToggleView.Text = "Режим: каждая итерация";
            this.miToggleView.Click += new System.EventHandler(this.MiToggleView_Click);

            this.miExit.Text = "Выход";
            this.miExit.Click += new System.EventHandler(this.MiExit_Click);

            // ================= dgvInput =================
            this.dgvInput.Location = new System.Drawing.Point(12, 30);
            this.dgvInput.Size = new System.Drawing.Size(240, 400);
            this.dgvInput.AllowUserToAddRows = true;
            this.dgvInput.ColumnHeadersVisible = false;
            this.dgvInput.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInput.Name = "dgvInput";
            this.dgvInput.TabIndex = 1;
            this.dgvInput.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn { Name = "Value" });

            // ================= panelViz =================
            this.panelViz.Location = new System.Drawing.Point(260, 30);
            this.panelViz.Size = new System.Drawing.Size(680, 400);
            this.panelViz.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelViz.BackColor = System.Drawing.Color.White;
            this.panelViz.Name = "panelViz";
            this.panelViz.TabIndex = 2;

            // ================= grpAlgorithms =================
            this.grpAlgorithms.Text = "Алгоритмы";
            this.grpAlgorithms.Location = new System.Drawing.Point(12, 440);
            this.grpAlgorithms.Size = new System.Drawing.Size(200, 125);
            this.grpAlgorithms.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.cbBubble, this.cbInsertion, this.cbShaker, this.cbQuick, this.cbBogo });

            this.cbBubble.Text = "Пузырьковая";
            this.cbBubble.Location = new System.Drawing.Point(10, 20);
            this.cbInsertion.Text = "Вставками";
            this.cbInsertion.Location = new System.Drawing.Point(10, 40);
            this.cbShaker.Text = "Шейкерная";
            this.cbShaker.Location = new System.Drawing.Point(10, 60);
            this.cbQuick.Text = "Быстрая";
            this.cbQuick.Location = new System.Drawing.Point(10, 80);
            this.cbBogo.Text = "BOGO";
            this.cbBogo.Location = new System.Drawing.Point(10, 100);

            // ================= grpDirection =================
            this.grpDirection.Text = "Направление";
            this.grpDirection.Location = new System.Drawing.Point(220, 440);
            this.grpDirection.Size = new System.Drawing.Size(180, 80);
            this.grpDirection.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.rbAscending, this.rbDescending });

            this.rbAscending.Text = "По возрастанию";
            this.rbAscending.Location = new System.Drawing.Point(10, 20);
            this.rbAscending.Size = new System.Drawing.Size(160, 20);
            this.rbAscending.Checked = true;
            this.rbDescending.Text = "По убыванию";
            this.rbDescending.Location = new System.Drawing.Point(10, 45);
            this.rbDescending.Size = new System.Drawing.Size(160, 20);

            // ================= grpBogo =================
            this.grpBogo.Text = "Лимит итераций BOGO";
            this.grpBogo.Location = new System.Drawing.Point(410, 440);
            this.grpBogo.Size = new System.Drawing.Size(200, 80);
            this.grpBogo.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblBogoLimit, this.txtBogoLimit });

            this.lblBogoLimit.Text = "Итераций:";
            this.lblBogoLimit.Location = new System.Drawing.Point(10, 25);
            this.lblBogoLimit.Width = 70;
            this.txtBogoLimit.Text = "10000";
            this.txtBogoLimit.Location = new System.Drawing.Point(85, 22);
            this.txtBogoLimit.Width = 100;

            // ================= grpSpeed =================
            this.grpSpeed.Text = "Скорость визуализации";
            this.grpSpeed.Location = new System.Drawing.Point(620, 440);
            this.grpSpeed.Size = new System.Drawing.Size(320, 120);
            this.grpSpeed.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDelay, this.nudDelay, this.lblDelayHint,
                this.lblSkip, this.nudSkip, this.lblSkipHint });

            this.lblDelay.Text = "Задержка между кадрами (мс):";
            this.lblDelay.Location = new System.Drawing.Point(10, 25);
            this.lblDelay.Size = new System.Drawing.Size(180, 20);

            this.nudDelay.Location = new System.Drawing.Point(200, 23);
            this.nudDelay.Size = new System.Drawing.Size(70, 20);
            this.nudDelay.Minimum = 0;
            this.nudDelay.Maximum = 2000;
            this.nudDelay.Increment = 10;
            this.nudDelay.Value = 100;

            this.lblDelayHint.Text = "0 — быстро, 100 — видно шаги, 500 — медленно";
            this.lblDelayHint.Location = new System.Drawing.Point(10, 45);
            this.lblDelayHint.Size = new System.Drawing.Size(300, 16);
            this.lblDelayHint.ForeColor = System.Drawing.Color.Gray;

            this.lblSkip.Text = "Пропускать шагов:";
            this.lblSkip.Location = new System.Drawing.Point(10, 70);
            this.lblSkip.Size = new System.Drawing.Size(180, 20);

            this.nudSkip.Location = new System.Drawing.Point(200, 68);
            this.nudSkip.Size = new System.Drawing.Size(70, 20);
            this.nudSkip.Minimum = 1;
            this.nudSkip.Maximum = 10000;
            this.nudSkip.Increment = 1;
            this.nudSkip.Value = 1;

            this.lblSkipHint.Text = "1 — каждый шаг, 10 — каждый 10-й";
            this.lblSkipHint.Location = new System.Drawing.Point(10, 90);
            this.lblSkipHint.Size = new System.Drawing.Size(300, 16);
            this.lblSkipHint.ForeColor = System.Drawing.Color.Gray;

            // ================= dgvStats =================
            this.dgvStats.Location = new System.Drawing.Point(12, 570);
            this.dgvStats.Size = new System.Drawing.Size(928, 160);
            this.dgvStats.ReadOnly = true;
            this.dgvStats.AllowUserToAddRows = false;
            this.dgvStats.RowHeadersVisible = false;
            this.dgvStats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStats.Name = "dgvStats";
            this.dgvStats.Columns.Add("Algo", "Алгоритм");
            this.dgvStats.Columns.Add("Count", "Элементов");
            this.dgvStats.Columns.Add("Time", "Время, мс");
            this.dgvStats.Columns.Add("Status", "Статус");

            this.dgvStats.Columns[0].FillWeight = 30;
            this.dgvStats.Columns[1].FillWeight = 20;
            this.dgvStats.Columns[2].FillWeight = 20;
            this.dgvStats.Columns[3].FillWeight = 60;

            // ================= lblStatus =================
            this.lblStatus.Location = new System.Drawing.Point(12, 740);
            this.lblStatus.Size = new System.Drawing.Size(928, 20);
            this.lblStatus.Text = "Готово";
            this.lblStatus.Name = "lblStatus";

            // ================= MainForm =================
            this.ClientSize = new System.Drawing.Size(960, 770);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.dgvInput);
            this.Controls.Add(this.panelViz);
            this.Controls.Add(this.grpAlgorithms);
            this.Controls.Add(this.grpDirection);
            this.Controls.Add(this.grpBogo);
            this.Controls.Add(this.grpSpeed);
            this.Controls.Add(this.dgvStats);
            this.Controls.Add(this.lblStatus);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Сортировки: визуализация и сравнение";
        }
    }
}