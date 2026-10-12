using System;
using System.Collections.Generic;

namespace SortingVisualizer.Services
{
    public enum OptimizeMode
    {
        Minimum,    // поиск минимума
        Maximum,    // поиск максимума
        Root        // поиск корня (f(x) = 0)
    }

    public class GS_IterationStep
    {
        public int Number;
        public double A;
        public double B;
        public double X1;
        public double X2;
        public double Fx1;
        public double Fx2;
        public string Keep;
        public string Note;
    }

    public class GoldenSectionResult
    {
        public bool Success;
        public string Message;
        public double Xmin;          // точка экстремума / корня
        public double Fmin;          // f в этой точке
        public int Iterations;
        public List<GS_IterationStep> Steps = new List<GS_IterationStep>();
    }

    public static class GoldenSectionSolver
    {
        private const double INV_PHI = 0.6180339887498949;
        private const double BLOWUP_THRESHOLD = 1e10;

        public static GoldenSectionResult Solve(
            FunctionParser f, double a, double b, double eps, OptimizeMode mode)
        {
            var result = new GoldenSectionResult();

            if (a >= b)
            {
                result.Message = "a должно быть меньше b.";
                return result;
            }
            if (eps <= 0)
            {
                result.Message = "Точность ε должна быть положительной.";
                return result;
            }

            // ============ Режим «корень» — используем дихотомию ============
            if (mode == OptimizeMode.Root)
            {
                var dr = DichotomySolver.Solve(f, a, b, eps);
                result.Success = dr.Success;
                result.Message = dr.Message;
                result.Xmin = dr.Root;
                try { result.Fmin = f.Evaluate(dr.Root); }
                catch { result.Fmin = double.NaN; }
                result.Iterations = dr.Iterations;

                // переносим шаги дихотомии в формат золотого сечения
                foreach (var st in dr.Steps)
                {
                    result.Steps.Add(new GS_IterationStep
                    {
                        Number = st.Number,
                        A = st.A,
                        B = st.B,
                        X1 = st.C,
                        X2 = double.NaN,
                        Fx1 = st.Fc,
                        Fx2 = double.NaN,
                        Keep = st.Keep,
                        Note = "дихотомия"
                    });
                }
                return result;
            }

            // ============ Для Min/Max — золотое сечение ============
            bool isMax = (mode == OptimizeMode.Maximum);

            // ============ Сканирование ============
            const int N = 200;
            double h = (b - a) / N;
            int localExtremums = 0;
            double lastSlopeSign = 0;
            double fPrev = double.NaN;

            for (int i = 0; i <= N; i++)
            {
                double x = a + i * h;
                double fx;

                try { fx = f.Evaluate(x); }
                catch (Exception ex)
                {
                    result.Message = "Ошибка вычисления f(" + x.ToString("F6") + "): " +
                                     ex.Message;
                    return result;
                }

                if (double.IsNaN(fx) || double.IsInfinity(fx))
                {
                    result.Message = "Функция имеет разрыв в точке x = " +
                                     x.ToString("F6") + ". Метод неприменим.";
                    return result;
                }

                if (Math.Abs(fx) > BLOWUP_THRESHOLD)
                {
                    result.Message = "Функция резко возрастает около x = " +
                                     x.ToString("F6") + ". Вероятен разрыв.";
                    return result;
                }

                if (i >= 1)
                {
                    double slope = fx - fPrev;
                    double sign = Math.Sign(slope);

                    // для максимума знак производной инвертируем
                    double signForSearch = isMax ? -sign : sign;

                    if (lastSlopeSign < 0 && signForSearch > 0)
                        localExtremums++;

                    if (signForSearch != 0)
                        lastSlopeSign = signForSearch;
                }

                fPrev = fx;
            }

            if (localExtremums == 0)
            {
                double fa = f.Evaluate(a);
                double fb = f.Evaluate(b);

                result.Success = true;
                if ((isMax && fa > fb) || (!isMax && fa < fb))
                {
                    result.Xmin = a;
                    result.Fmin = fa;
                    result.Message = (isMax ? "Максимум" : "Минимум") +
                                     " на границе: x = a = " + a.ToString("F6");
                }
                else
                {
                    result.Xmin = b;
                    result.Fmin = fb;
                    result.Message = (isMax ? "Максимум" : "Минимум") +
                                     " на границе: x = b = " + b.ToString("F6");
                }
                return result;
            }

            if (localExtremums > 1)
            {
                result.Message = "На интервале найдено " + localExtremums +
                                 " локальных " + (isMax ? "максимума" : "минимума") +
                                 ". Функция не унимодальна. Уточните интервал.";
                return result;
            }

            // ============ Основной цикл ============
            double left = a, right = b;
            int num = 0;
            const int MAX_ITER = 1000;

            double x1 = right - (right - left) * INV_PHI;
            double x2 = left + (right - left) * INV_PHI;
            double fx1 = f.Evaluate(x1);
            double fx2 = f.Evaluate(x2);

            while ((right - left) > eps && num < MAX_ITER)
            {
                if (double.IsNaN(fx1) || double.IsInfinity(fx1) ||
                    double.IsNaN(fx2) || double.IsInfinity(fx2))
                {
                    result.Message = "Функция не определена во внутренней точке.";
                    return result;
                }

                if (Math.Abs(fx1) > BLOWUP_THRESHOLD ||
                    Math.Abs(fx2) > BLOWUP_THRESHOLD)
                {
                    result.Message = "Функция резко возрастает между x = " +
                                     left.ToString("F6") + " и x = " +
                                     right.ToString("F6") + ". Вероятен разрыв.";
                    return result;
                }

                var step = new GS_IterationStep
                {
                    Number = num,
                    A = left,
                    B = right,
                    X1 = x1,
                    X2 = x2,
                    Fx1 = fx1,
                    Fx2 = fx2
                };

                // Сравнение: для максимума — наоборот
                bool takeLeft;
                if (isMax)
                    takeLeft = fx1 > fx2;    // для максимума больший «лучше»
                else
                    takeLeft = fx1 < fx2;    // для минимума меньший «лучше»

                if (takeLeft)
                {
                    step.Keep = "[a, x2]";
                    step.Note = "x2 → b, пересчёт x1";
                    right = x2;
                    x2 = x1;
                    fx2 = fx1;
                    x1 = right - (right - left) * INV_PHI;
                    fx1 = f.Evaluate(x1);
                }
                else
                {
                    step.Keep = "[x1, b]";
                    step.Note = "x1 → a, пересчёт x2";
                    left = x1;
                    x1 = x2;
                    fx1 = fx2;
                    x2 = left + (right - left) * INV_PHI;
                    fx2 = f.Evaluate(x2);
                }

                result.Steps.Add(step);
                num++;
            }

            if (num >= MAX_ITER)
            {
                result.Message = "Достигнут лимит итераций (" + MAX_ITER + ").";
                return result;
            }

            double xExt = (left + right) / 2;
            double fExt = f.Evaluate(xExt);

            result.Success = true;
            result.Xmin = xExt;
            result.Fmin = fExt;
            result.Iterations = num;
            result.Message = (isMax ? "Максимум" : "Минимум") +
                             " найден: x = " + xExt.ToString("F6") +
                             ", f(x) = " + fExt.ToString("F6");
            return result;
        }
    }
}