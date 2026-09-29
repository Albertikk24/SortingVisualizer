using System;

namespace SortingVisualizer.Algorithms
{
    public interface ISortAlgorithm
    {
        string Name { get; }

        /// <summary>
        /// Сортировка массива.
        /// </summary>
        /// <param name="data">Рабочая копия массива</param>
        /// <param name="ascending">true — по возрастанию</param>
        /// <param name="onStep">Callback на каждое сравнение/перестановку</param>
        /// <param name="onPass">Callback на каждый новый проход (полный прогон массива)</param>
        /// <param name="maxIterations">Лимит итераций для BOGO</param>
        void Sort(double[] data, bool ascending,
                  Action<double[], int, int> onStep,
                  Action onPass,
                  int maxIterations);
    }
}