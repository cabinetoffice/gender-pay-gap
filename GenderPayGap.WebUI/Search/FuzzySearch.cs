namespace GenderPayGap.WebUI.Search
{
    public class FuzzySearch
    {

        // The Damerau–Levenshtein distance differs from the classical Levenshtein distance by including transpositions among its
        // allowable operations. The classical Levenshtein distance only allows insertion, deletion, and substitution operations.
        // Modifying this distance by including transpositions of adjacent symbols produces a different distance measure, known as the
        // Damerau–Levenshtein distance.

        public static int GetDamerauLevenshteinDistance(string s, string t)
        {
            var bounds = new {Height = s.Length + 1, Width = t.Length + 1};

            int[,] matrix = new int[bounds.Height, bounds.Width];

            for (int height = 0; height < bounds.Height; height++)
            {
                matrix[height, 0] = height;
            }

            for (int width = 0; width < bounds.Width; width++)
            {
                matrix[0, width] = width;
            }

            for (int height = 1; height < bounds.Height; height++)
            {
                for (int width = 1; width < bounds.Width; width++)
                {
                    int cost = (s[height - 1] == t[width - 1]) ? 0 : 1;
                    int insertion = matrix[height, width - 1] + 1;
                    int deletion = matrix[height - 1, width] + 1;
                    int substitution = matrix[height - 1, width - 1] + cost;

                    int distance = Math.Min(insertion, Math.Min(deletion, substitution));

                    if (height > 1 && width > 1 && s[height - 1] == t[width - 2] && s[height - 2] == t[width - 1])
                    {
                        distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                    }

                    matrix[height, width] = distance;
                }
            }

            return matrix[bounds.Height - 1, bounds.Width - 1];
        }

    }
}
