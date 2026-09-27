using System;

namespace SortingVisualizer.Algorithms
{
    public class InsertionSort : ISortAlgorithm
    {
        public string Name { get { return "Вставками"; } }

        public void Sort(double[] data, bool ascending,
                         Action<double[], int, int> onStep,
                         int maxIterations)
        {
            for (int i = 1; i < data.Length; i++)
            {
                double key = data[i];
                int j = i - 1;

                while (j >= 0 && (ascending ? data[j] > key : data[j] < key))
                {
                    data[j + 1] = data[j];
                    if (onStep != null) onStep(data, j, j + 1);
                    j--;
                }
                data[j + 1] = key;
                if (onStep != null) onStep(data, j + 1, i);
            }
        }
    }
}