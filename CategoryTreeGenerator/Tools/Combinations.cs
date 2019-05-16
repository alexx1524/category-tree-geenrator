using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CategoryTreeGenerator.Tools
{
    public static class Combinations
    {
        public static IEnumerable MakeCombinations<T>(IEnumerable<T> elements, int k)
        {
            T[] elem = elements.ToArray();
            int size = elem.Length;

            if (k > size)
            {
                yield break;
            }

            int[] numbers = new int[k];

            for (int i = 0; i < k; i++)
            {
                numbers[i] = i;
            }

            do
            {
                yield return numbers.Select(n => elem[n]);
            }
            while (NextCombination(numbers, size, k));
        }
        private static bool NextCombination(IList<int> num, int n, int k)
        {
            bool finished;

            bool changed = finished = false;

            if (k <= 0)
            {
                return false;
            }

            for (int i = k - 1; !finished && !changed; i--)
            {
                if (num[i] < n - 1 - (k - 1) + i)
                {
                    num[i]++;

                    if (i < k - 1)
                    {
                        for (int j = i + 1; j < k; j++)
                        {
                            num[j] = num[j - 1] + 1;
                        }
                    }

                    changed = true;
                }
                finished = i == 0;
            }

            return changed;
        }
    }
}