using System.Drawing;
using System.Windows.Forms;

namespace SortingVisualizer
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue)
        {
            var form = new Form();
            form.Width = 500;
            form.Height = 170;
            form.Text = caption;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ShowInTaskbar = false;

            var label = new Label();
            label.Left = 15;
            label.Top = 15;
            label.Text = text;
            label.Width = 460;
            label.Height = 20;

            var textBox = new TextBox();
            textBox.Left = 15;
            textBox.Top = 40;
            textBox.Width = 460;
            textBox.Text = defaultValue;

            var btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Left = 280;
            btnOk.Width = 95;
            btnOk.Top = 80;
            btnOk.DialogResult = DialogResult.OK;

            var btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Left = 380;
            btnCancel.Width = 95;
            btnCancel.Top = 80;
            btnCancel.DialogResult = DialogResult.Cancel;

            form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK
                ? textBox.Text.Trim()
                : null;
        }

        public static string ShowDialog(string text, string caption)
        {
            return ShowDialog(text, caption, "");
        }
    }
}