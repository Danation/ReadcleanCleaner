using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReadcleanCleaner
{
    public static class StringExtensions
    {
        // http://stackoverflow.com/a/21755803/1192877
        public static string ToUpperFirstChar(this string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsLower(newString[0]))
                newString = Char.ToUpper(newString[0]) + newString.Substring(1);
            return newString;
        }

        // http://stackoverflow.com/a/24580455/1192877
        public static string ReplaceCaseInsensitiveFind(this string str, string findMe, string newValue)
        {
            return Regex.Replace(str,
                Regex.Escape(findMe),
                Regex.Replace(newValue, "\\$[0-9]+", @"$$$0"),
                RegexOptions.IgnoreCase);
        }
    }
}
