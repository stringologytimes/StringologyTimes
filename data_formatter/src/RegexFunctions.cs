using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text.RegularExpressions;
namespace DataProcessor
{
    class SpecialRegexMatchResult
    {
        public bool IsMatch { get; set; }
        public string? NewValue { get; set; } = null;

        public static SpecialRegexMatchResult Match(string specialRegexString, string text, string newTextRepresentation)
        {


            var fstChar = specialRegexString[0];
            if (fstChar == '=')
            {
                var regex = new Regex(specialRegexString.Substring(1));
                var match = regex.Match(text);
                if (match.Success)
                {
                    var fstChar2 = newTextRepresentation[0];
                    if (fstChar2 == '=')
                    {
                        var result = newTextRepresentation.Substring(1);
                        var counter = 0;
                        foreach (Group group in match.Groups)
                        {
                            result = result.Replace($"#[{counter}]", group.Value);

                            counter++;
                        }
                        return new SpecialRegexMatchResult { IsMatch = true, NewValue = result };

                    }
                    else
                    {
                        return new SpecialRegexMatchResult { IsMatch = true, NewValue = newTextRepresentation };
                    }
                }
                else
                {
                    return new SpecialRegexMatchResult { IsMatch = false, NewValue = null };
                }
            }
            else
            {
                if (text == specialRegexString)
                {
                    return new SpecialRegexMatchResult { IsMatch = true, NewValue = newTextRepresentation };
                }
                else
                {
                    return new SpecialRegexMatchResult { IsMatch = false, NewValue = null };
                }
            }
            throw new Exception("Invalid special regex string: " + specialRegexString);

            
        }
    }
}
