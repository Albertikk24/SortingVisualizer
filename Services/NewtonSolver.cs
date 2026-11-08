using System;
using System.Collections.Generic;

namespace SortingVisualizer.Services
{
    public class NewtonStep
    {
        public int Number;
        public double X;
        public double Fx;
        public double Derivative;       // f'(x)
        public double SecondDerivative; // f''(x)
        public double NextX;
    }

    public class NewtonResult
    {
        public bool Success;
        public string Message;
        public double Xmin;
        public double Fmin;
        public int Iterations;
        public List<NewtonStep> Steps = new List<NewtonStep>();
    }

    public static class NewtonSolver
    {
        private const double H = 1e-5; // Шаг для производных
        private const double MAX_VAL = 1e10; // Порог разрыва

        private static double F1(FunctionParser f, double x)
        {
            return (f.Evaluate(x + H) - f.Evaluate(x - H)) / (2 * H);
        }

        private static double F2(FunctionParser f, double x)
        {
            return (f.Evaluate(x + H) - 2 * f.Evaluate(x) + f.Evaluate(x - H)) / (H * H);
        }

        public static NewtonResult Solve(
            FunctionParser f, double a, double b, double eps,
            OptimizeMode mode, double startX)
        {
            var result = new NewtonResult();

            // ============ 1. БАЗОВЫЕ ПРОВЕРКИ ============
            if (a >= b)
            {
                result.Message = "Ошибка: a должно быть меньше b.";
                return result;
            }
            if (eps <= 0)
            {
                result.Message = "Ошибка: точность ε должна быть > 0.";
                return result;
            }
            if (startX <= a || startX >= b)
            {
                result.Message = $"Ошибка: начальная точка x0 ({startX}) должна быть строго внутри интервала ({a}, {b}).";
                return result;
            }

            // ============ 2. ПРЕДВАРИТЕЛЬНЫЙ АНАЛИЗ ФУНКЦИИ ============
            // Проходим по сетке, ищем разрывы и скачки
            const int STEPS = 100;
            double dx = (b - a) / STEPS;
            for (int i = 0; i <= STEPS; i++)
            {
                double x = a + i * dx;
                double y;
                try { y = f.Evaluate(x); }
                catch (Exception ex)
                {
                    result.Message = $"Ошибка вычисления f({x:F4}): {ex.Message}";
                    return result;
                }

                if (double.IsNaN(y) || double.IsInfinity(y) || Math.Abs(y) > MAX_VAL)
                {
                    result.Message = $"Функция имеет разрыв или уходит в бесконечность около x = {x:F4}. Метод Ньютона неприменим.";
                    return result;
                }
            }

            // ============ 3. ОСНОВНОЙ ЦИКЛ НЬЮТОНА ============
            double xCur = startX;
            const int MAX_ITER = 50;
            int iter = 0;

            while (iter < MAX_ITER)
            {
                double fx, d1, d2;
                try
                {
                    fx = f.Evaluate(xCur);
                    d1 = F1(f, xCur);
                    d2 = F2(f, xCur);
                }
                catch (Exception ex)
                {
                    result.Message = "Ошибка производных: " + ex.Message;
                    return result;
                }

                // Проверка на разрыв в текущей точке
                if (double.IsNaN(fx) || double.IsInfinity(fx) || Math.Abs(fx) > MAX_VAL)
                {
                    result.Message = $"Функция разрывна в точке x = {xCur:F4}. Метод остановлен.";
                    return result;
                }

                double nextX;

                // ---- РЕЖИМ КОРНЯ (f(x) = 0) ----
                if (mode == OptimizeMode.Root)
                {
                    if (Math.Abs(d1) < 1e-6)
                    {
                        result.Message = $"Ошибка: f'({xCur:F4}) ≈ 0. Касательная горизонтальна, корень не найти. Смените x0.";
                        return result;
                    }
                    nextX = xCur - fx / d1;
                }
                // ---- РЕЖИМ МИНИМУМА / МАКСИМУМА ----
                else
                {
                    if (Math.Abs(d2) < 1e-6)
                    {
                        result.Message = $"Ошибка: f''({xCur:F4}) ≈ 0. Вторая производная равна нулю, метод Ньютона расходится. Смените x0.";
                        return result;
                    }

                    nextX = xCur - d1 / d2;

                    // Проверка типа экстремума
                    if (mode == OptimizeMode.Minimum && d2 < 0)
                    {
                        result.Message = $"Ошибка: в точке x = {xCur:F4} найден МАКСИМУМ (f'' < 0), а искали минимум. Смените x0.";
                        return result;
                    }
                    if (mode == OptimizeMode.Maximum && d2 > 0)
                    {
                        result.Message = $"Ошибка: в точке x = {xCur:F4} найден МИНИМУМ (f'' > 0), а искали максимум. Смените x0.";
                        return result;
                    }
                }

                // Записываем шаг
                result.Steps.Add(new NewtonStep
                {
                    Number = iter,
                    X = xCur,
                    Fx = fx,
                    Derivative = d1,
                    SecondDerivative = d2,
                    NextX = nextX
                });

                // Условие остановки
                if (Math.Abs(nextX - xCur) < eps)
                {
                    xCur = nextX;
                    break;
                }

                // Проверка вылета за границы
                if (nextX < a || nextX > b)
                {
                    result.Message = $"Ошибка: метод Ньютона вылетел за пределы [{a}; {b}] (ушел в x = {nextX:F4}). Смените начальную точку x0.";
                    return result;
                }

                xCur = nextX;
                iter++;
            }

            // ============ 4. УСПЕШНОЕ ЗАВЕРШЕНИЕ ============
            result.Success = true;
            result.Xmin = xCur;

            try
            {
                result.Fmin = f.Evaluate(xCur);
            }
            catch
            {
                result.Fmin = double.NaN;
            }

            result.Iterations = iter;

            string what = mode == OptimizeMode.Root ? "Корень" : (mode == OptimizeMode.Maximum ? "Максимум" : "Минимум");
            result.Message = $"{what} найден: x = {xCur:F6}, f(x) = {result.Fmin:F6} (итераций: {iter})";

            return result;
        }
    }
}