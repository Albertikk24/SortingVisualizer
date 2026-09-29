using System;

namespace SortingVisualizer.Algorithms
{
    public class BubbleSort : ISortAlgorithm
    {
        public string Name { get { return "Пузырьковая"; } }

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         Action onPass,
                         int maxIterations)
        {
            int n = data.Length;
            int lastSwap = n - 1;   // последняя перестановка в проходе

            for (int i = 0; i < n - 1; i++)
            {
                if (onPass != null) onPass();   // ← новый проход

                bool swapped = false;
                int newLastSwap = 0;

                for (int j = 0; j < lastSwap; j++)
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
                        newLastSwap = j;
                    }
                    if (onStep != null) onStep(data, j, j + 1);
                }

                lastSwap = newLastSwap;
                if (!swapped) break;   // ранний выход
            }
        }
    }
}