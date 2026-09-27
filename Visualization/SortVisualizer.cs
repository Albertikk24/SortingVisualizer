using System;
using System.Drawing;
using System.Windows.Forms;

namespace SortingVisualizer.Visualization
{
    public class SortVisualizer
    {
        private readonly Panel _panel;
        public Color BarColor { get; set; }
        public Color HighlightColor { get; set; }

        public SortVisualizer(Panel panel)
        {
            _panel = panel;
            BarColor = Color.SteelBlue;
            HighlightColor = Color.OrangeRed;
        }

        public void Draw(double[] data, int hiA, int hiB)
        {
            if (data == null || data.Length == 0)
            {
                _panel.BackgroundImage = null;
                return;
            }

            int w = Math.Max(_panel.Width, 1);
            int h = Math.Max(_panel.Height, 1);
            var bmp = new Bitmap(w, h);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                double min = data[0], max = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    if (data[i] < min) min = data[i];
                    if (data[i] > max) max = data[i];
                }
                double range = max - min;
                if (range <= 0) range = 1;

                float barW = Math.Max(1f, (float)w / data.Length);
                using (var normal = new SolidBrush(BarColor))
                using (var hi = new SolidBrush(HighlightColor))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        float bh = (float)((data[i] - min) / range) * (h - 4);
                        if (bh < 1) bh = 1;

                        var brush = (i == hiA || i == hiB) ? hi : normal;
                        var rect = new RectangleF(
                            i * barW,
                            h - bh,
                            Math.Max(1f, barW - 0.3f),
                            bh);
                        g.FillRectangle(brush, rect);
                    }
                }
            }

            var old = _panel.BackgroundImage;
            _panel.BackgroundImage = bmp;
            if (old != null) old.Dispose();
        }

        public void Draw(double[] data)
        {
            Draw(data, -1, -1);
        }
    }
}