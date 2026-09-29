using System;

namespace SortingVisualizer.Algorithms
{
    public class ShakerSort : ISortAlgorithm
    {
        public string Name { get { return "Шейкерная"; } }

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         Action onPass,
                         int maxIterations)
        {
            int n = data.Length;
            int left = 0, right = n - 1;
            int maxPasses = (n - 1) / 2;
            bool swapped = true;

            for (int pass = 0; pass < maxPasses && swapped && left < right; pass++)
            {
                if (onPass != null) onPass();   // ← один двойной проход

                swapped = false;

                // Прямой проход
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

                // Обратный проход
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