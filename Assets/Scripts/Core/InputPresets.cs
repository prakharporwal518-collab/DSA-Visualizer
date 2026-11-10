using System;

namespace DSAVisualizer.Core
{
    /// <summary>The input shapes users can poke in the Sorting Lab.</summary>
    public enum InputPreset
    {
        Random,
        Sorted,
        Reversed,
        NearlySorted,
        FewUnique
    }

    /// <summary>
    /// Generates the preset inputs. Values stay in 1..9 so bars are easy to compare by eye
    /// and labels stay a single readable digit.
    /// </summary>
    public static class InputPresets
    {
        public const int MinValue = 1;
        public const int MaxValue = 9;

        public static int[] Generate(InputPreset preset, int count, int seed)
        {
            var rng = new Random(seed);
            var values = new int[count];
            switch (preset)
            {
                case InputPreset.Sorted:
                    FillAscending(values);
                    break;
                case InputPreset.Reversed:
                    FillAscending(values);
                    Array.Reverse(values);
                    break;
                case InputPreset.NearlySorted:
                    FillAscending(values);
                    if (count >= 2)
                    {
                        // One adjacent pair out of place: best case for insertion/bubble sort.
                        int i = rng.Next(0, count - 1);
                        int tmp = values[i];
                        values[i] = values[i + 1];
                        values[i + 1] = tmp;
                    }
                    break;
                case InputPreset.FewUnique:
                {
                    int[] pool = { 2, 5, 8 };
                    for (int i = 0; i < count; i++) values[i] = pool[rng.Next(pool.Length)];
                    break;
                }
                default:
                    FillAscending(values);
                    for (int i = count - 1; i > 0; i--) // Fisher-Yates shuffle
                    {
                        int j = rng.Next(i + 1);
                        int tmp = values[i];
                        values[i] = values[j];
                        values[j] = tmp;
                    }
                    break;
            }
            return values;
        }

        /// <summary>Evenly spread ascending values across MinValue..MaxValue (distinct while count allows it).</summary>
        private static void FillAscending(int[] values)
        {
            int n = values.Length;
            for (int i = 0; i < n; i++)
                values[i] = n <= 1 ? MinValue : MinValue + (int)Math.Round(i * (MaxValue - MinValue) / (double)(n - 1));
        }
    }
}
