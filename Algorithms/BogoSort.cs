using System;

namespace SortingVisualizer.Algorithms
{
    public class BogoSort : ISortAlgorithm
    {
        public string Name { get { return "BOGO"; } }
        private static readonly Random _rnd = new Random();

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         int maxIterations)
        {
            int iter = 0;
            while (!IsSorted(data, ascending) && iter < maxIterations)
            {
                Shuffle(data);
                iter++;
                if (onStep != null) onStep(data, -1, -1);
            }

            if (!IsSorted(data, ascending))
                throw new InvalidOperationException(
                    "лимит итераций (" + maxIterations + ") превышен");
        }

        private static bool IsSorted(double[] d, bool asc)
        {
            for (int i = 0; i < d.Length - 1; i++)
                if (asc ? d[i] > d[i + 1] : d[i] < d[i + 1]) return false;
            return true;
        }

        private static void Shuffle(double[] d)
        {
            for (int i = d.Length - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                double tmp = d[i];
                d[i] = d[j];
                d[j] = tmp;
            }
        }
    }
}