using System;
using System.Collections.Generic;
using System.Globalization;

namespace SortingVisualizer.Services
{
    public class FunctionParser
    {
        private readonly string _expr;
        private int _pos;

        public FunctionParser(string expr)
        {
            _expr = (expr ?? "").Replace(" ", "").ToLowerInvariant();
        }

        public double Evaluate(double x)
        {
            _pos = 0;
            double v = ParseExpr(x);
            if (_pos < _expr.Length)
                throw new ArgumentException("Не удалось разобрать формулу: лишние символы в позиции " + _pos);
            return v;
        }

        private double ParseExpr(double x)
        {
            double v = ParseTerm(x);
            while (_pos < _expr.Length && (_expr[_pos] == '+' || _expr[_pos] == '-'))
            {
                char op = _expr[_pos++];
                double r = ParseTerm(x);
                v = (op == '+') ? v + r : v - r;
            }
            return v;
        }

        private double ParseTerm(double x)
        {
            double v = ParseFactor(x);
            while (_pos < _expr.Length &&
                   (_expr[_pos] == '*' || _expr[_pos] == '/' || _expr[_pos] == '^'))
            {
                char op = _expr[_pos++];
                double r = ParseFactor(x);
                if (op == '*') v *= r;
                else if (op == '/')
                {
                    if (Math.Abs(r) < 1e-15)
                        throw new DivideByZeroException("Деление на ноль в формуле.");
                    v /= r;
                }
                else v = Math.Pow(v, r);
            }
            return v;
        }

        private double ParseFactor(double x)
        {
            // Унарный минус
            if (_pos < _expr.Length && _expr[_pos] == '-')
            {
                _pos++;
                return -ParseFactor(x);
            }
            if (_pos < _expr.Length && _expr[_pos] == '+')
            {
                _pos++;
                return ParseFactor(x);
            }

            // Число
            if (_pos < _expr.Length && (char.IsDigit(_expr[_pos]) || _expr[_pos] == '.'))
            {
                int start = _pos;
                while (_pos < _expr.Length && (char.IsDigit(_expr[_pos]) || _expr[_pos] == '.'))
                    _pos++;
                string num = _expr.Substring(start, _pos - start).Replace('.', ',');
                return double.Parse(num, CultureInfo.CurrentCulture);
            }

            // Идентификатор: x, sin, cos, tg, ln, exp, sqrt, pi, e
            if (_pos < _expr.Length && char.IsLetter(_expr[_pos]))
            {
                int start = _pos;
                while (_pos < _expr.Length && char.IsLetter(_expr[_pos]))
                    _pos++;
                string name = _expr.Substring(start, _pos - start);

                // Отдельно: x
                if (name == "x") return x;
                if (name == "pi") return Math.PI;
                if (name == "e") return Math.E;

                // Функции: sin(...), cos(...), ...
                if (_pos < _expr.Length && _expr[_pos] == '(')
                {
                    _pos++;    // пропускаем '('
                    double arg = ParseExpr(x);
                    if (_pos >= _expr.Length || _expr[_pos] != ')')
                        throw new ArgumentException("Не закрыта скобка для " + name);
                    _pos++;    // пропускаем ')'

                    switch (name)
                    {
                        case "sin": return Math.Sin(arg);
                        case "cos": return Math.Cos(arg);
                        case "tg":
                        case "tan": return Math.Tan(arg);
                        case "ctg": return 1.0 / Math.Tan(arg);
                        case "ln": return Math.Log(arg);
                        case "lg": return Math.Log10(arg);
                        case "exp": return Math.Exp(arg);
                        case "sqrt": return Math.Sqrt(arg);
                        case "abs": return Math.Abs(arg);
                    }
                    throw new ArgumentException("Неизвестная функция: " + name);
                }

                throw new ArgumentException("Неизвестный идентификатор: " + name);
            }

            // Скобка
            if (_pos < _expr.Length && _expr[_pos] == '(')
            {
                _pos++;
                double v = ParseExpr(x);
                if (_pos >= _expr.Length || _expr[_pos] != ')')
                    throw new ArgumentException("Не закрыта скобка");
                _pos++;
                return v;
            }

            throw new ArgumentException("Неожиданный символ в позиции " + _pos);
        }
    }
}