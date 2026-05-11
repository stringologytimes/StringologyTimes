using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
namespace DataProcessor
{
    public class ContainerTitleNormalization
    {
        private static string Normalize(string title)
        {
            string ordinalWords =
    @"(?:First|Second|Third|Fourth|Fifth|Sixth|Seventh|Eighth|Ninth|" +
    @"Tenth|Eleventh|Twelfth|Thirteenth|Fourteenth|Fifteenth|Sixteenth|Seventeenth|Eighteenth|Nineteenth|Ninteenth|" +
    @"Twentieth|Thirtieth|Fortieth|Fiftieth|Sixtieth|Seventieth|Eightieth|Ninetieth|" +
    @"(?:Twenty|Thirty|Thrity|Thiry|Forty|Fifty|Sixty|Seventy|Eighty|Ninety)-(?:First|Second|Third|Fourth|Fifth|Sixth|Seventh|Eighth|Ninth))";

            var result = title;
            result = title.TrimEnd('.');

            result = Regex.Replace(result, @"\[Conference Record\]", "");
            result = Regex.Replace(result, @"\[Proceedings \d+\]", "");
            result = Regex.Replace(result, @"\[Proceedings\]", "");
            result = Regex.Replace(result, @"Proceedings.,", "");

            // Remove "Proceedings of the <number> on"
            result = Regex.Replace(
            result,
                @"^Proceedings of the \d+ on\s*",
                "",
                RegexOptions.IgnoreCase
            );

            // Remove "Proceedings."
            result = result.Replace("Proceedings.", "");

            // Replace "Proceedings of" with "Proceedings of the"
            result = Regex.Replace(
                result,
                @"^Proceedings of(?! the\b)",
                "Proceedings of the",
                RegexOptions.IgnoreCase
            );

            // Replace "Proceedings" with "Proceedings of the"
            result = Regex.Replace(
                result,
                @"^Proceedings(?! of\b)",
                "Proceedings of the",
                RegexOptions.IgnoreCase
            );

            // Remove "Proceedings of the"
            result = Regex.Replace(
                result,
                @"^Proceedings of the",
                "",
                RegexOptions.IgnoreCase
            );



            // Remove parentheses and numbers
            result = Regex.Replace(result, @"\([^)]*\)", "");


            // Remove years
            result = Regex.Replace(result, @", \d{4}\.", "");
            result = Regex.Replace(result, @"\d{4}\.", "");
            result = Regex.Replace(result, @"\[\d{4}\]", "");

            result = Regex.Replace(result, @"\b(?:20\d{2}|19\d{2})\b", "");

            // Remove ordinal words
            result = Regex.Replace(result, @"\b\d+(?:st|nd|rd|th)\b", "");

            // Remove ordinal words
            result = Regex.Replace(result, $@"\b{ordinalWords}\b", "", RegexOptions.IgnoreCase);

            // Remove single digits
            result = Regex.Replace(result, @"['’`]\d{2}\b", "");

            // Remove " - " but not "on - "
            result = Regex.Replace(result, @"(?<!\bon)\s+-\s+.*$", "").Trim();

            // Replace multiple spaces with a single space
            result = Regex.Replace(result, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(result))
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }

            return result;
        }

        public static void NormalizeDOIElementDictionary(Dictionary<string, DOIElement> doiElementDict)
        {
            doiElementDict.Keys.ToList().ForEach((key) =>
            {
                doiElementDict[key].ContainerTitle = Normalize(doiElementDict[key].ContainerTitle);
            });
        }


    }
}
