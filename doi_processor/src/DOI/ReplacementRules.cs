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
        /*
        public static void ReplaceDOI(string dataFolderPath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(dataFolderPath + "/raw");
            doiToTagMapper.Keys.ToList().ForEach((doi) =>
            {
                if (doiToTagMapper.ContainsKey(doi) && doiToTagMapper[doi].Count > 0)
                {
                    Console.WriteLine($"DOI: {doi} -> {string.Join(',', doiToTagMapper[doi])}");

                }
            });


            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v.DOI))
                {
                    v.Tags.AddRange(doiToTagMapper[v.DOI]);
                }
            });
            secondaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v.DOI))
                {
                    v.Tags.AddRange(doiToTagMapper[v.DOI]);
                }
            });
        }
        */

        public static void AppendTags(string dataFolderPath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/append_tags.log";
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            var logFile = new StreamWriter(logFilePath);
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(dataFolderPath + "/raw");
            doiToTagMapper.Keys.ToList().ForEach((doi) =>
            {
                if (doiToTagMapper.ContainsKey(doi) && doiToTagMapper[doi].Count > 0)
                {
                    logFile.WriteLine($"DOI: {doi} -> {string.Join(',', doiToTagMapper[doi])}");

                }
            });


            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v.DOI))
                {
                    v.Tags.AddRange(doiToTagMapper[v.DOI]);
                }
            });
            secondaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v.DOI))
                {
                    v.Tags.AddRange(doiToTagMapper[v.DOI]);
                }
            });
        }


        public static void ReplaceContainerTitleUsingDBLPSummary(string dblpSummaryPath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            if (File.Exists(dblpSummaryPath))
            {
                var dblpProceedingsSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(dblpSummaryPath);
                dblpProceedingsSeriesDictionary.BuildDoiToBookTitleMapper();

                primaryDOIElementDict.Keys.ToList().ForEach((key) =>
                {
                    var doiElement = primaryDOIElementDict[key];
                    var doi = doiElement.DOI;
                    var bookTitle = dblpProceedingsSeriesDictionary.SearchBookTitleByDOI(doi);
                    /*

                    if (doi.StartsWith("10.4230/lipics.approx/random.2020.35"))
                    {
                        Console.WriteLine($"No Hit: {doi}/" + bookTitle);
                    }
                    */

                    if (bookTitle != null)
                    {
                        //Console.WriteLine($"Replaced container title: {doiElement.ContainerTitle} -> {bookTitle}");
                        doiElement.ContainerTitle = bookTitle;
                    }

                });

                secondaryDOIElementDict.Keys.ToList().ForEach((key) =>
                {
                    var doiElement = secondaryDOIElementDict[key];
                    var doi = doiElement.DOI;
                    var bookTitle = dblpProceedingsSeriesDictionary.SearchBookTitleByDOI(doi);
                    if (bookTitle != null)
                    {
                        //Console.WriteLine($"Replaced container title: {doiElement.ContainerTitle} -> {bookTitle}");
                        doiElement.ContainerTitle = bookTitle;
                    }
                });
            }
            else
            {
                Console.WriteLine("NoDBLP summary file found: " + dblpSummaryPath);
            }
        }
        public static void ReplaceType(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/type_replacement_rules.log";
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            var logFile = new StreamWriter(logFilePath);

            if (File.Exists(rulePath))
            {
                var typeReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var typeReplacementRulesDict = new Dictionary<string, string>();
                typeReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split('\t');
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    typeReplacementRulesDict[key] = value;
                    logFile.WriteLine($"Added type replacement rule: {key} -> {value}");
                });

                replace(typeReplacementRulesDict, "Type", primaryDOIElementDict, logFile);
                replace(typeReplacementRulesDict, "Type", secondaryDOIElementDict, logFile);
            }
            else
            {
                logFile.WriteLine("No type replacement rules file found: " + rulePath);
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

        public static void ReplaceContainerTitleByDOIPrefix(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/doi_prefix_key_container_title_value.log";
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            var logFile = new StreamWriter(logFilePath);
            if (File.Exists(rulePath))
            {
                var typeReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var typeReplacementRulesList = new List<Tuple<string, string>>();
                typeReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split('\t');
                    if (cols.Length != 2)
                    {
                        logFile.WriteLine($"Invalid DOI prefix rule: {v}");
                    }
                    else
                    {
                        var key = cols[0].Trim().ToLower();
                        var value = cols[1].Trim();
                        typeReplacementRulesList.Add(new Tuple<string, string>(key, value));
                        logFile.WriteLine($"Added DOI prefix rule: {key} -> {value}");
                    }
                });

                replace2(typeReplacementRulesList, "ContainerTitle", primaryDOIElementDict);
                replace2(typeReplacementRulesList, "ContainerTitle", secondaryDOIElementDict);
            }
            else
            {
                logFile.WriteLine("No DOI prefix rule file found: " + rulePath);
            }
        }
        public static void ReplaceTypeByDOIPrefix(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            if (File.Exists(rulePath))
            {
                var typeReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var typeReplacementRulesList = new List<Tuple<string, string>>();
                typeReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split('\t');
                    var key = cols[0].Trim().ToLower();
                    var value = cols[1].Trim();
                    typeReplacementRulesList.Add(new Tuple<string, string>(key, value));
                    Console.WriteLine($"Added DOI prefix rule: {key} -> {value}");
                });

                replace2(typeReplacementRulesList, "Type", primaryDOIElementDict);
                replace2(typeReplacementRulesList, "Type", secondaryDOIElementDict);
            }
            else
            {
                Console.WriteLine("No type replacement rules file found: " + rulePath);
            }
        }

        public static void ReplaceContainerTitle(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/container_title_replacement_rules.log";
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            var logFile = new StreamWriter(logFilePath);

            if (File.Exists(rulePath))
            {
                var replacementRules = CSVFunctions.ReadCSV(rulePath);
                var replacementRulesDict = new Dictionary<string, string>();
                replacementRules.ForEach((v) =>
                {
                    var cols = v.Split('\t');
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    replacementRulesDict[key] = value;
                    logFile.WriteLine($"Added type replacement rule: {key} -> {value}");
                });

                replace(replacementRulesDict, "ContainerTitle", primaryDOIElementDict, logFile);
                replace(replacementRulesDict, "ContainerTitle", secondaryDOIElementDict, logFile);
            }
            else
            {
                Console.WriteLine("No container-title replacement rules file found: " + rulePath);
            }

        }

        private static bool MatchCheck(string keyWithMark, string value)
        {
            if (keyWithMark.Length == 0)
            {
                return false;
            }
            var key = keyWithMark[0] == '=' ? keyWithMark.Substring(1) : keyWithMark;

            if (keyWithMark[0] == '=')
            {

                Regex regex = new Regex(key);
                Match match = regex.Match(value);
                return match.Success;
            }
            else
            {
                return key == value;
            }

        }

        private static void replace2(List<Tuple<string, string>> rules, string replacedPropertyName, Dictionary<string, DOIElement> DOIElementDict)
        {
            DOIElementDict.Values.ToList().ForEach((DOIElement v) =>
            {
                foreach (var rule in rules)
                {
                    // rule.Item1がv.DOIのprefixであるかどうかを判定
                    if (!string.IsNullOrEmpty(v.DOI) && !string.IsNullOrEmpty(rule.Item1))
                    {
                        if (v.DOI.StartsWith(rule.Item1))
                        {
                            if (replacedPropertyName == "ContainerTitle")
                            {
                                v.ContainerTitle = rule.Item2;
                            }
                            else if (replacedPropertyName == "Type")
                            {
                                v.Type = rule.Item2;

                            }
                        }
                    }
                }
            });
        }


        private static void replace(Dictionary<string, string> rules, string replacedPropertyName, Dictionary<string, DOIElement> DOIElementDict, StreamWriter logFile)
        {
            var keyList = rules.Keys.ToList();
            HashSet<string> replacedNameSet = new HashSet<string>();
            DOIElementDict.Values.ToList().ForEach((DOIElement v) =>
            {
#pragma warning disable CS0219 
                bool isMatched = false;
#pragma warning restore CS0219

                foreach (var keyWithMark in keyList)
                {
                    var value = replacedPropertyName == "ContainerTitle" ? v.ContainerTitle : v.Type;
                    var isKeyMatched = MatchCheck(keyWithMark, value);

                    if (isKeyMatched)
                    {
                        if (replacedPropertyName == "ContainerTitle")
                        {
                            if (!replacedNameSet.Contains(v.ContainerTitle))
                            {
                                replacedNameSet.Add(v.ContainerTitle);
                                logFile.WriteLine($"Replaced container title: {v.ContainerTitle} -> {rules[keyWithMark]}");
                            }
                            v.ContainerTitle = rules[keyWithMark];
                            isMatched = true;
                            break;

                        }
                        else if (replacedPropertyName == "Type")
                        {
                            if (!replacedNameSet.Contains(v.Type))
                            {
                                replacedNameSet.Add(v.Type);
                                logFile.WriteLine($"Replaced type: {v.Type} -> {rules[keyWithMark]}");
                            }
                            v.Type = rules[keyWithMark];
                        }
                    }
                }

                /*
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
                */


            });
        }



    }
}