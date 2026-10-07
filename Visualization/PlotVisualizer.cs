using System;
using System.Drawing;
using System.Windows.Forms;
using SortingVisualizer.Services;

namespace SortingVisualizer.Visualization
{
    public class PlotVisualizer
    {
        private readonly Panel _panel;

        public Color AxisColor { get; set; } = Color.Black;
        public Color CurveColor { get; set; } = Color.SteelBlue;
        public Color RootColor { get; set; } = Color.Red;
        public Color GridColor { get; set; } = Color.FromArgb(230, 230, 230);

        public double ViewXmin { get; private set; }
        public double ViewXmax { get; private set; }
        public double ViewYmin { get; private set; }
        public double ViewYmax { get; private set; }

        private FunctionParser _f;
        private double _a, _b;
        private double? _root;
        private bool _hasData;

        private double? _segA, _segB;
        private double? _midC;

        public PlotVisualizer(Panel panel) { _panel = panel; }

        public void FitToData(FunctionParser f, double a, double b, double? root)
        {
            _f = f;
            _a = a;
            _b = b;
            _root = root;
            _hasData = true;

            int samples = 400;
            double ymin = double.MaxValue, ymax = double.MinValue;
            for (int i = 0; i <= samples; i++)
            {
                double x = a + (b - a) * i / samples;
                double y;
                try { y = f.Evaluate(x); }
                catch { continue; }
                if (double.IsNaN(y) || double.IsInfinity(y)) continue;
                if (y < ymin) ymin = y;
                if (y > ymax) ymax = y;
            }

            if (ymin == double.MaxValue) { ymin = -1; ymax = 1; }
            if (Math.Abs(ymax - ymin) < 1e-12) { ymin -= 1; ymax += 1; }
            double dy = (ymax - ymin) * 0.1;

            ViewXmin = a;
            ViewXmax = b;
            ViewYmin = ymin - dy;
            ViewYmax = ymax + dy;
        }

        public void SetSegment(double? a, double? b, double? c)
        {
            _segA = a;
            _segB = b;
            _midC = c;
        }

        public void ZoomAt(float px, float py, double factor)
        {
            if (!_hasData) return;
            int w = Math.Max(_panel.Width, 1);
            int h = Math.Max(_panel.Height, 1);
            int margin = 40;
            int plotW = w - 2 * margin;
            int plotH = h - 2 * margin;

            double cx = ViewXmin + (px - margin) / plotW * (ViewXmax - ViewXmin);
            double cy = ViewYmin + (h - margin - py) / plotH * (ViewYmax - ViewYmin);

            double nx1 = cx + (ViewXmin - cx) * factor;
            double nx2 = cx + (ViewXmax - cx) * factor;
            double ny1 = cy + (ViewYmin - cy) * factor;
            double ny2 = cy + (ViewYmax - cy) * factor;

            if (Math.Abs(nx2 - nx1) < 1e-9) return;
            if (Math.Abs(ny2 - ny1) < 1e-9) return;

            ViewXmin = nx1; ViewXmax = nx2;
            ViewYmin = ny1; ViewYmax = ny2;
        }

        public void Pan(float dxPx, float dyPx)
        {
            if (!_hasData) return;
            int w = Math.Max(_panel.Width, 1);
            int h = Math.Max(_panel.Height, 1);
            int margin = 40;
            int plotW = w - 2 * margin;
            int plotH = h - 2 * margin;

            double dx = dxPx / plotW * (ViewXmax - ViewXmin);
            double dy = dyPx / plotH * (ViewYmax - ViewYmin);

            ViewXmin -= dx; ViewXmax -= dx;
            ViewYmin += dy; ViewYmax += dy;
        }

        public void ResetView()
        {
            if (_hasData) FitToData(_f, _a, _b, _root);
        }

        public void Draw()
        {
            int w = Math.Max(_panel.Width, 1);
            int h = Math.Max(_panel.Height, 1);
            var bmp = new Bitmap(w, h);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int margin = 40;
                int plotW = w - 2 * margin;
                int plotH = h - 2 * margin;

                Func<double, float> mapX = x =>
                    margin + (float)((x - ViewXmin) / (ViewXmax - ViewXmin) * plotW);
                Func<double, float> mapY = y =>
                    margin + plotH - (float)((y - ViewYmin) / (ViewYmax - ViewYmin) * plotH);

                // Сетка
                using (var pen = new Pen(GridColor, 1))
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        float gx = margin + (float)i / 10 * plotW;
                        float gy = margin + (float)i / 10 * plotH;
                        g.DrawLine(pen, gx, margin, gx, h - margin);
                        g.DrawLine(pen, margin, gy, w - margin, gy);
                    }
                }

                // Оси
                using (var pen = new Pen(AxisColor, 1))
                {
                    float y0 = mapY(0);
                    if (y0 >= margin && y0 <= h - margin)
                        g.DrawLine(pen, margin, y0, w - margin, y0);

                    float x0 = mapX(0);
                    if (x0 >= margin && x0 <= w - margin)
                        g.DrawLine(pen, x0, margin, x0, h - margin);

                    g.DrawRectangle(pen, margin, margin, plotW, plotH);
                }

                if (_hasData)
                {
                    // ============ ЯРКАЯ ПОДСВЕТКА ТЕКУЩЕГО ОТРЕЗКА ============
                    if (_segA.HasValue && _segB.HasValue)
                    {
                        float ax = mapX(_segA.Value);
                        float bx = mapX(_segB.Value);
                        float left = Math.Min(ax, bx);
                        float width = Math.Abs(bx - ax);

                        // 1) Полупрозрачная оранжевая заливка во всю высоту
                        using (var brush = new SolidBrush(Color.FromArgb(90, 255, 165, 0)))
                            g.FillRectangle(brush, left, margin, width, plotH);

                        // 2) Жирная оранжевая линия на оси X
                        float yAxis = mapY(0);
                        if (yAxis >= margin && yAxis <= h - margin)
                        {
                            using (var pen = new Pen(Color.Orange, 6))
                                g.DrawLine(pen, left, yAxis, left + width, yAxis);
                        }

                        // 3) Тонкие вертикальные границы отрезка
                        using (var pen = new Pen(Color.Orange, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                        {
                            g.DrawLine(pen, left, margin, left, h - margin);
                            g.DrawLine(pen, left + width, margin, left + width, h - margin);
                        }

                        // 4) Маркеры-стрелки сверху
                        using (var pen = new Pen(Color.DarkOrange, 2))
                        {
                            float my = margin + 6;
                            g.DrawLine(pen, left, my, left, my + 8);
                            g.DrawLine(pen, left + width, my, left + width, my + 8);
                        }
                    }

                    // ============ КРИВАЯ ============
                    int samples = Math.Max(plotW * 2, 400);
                    using (var pen = new Pen(CurveColor, 2))
                    {
                        PointF? prev = null;
                        for (int i = 0; i <= samples; i++)
                        {
                            double x = ViewXmin + (ViewXmax - ViewXmin) * i / samples;
                            double y;
                            try { y = _f.Evaluate(x); }
                            catch { prev = null; continue; }
                            if (double.IsNaN(y) || double.IsInfinity(y))
                            {
                                prev = null;
                                continue;
                            }
                            float px = mapX(x);
                            float py = mapY(y);
                            var cur = new PointF(px, py);
                            if (prev.HasValue) g.DrawLine(pen, prev.Value, cur);
                            prev = cur;
                        }
                    }

                    // ============ СЕРЕДИНА c ============
                    if (_midC.HasValue)
                    {
                        float px = mapX(_midC.Value);
                        float py = mapY(0);

                        // вертикальная линия от точки на графике до оси
                        double cy;
                        try { cy = _f.Evaluate(_midC.Value); }
                        catch { cy = 0; }
                        float pyGraph = mapY(cy);

                        using (var pen = new Pen(Color.DarkGoldenrod, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                            g.DrawLine(pen, px, pyGraph, px, py);

                        // круглая жёлтая точка на оси X
                        using (var brush = new SolidBrush(Color.Gold))
                        using (var pen = new Pen(Color.DarkGoldenrod, 2))
                        {
                            g.FillEllipse(brush, px - 7, py - 7, 14, 14);
                            g.DrawEllipse(pen, px - 7, py - 7, 14, 14);
                        }

                        // точка на самой кривой
                        using (var brush = new SolidBrush(Color.DarkGoldenrod))
                            g.FillEllipse(brush, px - 4, pyGraph - 4, 8, 8);
                    }

                    // ============ КОРЕНЬ ============
                    if (_root.HasValue)
                    {
                        double rx = _root.Value;
                        float px = mapX(rx);
                        float py = mapY(0);

                        using (var brush = new SolidBrush(RootColor))
                            g.FillEllipse(brush, px - 6, py - 6, 12, 12);

                        using (var pen = new Pen(RootColor, 1)
                        { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                            g.DrawLine(pen, px, margin, px, h - margin);

                        using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                            g.DrawString("x = " + rx.ToString("F5"), font,
                                Brushes.DarkRed, px + 8, py - 20);
                    }
                }

                // Подписи краёв
                using (var font = new Font("Segoe UI", 8))
                {
                    g.DrawString(ViewXmin.ToString("F3"), font, Brushes.Black,
                        margin, h - margin + 5);
                    g.DrawString(ViewXmax.ToString("F3"), font, Brushes.Black,
                        w - margin - 50, h - margin + 5);
                    g.DrawString(ViewYmax.ToString("F2"), font, Brushes.Black,
                        margin - 35, margin - 5);
                    g.DrawString(ViewYmin.ToString("F2"), font, Brushes.Black,
                        margin - 35, h - margin - 15);
                }
            }

            var old = _panel.BackgroundImage;
            _panel.BackgroundImage = bmp;
            if (old != null) old.Dispose();
        }
    }
}