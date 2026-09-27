using System;

namespace SortingVisualizer.Algorithms
{
    public interface ISortAlgorithm
    {
        string Name { get; }

        void Sort(double[] data, bool ascending,
                  Action<double[], int, int> onStep,
                  int maxIterations);
    }
}