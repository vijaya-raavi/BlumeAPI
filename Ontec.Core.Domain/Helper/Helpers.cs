using System.Text.RegularExpressions;

namespace Ontec.Core.Domain.Helper
{
    public static class Helpers
    {
        public static string SplitPascalCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            string[] words = Regex.Split(text, @"(?<!^)(?=[A-Z])");

            // Check if there are words to process
            if (words.Length == 0) return text;

            // Convert all words to lowercase except for the first one
            for (int i = 1; i < words.Length; i++)
            {
                words[i] = words[i].ToLower();
            }

            // Join the words back into a single string
            return string.Join(" ", words);
            //return Regex.Replace(text, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}
