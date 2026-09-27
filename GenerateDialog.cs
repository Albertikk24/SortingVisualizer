using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SortingVisualizer
{
    public class GenerateDialog : Form
    {
        private NumericUpDown nudCount;
        private NumericUpDown nudDecimals;
        private TextBox txtMin;
        private TextBox txtMax;
        private Button btnOk;
        private Button btnCancel;

        public int Count { get { return (int)nudCount.Value; } }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public int Decimals { get { return (int)nudDecimals.Value; } }

        public GenerateDialog()
        {
            MinValue = 0;
            MaxValue = 1;

            Text = "Генерация данных";
            Size = new Size(340, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            var lbl1 = new Label();
            lbl1.Text = "Количество:";
            lbl1.Location = new Point(15, 20);
            lbl1.Width = 110;

            nudCount = new NumericUpDown();
            nudCount.Location = new Point(160, 18);
            nudCount.Minimum = 1;
            nudCount.Maximum = 200000;
            nudCount.Value = 20;

            var lbl2 = new Label();
            lbl2.Text = "Минимум:";
            lbl2.Location = new Point(15, 60);
            lbl2.Width = 110;

            txtMin = new TextBox();
            txtMin.Text = "-1";
            txtMin.Location = new Point(160, 58);
            txtMin.Width = 150;

            var lbl3 = new Label();
            lbl3.Text = "Максимум:";
            lbl3.Location = new Point(15, 95);
            lbl3.Width = 110;

            txtMax = new TextBox();
            txtMax.Text = "0";
            txtMax.Location = new Point(160, 93);
            txtMax.Width = 150;

            var lbl4 = new Label();
            lbl4.Text = "Знаков после запятой:";
            lbl4.Location = new Point(15, 130);
            lbl4.Width = 140;

            nudDecimals = new NumericUpDown();
            nudDecimals.Location = new Point(160, 128);
            nudDecimals.Minimum = 0;
            nudDecimals.Maximum = 8;
            nudDecimals.Value = 4;

            btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Location = new Point(80, 175);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(180, 175);
            btnCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] {
                lbl1, nudCount, lbl2, txtMin, lbl3, txtMax,
                lbl4, nudDecimals, btnOk, btnCancel });

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            double mn, mx;
            if (!double.TryParse(txtMin.Text.Trim().Replace('.', ','),
                    NumberStyles.Any, CultureInfo.CurrentCulture, out mn))
            {
                MessageBox.Show("Минимум — не число.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (!double.TryParse(txtMax.Text.Trim().Replace('.', ','),
                    NumberStyles.Any, CultureInfo.CurrentCulture, out mx))
            {
                MessageBox.Show("Максимум — не число.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (mn >= mx)
            {
                MessageBox.Show("Минимум должен быть меньше максимума.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            MinValue = mn;
            MaxValue = mx;
        }
    }
}