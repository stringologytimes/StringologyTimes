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

        public static string Escape(string s)
        {
            return s.Replace("&amp;", "&")
                 .Replace("\r\n", " ")
                 .Replace("\n", " ")
                 .Replace("\r", " ");
        }

        public static void EscapeProcessing(Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/escape_container_title.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            var escapeLambda = (DOIElement v) =>
            {
                v.Title = Escape(v.Title);
                v.ContainerTitle = Escape(v.ContainerTitle);
                v.SeriesTitle = Escape(v.SeriesTitle);
                /*
                if (b)
                {
                    logFile.WriteLine($"Escaping container title: {v.ContainerTitle}");
                    v.ContainerTitle = v.ContainerTitle.Replace("&amp;", "&");
                }
                */
            };


            doiElementDict.Values.ToList().ForEach((v) =>
            {
                escapeLambda(v);
            });

            logFile.Close();
        }

        public static void AppendTags(string dataFolderPath, Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/append_tags.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(dataFolderPath + "/raw");
            doiToTagMapper.Keys.ToList().ForEach((doi) =>
            {
                if (doiToTagMapper.ContainsKey(doi) && doiToTagMapper[doi].Count > 0)
                {
                    logFile.WriteLine($"DOI: {doi} -> {string.Join(',', doiToTagMapper[doi])}");

                }
            });


            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v.DOI))
                {
                    v.Tags.AddRange(doiToTagMapper[v.DOI]);
                }
            });
        }


/*
        public static void ReplaceContainerTitleUsingDBLPSummary(string dblpSummaryPath, Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/replace_container_titles_using_dblp_summary.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            if (File.Exists(dblpSummaryPath))
            {
                var dblpProceedingsSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(dblpSummaryPath);
                dblpProceedingsSeriesDictionary.BuildDoiToSeriesTitleMapper();


                doiElementDict.Values.ToList().ForEach((doiElement) =>
                {
                    var doi = doiElement.DOI;
                    if (doiElement.ContainerDOI.Length > 0)
                    {
                        var seriesTitle = dblpProceedingsSeriesDictionary.SearchSeriesTitleByDOI(doiElement.ContainerDOI);
                        if (seriesTitle != null)
                        {
                            logFile.WriteLine($"#1 {doi}, {doiElement.SeriesTitle} -> {seriesTitle}");
                            doiElement.SeriesTitle = seriesTitle;

                        }
                    }
                    else
                    {
                        var seriesTitle = dblpProceedingsSeriesDictionary.SearchSeriesTitleByDOI(doi);
                        if (seriesTitle != null)
                        {
                            logFile.WriteLine($"#2 {doi}, {doiElement.SeriesTitle} -> {seriesTitle}");
                            doiElement.SeriesTitle = seriesTitle;

                        }
                    }

                });

            }
            else
            {
                Console.WriteLine("NoDBLP summary file found: " + dblpSummaryPath);
            }
            logFile.Close();
            Console.WriteLine("Log file: " + logFilePath);
        }
        */

        public static void ReplaceSeriesTitle(string rulePath, Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/series_title_replacement_rules.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            if (File.Exists(rulePath))
            {
                var seriesTitleReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var seriesTitleReplacementRulesDict = new Dictionary<string, string>();
                seriesTitleReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split('\t');
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    seriesTitleReplacementRulesDict[key] = value;
                    logFile.WriteLine($"Added series title replacement rule: {key} -> {value}");
                });


                replace(seriesTitleReplacementRulesDict, "SeriesTitle", doiElementDict, logFile);
            }
            else
            {
                logFile.WriteLine("No series title replacement rules file found: " + rulePath);
            }

            logFile.Close();
            Console.WriteLine("Log file: " + logFilePath);
        }

        public static void ReplaceType(string rulePath, Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/type_replacement_rules.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

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

                replace(typeReplacementRulesDict, "Type", doiElementDict, logFile);
            }
            else
            {
                logFile.WriteLine("No type replacement rules file found: " + rulePath);
            }

            var typeHashSet = new HashSet<string>();
            doiElementDict.Values.ToList().ForEach((v) =>
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

        public static void RpelaceContainerDOIByDOIPrefix(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/cotaniner_doi_replacement_rules.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");
            if (File.Exists(rulePath))
            {
                var ruleList = CSVFunctions.ReadCSVAsKeyValuePairList(rulePath);
                var mergedDictionary = new Dictionary<string, DOIElement>();
                primaryDOIElementDict.ToList().ForEach((v) =>
                {
                    mergedDictionary[v.Key] = v.Value;
                });
                secondaryDOIElementDict.ToList().ForEach((v) =>
                {
                    mergedDictionary[v.Key] = v.Value;
                });

                mergedDictionary.ToList().ForEach((v) =>
                {
                    var doi = v.Key;
                    var index = 0;
                    ruleList.ForEach((rule) =>
                    {
                        var regexMatchResult = SpecialRegexMatchResult.Match(rule.Key, doi, rule.Value);
                        if (regexMatchResult.IsMatch && regexMatchResult.NewValue != null)
                        {
                            v.Value.ContainerDOI = regexMatchResult.NewValue;
                            logFile.WriteLine("Replaced container DOI {0}: {1} -> {2}", index, doi, regexMatchResult.NewValue);

                            if (mergedDictionary.ContainsKey(v.Value.ContainerDOI))
                            {
                                v.Value.ContainerTitle = mergedDictionary[v.Value.ContainerDOI].Title;
                                v.Value.SeriesTitle = mergedDictionary[v.Value.ContainerDOI].SeriesTitle;
                            }
                        }
                        index++;
                    });
                });
            }
            else
            {
                logFile.WriteLine("No container DOI rule file found: " + rulePath);
            }
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
        }

        public static void ReplaceContainerTitleByDOIPrefix(string rulePath, Dictionary<string, DOIElement> doiElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/doi_prefix_key_container_title_value.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");
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

                replace2(typeReplacementRulesList, "ContainerTitle", doiElementDict);
            }
            else
            {
                logFile.WriteLine("No DOI prefix rule file found: " + rulePath);
            }
        }
        public static void ReplaceTypeByDOIPrefix(string rulePath, Dictionary<string, DOIElement> doiElementDict)
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

                replace2(typeReplacementRulesList, "Type", doiElementDict);
            }
            else
            {
                Console.WriteLine("No type replacement rules file found: " + rulePath);
            }
        }

        public static void ReplaceContainerTitle(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict, string logFolderPath)
        {
            var logFilePath = logFolderPath + "/container_title_replacement_rules.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

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
                    var value = "";
                    if (replacedPropertyName == "ContainerTitle")
                    {
                        value = v.ContainerTitle;
                    }
                    else if (replacedPropertyName == "Type")
                    {
                        value = v.Type;
                    }
                    else if (replacedPropertyName == "SeriesTitle")
                    {
                        value = v.DOI;
                    }

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
                        else if (replacedPropertyName == "SeriesTitle")
                        {
                            if (!replacedNameSet.Contains(v.SeriesTitle))
                            {
                                replacedNameSet.Add(v.SeriesTitle);
                                logFile.WriteLine($"Replaced series title: {v.DOI}, {v.SeriesTitle} -> {rules[keyWithMark]}");
                            }
                            v.SeriesTitle = rules[keyWithMark];
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