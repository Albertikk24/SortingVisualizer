using System;

namespace SortingVisualizer.Services
{
    public static class DataGenerator
    {
        private static readonly Random _rnd = new Random();

        public static double[] Generate(int count, double min, double max, int decimals)
        {
            if (count <= 0)
                throw new ArgumentException("Количество должно быть > 0.");
            if (min >= max)
                throw new ArgumentException("Min должно быть меньше Max.");

            var arr = new double[count];
            for (int i = 0; i < count; i++)
            {
                double v = min + _rnd.NextDouble() * (max - min);
                arr[i] = Math.Round(v, decimals);
                if (arr[i] < min) arr[i] = min;
                if (arr[i] > max) arr[i] = max;
            }
            return arr;
        }
    }
}