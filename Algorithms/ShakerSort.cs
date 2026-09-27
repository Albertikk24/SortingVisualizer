using System;

namespace SortingVisualizer.Algorithms
{
    public class ShakerSort : ISortAlgorithm
    {
        public string Name { get { return "Шейкерная"; } }

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         int maxIterations)
        {
            int n = data.Length;
            int left = 0, right = n - 1;
            int maxPasses = (n - 1) / 2;
            bool swapped = true;

            for (int pass = 0; pass < maxPasses && swapped && left < right; pass++)
            {
                swapped = false;

                for (int i = left; i < right; i++)
                {
                    bool needSwap = ascending
                        ? data[i] > data[i + 1]
                        : data[i] < data[i + 1];
                    if (needSwap)
                    {
                        double tmp = data[i];
                        data[i] = data[i + 1];
                        data[i + 1] = tmp;
                        swapped = true;
                    }
                    if (onStep != null) onStep(data, i, i + 1);
                }
                right--;

                for (int i = right; i > left; i--)
                {
                    bool needSwap = ascending
                        ? data[i - 1] > data[i]
                        : data[i - 1] < data[i];
                    if (needSwap)
                    {
                        double tmp = data[i - 1];
                        data[i - 1] = data[i];
                        data[i] = tmp;
                        swapped = true;
                    }
                    if (onStep != null) onStep(data, i - 1, i);
                }
                left++;
            }
        }
    }
}