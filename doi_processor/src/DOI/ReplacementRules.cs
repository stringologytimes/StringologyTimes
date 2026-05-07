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
    public class ReplacementRules
    {
        public static void ReplaceType(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            if (File.Exists(rulePath))
            {
                var typeReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var typeReplacementRulesDict = new Dictionary<string, string>();
                typeReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split(",");
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    typeReplacementRulesDict[key] = value;
                    Console.WriteLine($"Added type replacement rule: {key} -> {value}");
                });

                replace(typeReplacementRulesDict, "Type", primaryDOIElementDict);
                replace(typeReplacementRulesDict, "Type", secondaryDOIElementDict);
            }
            else
            {
                Console.WriteLine("No type replacement rules file found: " + rulePath);
            }

            var typeHashSet = new HashSet<string>();
            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                typeHashSet.Add(v.Type);
            });
            secondaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                typeHashSet.Add(v.Type);
            });
            var typeList = typeHashSet.ToList();
            typeList.Sort();
            typeList.ForEach((v) =>
            {
                Console.WriteLine(v);
            });

        }

        public static void ReplaceContainerTitle(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            if (File.Exists(rulePath))
            {
                var replacementRules = CSVFunctions.ReadCSV(rulePath);
                var replacementRulesDict = new Dictionary<string, string>();
                replacementRules.ForEach((v) =>
                {
                    var cols = v.Split(",");
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    replacementRulesDict[key] = value;
                    Console.WriteLine($"Added type replacement rule: {key} -> {value}");
                });

                replace(replacementRulesDict, "ContainerTitle", primaryDOIElementDict);
                replace(replacementRulesDict, "ContainerTitle", secondaryDOIElementDict);
            }
            else
            {
                Console.WriteLine("No container-title replacement rules file found: " + rulePath);
            }

        }

        private static void replace(Dictionary<string, string> rules, string replacedPropertyName, Dictionary<string, DOIElement> DOIElementDict)
        {
            var keyList = rules.Keys.ToList();
            HashSet<string> replacedNameSet = new HashSet<string>();
            DOIElementDict.Values.ToList().ForEach((DOIElement v) =>
            {
                bool isMatched = false;
                foreach (var key in keyList)
                {
                    Regex regex = new Regex(key);

                    if (replacedPropertyName == "ContainerTitle")
                    {
                        Match match = regex.Match(v.ContainerTitle);
                        if (match.Success)
                        {
                            if (!replacedNameSet.Contains(v.ContainerTitle))
                            {
                                replacedNameSet.Add(v.ContainerTitle);
                                Console.WriteLine($"Replaced container title: {v.ContainerTitle} -> {rules[key]}");
                            }
                            v.ContainerTitle = rules[key];
                            isMatched = true;
                            break;
                        }
                    }
                    else if (replacedPropertyName == "Type")
                    {
                        Match match = regex.Match(v.Type);

                        if (match.Success)
                        {
                            if (!replacedNameSet.Contains(v.Type))
                            {
                                replacedNameSet.Add(v.Type);
                                Console.WriteLine($"Replaced type: {v.Type} -> {rules[key]}");
                            }
                            v.Type = rules[key];
                        }
                    }


                }
                if (!isMatched)
                {
                    if (replacedPropertyName == "ContainerTitle" && v.ContainerTitle.IndexOf("2016") != -1 && v.ContainerTitle.IndexOf("DCC") != -1)
                    {
                        foreach (char c in v.ContainerTitle)
                        {
                            Console.WriteLine($"{c} : {(int)c}");
                        }
                    }


                }


            });
        }



    }
}