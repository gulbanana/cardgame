using System.Text.RegularExpressions;

namespace Cardgame.Views
{
    internal static class Strings
    {
        public static string TitleCase(string words)
        {
            return Regex.Replace(words, @"((?<=[a-z])[A-Z]|(?<!^)[A-Z](?=[a-z]))", " $1");
        }
    }
}