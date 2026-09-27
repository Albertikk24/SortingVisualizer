using System;

namespace SortingVisualizer.Algorithms
{
    public class BubbleSort : ISortAlgorithm
    {
        public string Name { get { return "Пузырьковая"; } }

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         int maxIterations)
        {
            int n = data.Length;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - 1 - i; j++)
                {
                    bool needSwap = ascending
                        ? data[j] > data[j + 1]
                        : data[j] < data[j + 1];

                    if (needSwap)
                    {
                        double tmp = data[j];
                        data[j] = data[j + 1];
                        data[j + 1] = tmp;
                        swapped = true;
                    }
                    if (onStep != null) onStep(data, j, j + 1);
                }
                if (!swapped) break;
            }
        }
    }
}