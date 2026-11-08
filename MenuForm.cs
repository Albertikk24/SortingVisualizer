using System;
using System.Drawing;
using System.Windows.Forms;

namespace SortingVisualizer
{
    public class MenuForm : Form
    {
        public MenuForm()
        {
            Text = "Выбор задачи";
            Size = new Size(520, 460); // Увеличили высоту для новой кнопки
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var lblTitle = new Label();
            lblTitle.Text = "Выберите, что запустить:";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(460, 30);

            var btnSort = new Button();
            btnSort.Text = "Сортировки: визуализация и сравнение";
            btnSort.Location = new Point(20, 70);
            btnSort.Size = new Size(460, 50);
            btnSort.Click += (s, e) => RunForm(new MainForm());

            var btnDich = new Button();
            btnDich.Text = "Метод дихотомии: поиск корня функции";
            btnDich.Location = new Point(20, 135);
            btnDich.Size = new Size(460, 50);
            btnDich.Click += (s, e) => RunForm(new DichotomyForm());

            var btnGolden = new Button();
            btnGolden.Text = "Метод золотого сечения: поиск минимума";
            btnGolden.Location = new Point(20, 200);
            btnGolden.Size = new Size(460, 50);
            btnGolden.Click += (s, e) => RunForm(new GoldenSectionForm());

            var btnNewton = new Button();
            btnNewton.Text = "Метод Ньютона: экстремумы и корни";
            btnNewton.Location = new Point(20, 265);
            btnNewton.Size = new Size(460, 50);
            btnNewton.Click += (s, e) => RunForm(new NewtonForm());

            var btnExit = new Button();
            btnExit.Text = "Выход";
            btnExit.Location = new Point(360, 360);
            btnExit.Size = new Size(120, 40);
            btnExit.Click += (s, e) => Close();

            Controls.AddRange(new Control[] {
                lblTitle, btnSort, btnDich, btnGolden, btnNewton, btnExit });
        }

        private void RunForm(Form f)
        {
            Hide();
            using (f)
            {
                f.FormClosed += (s2, e2) => Show();
                f.ShowDialog();
            }
            Show();
        }
    }
}