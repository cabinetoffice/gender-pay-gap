using System;
using System.Collections.Generic;
using System.Linq;

namespace GenderPayGap.Extensions
{
    public static class Numeric
    {

        public static Random Random = new Random(VirtualDateTime.Now.Millisecond);

        /// <summary>
        ///     Returns a randam value between two ranges (inclusive) but excludes certain values
        /// </summary>
        /// <param name="Min">The minimum value of the returned result</param>
        /// <param name="Max">The maximum value of the returned result</param>
        /// <param name="excludes">Values to exclude from results</param>
        /// <returns>The random value between the min and max values (excluding specified results)</returns>
        public static int Rand(int Min, int Max, params int[] excludes)
        {
            int result;
            if (excludes == null || excludes.Length == 0)
            {
                result = Random.Next(Min, Max + 1);
                return result;
            }

            //Get all values within the range
            int[] range = Enumerable.Range(Min, (Max - Min) + 1).ToArray();

            //Normalise all excludes and sort
            excludes = new SortedSet<int>(excludes).ToArray();

            //Throw wrror if all values are excluded
            if (range.SequenceEqual(excludes))
            {
                throw new ArgumentOutOfRangeException(nameof(excludes), "All values cannot be excluded");
            }

            //Get a random value unit it is not in the cluded range
            do
            {
                result = Random.Next(Min, Max + 1);
            } while (excludes.Contains(result));

            return result;
        }

        public static bool Between(this int num, int lower, int upper, bool inclusive = true)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }

        public static bool Contains(this int[] numbers, int value)
        {
            if (numbers == null || numbers.Length < 1)
            {
                return false;
            }

            foreach (int i in numbers)
            {
                if (i == value)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
