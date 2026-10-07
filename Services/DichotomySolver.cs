using System;
using System.Collections.Generic;

namespace SortingVisualizer.Services
{
    public class IterationStep
    {
        public int Number;
        public double A;
        public double B;
        public double C;
        public double Fa;
        public double Fc;
        public double Fb;
        public string Keep;   // "[a, c]" или "[c, b]"
    }

    public class DichotomyResult
    {
        public bool Success;
        public string Message;
        public double Root;
        public int Iterations;
        public List<IterationStep> Steps = new List<IterationStep>();
        public List<double> MultipleRoots = new List<double>();
    }

    public static class DichotomySolver
    {
        public static DichotomyResult Solve(
            FunctionParser f, double a, double b, double eps)
        {
            var result = new DichotomyResult();

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

            // ============ 1. СКАНИРОВАНИЕ — только для проверки количества корней ============
            const int N = 200;
            double h = (b - a) / N;
            int signChanges = 0;

            double xPrev = a;
            double fPrev;
            try { fPrev = f.Evaluate(xPrev); }
            catch (Exception ex)
            {
                result.Message = "Ошибка вычисления f(a): " + ex.Message;
                return result;
            }

            for (int i = 1; i <= N; i++)
            {
                double xCur = a + i * h;
                double fCur;
                try { fCur = f.Evaluate(xCur); }
                catch (Exception ex)
                {
                    result.Message = "Ошибка вычисления f(" + xCur + "): " + ex.Message;
                    return result;
                }

                if (fPrev == 0) signChanges++;
                else if (fPrev * fCur < 0) signChanges++;

                xPrev = xCur;
                fPrev = fCur;
            }

            if (signChanges == 0)
            {
                result.Message = "На интервале [" + a + ", " + b +
                                 "] корней не найдено (функция не меняет знак).";
                return result;
            }

            if (signChanges > 1)
            {
                result.Message = "У вас несколько корней на выбранном интервале (" +
                                 signChanges + " шт.). Уточните интервал.";
                return result;
            }

            // ============ 2. ДИХОТОМИЯ — от полного [a, b] ============
            double fa = f.Evaluate(a);
            double fb = f.Evaluate(b);

            if (fa * fb > 0)
            {
                // Знак не меняется на концах, хотя внутри корень есть —
                // редкий случай. Обрабатываем через сканирование.
                result.Message = "На концах интервала f(a) и f(b) одного знака — " +
                                 "дихотомия неприменима. Уточните интервал.";
                return result;
            }

            double left = a, right = b;
            double fLeft = fa, fRight = fb;
            double c = a;
            int num = 0;

            const int MAX_ITER = 1000;
            while ((right - left) / 2 > eps && num < MAX_ITER)
            {
                c = (left + right) / 2;
                double fc = f.Evaluate(c);

                var step = new IterationStep
                {
                    Number = num,
                    A = left,
                    B = right,
                    C = c,
                    Fa = fLeft,
                    Fc = fc,
                    Fb = fRight,
                    Keep = (fLeft * fc < 0) ? "[a, c]" : "[c, b]"
                };
                result.Steps.Add(step);

                if (Math.Abs(fc) < 1e-15) break;

                if (fLeft * fc < 0) { right = c; fRight = fc; }
                else { left = c; fLeft = fc; }

                num++;
            }

            result.Success = true;
            result.Root = (left + right) / 2;
            result.Iterations = num;
            result.Message = "Корень найден.";
            return result;
        }
    }
}