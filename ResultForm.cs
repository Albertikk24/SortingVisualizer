using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SortingVisualizer
{
    public class ResultForm : Form
    {
        public ResultForm(string algoName, double[] data, bool ascending)
        {
            Text = algoName + " — итоговый массив (" +
                   (ascending ? "по возрастанию" : "по убыванию") + ")";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;

            var txt = new TextBox();
            txt.Multiline = true;
            txt.ScrollBars = ScrollBars.Both;
            txt.ReadOnly = true;
            txt.Dock = DockStyle.Fill;
            txt.Font = new Font("Consolas", 10);
            txt.WordWrap = false;

            var sb = new StringBuilder();
            sb.AppendLine("Элементов: " + data.Length);
            sb.AppendLine("Проверка отсортированности: " +
                          (IsSorted(data, ascending) ? "OK ✔" : "ОШИБКА ✘"));
            sb.AppendLine();
            sb.AppendLine("Итоговый массив:");
            sb.AppendLine();

            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("0.#####"));
                if ((i + 1) % 10 == 0)
                    sb.AppendLine();
                else
                    sb.Append("\t");
            }
            if (data.Length % 10 != 0) sb.AppendLine();

            txt.Text = sb.ToString();
            Controls.Add(txt);
        }

        private static bool IsSorted(double[] arr, bool ascending)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                if (ascending)
                {
                    if (arr[i] > arr[i + 1]) return false;
                }
                else
                {
                    if (arr[i] < arr[i + 1]) return false;
                }
            }
            return true;
        }
    }
}