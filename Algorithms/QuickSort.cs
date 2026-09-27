using System;

namespace SortingVisualizer.Algorithms
{
    public class QuickSort : ISortAlgorithm
    {
        public string Name { get { return "Быстрая"; } }

        private bool _asc;
        private Action<double[], int, int> _step;

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         int maxIterations)
        {
            _asc = ascending;
            _step = onStep;
            Qs(data, 0, data.Length - 1);
        }

        private void Qs(double[] d, int lo, int hi)
        {
            if (lo < hi)
            {
                int p = Partition(d, lo, hi);
                Qs(d, lo, p - 1);
                Qs(d, p + 1, hi);
            }
        }

        private int Partition(double[] d, int lo, int hi)
        {
            double pivot = d[hi];
            int i = lo - 1;
            for (int j = lo; j < hi; j++)
            {
                bool ok = _asc ? d[j] < pivot : d[j] > pivot;
                if (ok)
                {
                    i++;
                    double tmp = d[i];
                    d[i] = d[j];
                    d[j] = tmp;
                }
                if (_step != null) _step(d, j, hi);
            }
            double tmp2 = d[i + 1];
            d[i + 1] = d[hi];
            d[hi] = tmp2;
            return i + 1;
        }
    }
}