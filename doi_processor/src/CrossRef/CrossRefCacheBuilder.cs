using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace DataProcessor
{
    class CrossRefCacheBuilder
    {
        public static DirectoryInfo SearchCrossRefFolder(string externalFolderPath)
        {
            DirectoryInfo di = new DirectoryInfo(externalFolderPath);
            if (di.Exists)
            {
                string CrossRefFilePath = "";

                foreach (var dir in Directory.GetDirectories(di.FullName)) // 直下のフォルダのみ
                {
                    var containsGZ = false;
                    var nameCheck = dir.IndexOf("Crossref") != -1;


                    foreach (var file in Directory.EnumerateFiles(dir)) // 直下のファイルのみ
                    {
                        FileInfo fi = new FileInfo(file);

                        if (fi.Extension == ".gz")
                        {
                            containsGZ = true;
                        }
                    }
                    if (containsGZ && nameCheck)
                    {
                        CrossRefFilePath = dir;
                    }
                }
                if (CrossRefFilePath != "")
                {
                    return new DirectoryInfo(CrossRefFilePath);
                }
            }
            throw new Exception("CrossRef folder not found");
        }

        public static void BuildBigCache(string dataFolderPath)
        {
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

            DataProcessor.CrossRefGZFileToDOICache.Build(dataFolderPath, crossrefFolderInfo.FullName);

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(dataFolderPath);
            }

            MinorCache.BuildISSNFile(dataFolderPath);
            MinorCache.BuildISBNFile(dataFolderPath);
            MinorCache.BuildTitleFile(dataFolderPath);
            MinorCache.BuildTypeListFile(dataFolderPath);
            //BuildBookCache(dataFolderPath, crossRefDoiListFolderPath);

        }


        public static async Task UpdateSmallCache(string dataFolderPath, Dictionary<string, DOICacheInfo> doiCacheInfoDict, string mailAddress)
        {
            Console.WriteLine("Building CrossRefSmallCache [START]");
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }
            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);

            // Build Found DOI Cache
            CrossRefLocalCache.Update(doiCacheInfoDict, crossRefDOIPrefixSet, dataFolderPath, crossrefFolderInfo.FullName);
            CrossRefLocalCache.UpdateDOICache(doiCacheInfoDict, dataFolderPath);
            await CrossRefExternalCache.Update(dataFolderPath, doiCacheInfoDict, crossRefDOIPrefixSet, mailAddress);
            CrossRefExternalCache.UpdateDOICache(doiCacheInfoDict, crossRefDOIPrefixSet, dataFolderPath);

            Console.WriteLine("CrossRefSmallCache [END]");

        }



        /*
                public static async Task UpdateSmallCache(string dataFolderPath, ReadOnlySet<string> doiSet, string mailAddress)
                {
                    Console.WriteLine("Building CrossRefSmallCache [START]");
                    var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

                    var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
                    var otherCSVFileInfo = new FileInfo(otherCSVPath);
                    if (!otherCSVFileInfo.Exists)
                    {
                        throw new Exception("others.tsv not found");
                    }

                    // Build Found DOI Cache
                    CrossRefLocalCache.Update(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);


                    var unknownDOIFilePath = CrossRefExternalCache.GetUnknownDOIFilePath(dataFolderPath);
                    var unknownDOIDictionary = CSVFunctions.ReadCSVAasDictionary(unknownDOIFilePath);
                    await CrossRefExternalCache.Build(dataFolderPath, doiSet, unknownDOIDictionary, mailAddress);


                    CSVFunctions.WriteCSVAsDictionary(unknownDOIFilePath, unknownDOIDictionary);


                    Console.WriteLine("CrossRefSmallCache [END]");

                }
                */

        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath,
        IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> crossRefDOIPrefixSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/load_small_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var doiElementDict = new Dictionary<string, DOIElement>();
            var crossRefDic = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);
            crossRefDic.ToList().ForEach((v) =>
            {
                var doiElement = CrossRefParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });
            var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);
            crossRefExternalDic.ToList().ForEach((v) =>
            {
                var doiElement = CrossRefParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });


            var lambdaLoadFunction = (DOICacheInfo v, bool isPrimary) =>
            {
                var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (crossRefDOIPrefixSet.Contains(doiPrefix))
                {
                    if (crossRefDic.ContainsKey(v.DOI))
                    {
                        var r = CrossRefParser.Parse(crossRefDic[v.DOI]);
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                    else if (crossRefExternalDic.ContainsKey(v.DOI))
                    {
                        var r = CrossRefParser.Parse(crossRefExternalDic[v.DOI]);
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                    else
                    {
                        var r = new DOIElement() { DOI = v.DOI, Source = "CrossRef:NotFound" };
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                }
                else
                {
                    return null;
                }
            };




            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = lambdaLoadFunction(v, v.IsPrimary);
                if (doiElement != null)
                {
                    doiElementDict[v.DOI] = doiElement;
                }
            });




            /*

            var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
            var isbnDictionary = CSVFunctions.ReadCSVAasDictionary(isbnFilePath);
            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var titleDictionary = CSVFunctions.ReadCSVAasDictionary(titleFilePath);

            logFile.WriteLine("DOI Dictionary: " + doiElementDict.Count);

            logFile.WriteLine("ISBN Dictionary: " + isbnDictionary.Count);
            logFile.WriteLine("Title Dictionary: " + titleDictionary.Count);

            doiElementDict.ToList().ForEach((v) =>
            {
                if (v.Value.ContainerDOI.Length == 0)
                {
                    for (int i = 0; i < v.Value.ISBNList.Count; i++)
                    {
                        var ISBN = v.Value.ISBNList[i];
                        if (isbnDictionary.ContainsKey(ISBN) && ISBN.Length > 0)
                        {
                            v.Value.ContainerDOI = isbnDictionary[ISBN];
                            logFile.WriteLine("Matched ISBN: " + v.Value.DOI + " -> " + v.Value.ContainerDOI);
                            break;
                        }
                    }
                }

                if (v.Value.ContainerDOI.Length == 0 && v.Value.ContainerTitle.Length > 0)
                {
                    if (titleDictionary.ContainsKey(v.Value.ContainerTitle))
                    {
                        v.Value.ContainerDOI = titleDictionary[v.Value.ContainerTitle];
                        logFile.WriteLine("Matched Container Title: " + v.Value.DOI + " -> " + v.Value.ContainerDOI);
                    }
                }

                if (v.Value.ContainerDOI.Length == 0)
                {
                    var isbnString = string.Join(",", v.Value.ISBNList);
                    var titleString = v.Value.ContainerTitle;

                    var bit = "";
                    for (int i = 0; i < v.Value.ISBNList.Count; i++)
                    {
                        var ISBN = v.Value.ISBNList[i];
                        if (isbnDictionary.ContainsKey(ISBN))
                        {
                            bit += "1";
                        }
                        else
                        {
                            bit += "0";
                        }
                    }

                    logFile.WriteLine("No Container DOI found: " + v.Value.DOI + " / " + isbnString + " / " + titleString + " / " + bit);
                }
            });
            */


            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return doiElementDict;
        }


        public static void UpdateContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> crossRefDOIPrefixSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/update_container_doi.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);

            var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
            var isbnDictionary = CSVFunctions.ReadCSVAasDictionary(isbnFilePath);
            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var titleDictionary = CSVFunctions.ReadCSVAasDictionary(titleFilePath);

            var doiElementDict = LoadSmallCache(dataFolderPath, doiCacheInfoDict, crossRefDOIPrefixSet);



            doiCacheInfoDict.Values.ToList().ForEach((w) =>
            {
                var v = doiElementDict[w.DOI];
                if (w.ContainerDOI.Length == 0)
                {
                    if (v.ContainerDOI.Length > 0)
                    {
                        w.ContainerDOI = v.ContainerDOI;
                    }
                }


                if (w.ContainerDOI.Length == 0)
                {
                    for (int i = 0; i < v.ISBNList.Count; i++)
                    {
                        var ISBN = v.ISBNList[i];
                        if (isbnDictionary.ContainsKey(ISBN) && ISBN.Length > 0)
                        {
                            w.ContainerDOI = isbnDictionary[ISBN];
                            logFile.WriteLine("Matched ISBN: " + w.DOI + " -> " + w.ContainerDOI);
                            break;
                        }
                    }
                }

                if (w.ContainerDOI.Length == 0 && v.ContainerTitle.Length > 0)
                {
                    if (titleDictionary.ContainsKey(v.ContainerTitle))
                    {
                        w.ContainerDOI = titleDictionary[v.ContainerTitle];
                        logFile.WriteLine("Matched Container Title: " + w.DOI + " -> " + w.ContainerDOI);
                    }
                }

                /*
                if (w.ContainerDOI.Length == 0)
                {
                    var isbnString = string.Join(",", v.ISBNList);
                    var titleString = v.ContainerTitle;

                    var bit = "";
                    for (int i = 0; i < v.ISBNList.Count; i++)
                    {
                        var ISBN = v.ISBNList[i];
                        if (isbnDictionary.ContainsKey(ISBN))
                        {
                            bit += "1";
                        }
                        else
                        {
                            bit += "0";
                        }
                    }

                    logFile.WriteLine("No Container DOI found: " + v.DOI + " / " + isbnString + " / " + titleString + " / " + bit);
                }
                */

            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

        }




        /*

                    public static void UpdateSmallCacheUsingDOIPrefix(string dataFolderPath)
                    {
                        var logFilePath = dataFolderPath + "/auto_generated/log/update_small_cache_using_doi_prefix.log";
                        var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

                        var doiPrefixSettingPath = dataFolderPath + "/raw/small_cache_setting/doi_prefix.tsv";
                        var doiPrefixDictionary = CSVFunctions.ReadCSVAasDictionary(doiPrefixSettingPath);

                        var additionalCrossRefDOISet = new HashSet<string>();

                        var doiElementDict = LoadSmallCache(dataFolderPath);
                        doiElementDict.ToList().ForEach((v) =>
                        {
                            var doi = v.Value.DOI;
                            doiPrefixDictionary.ToList().ForEach((w) =>
                            {
                                var regexMatchResult = SpecialRegexMatchResult.Match(w.Key, doi, w.Value);
                                if (regexMatchResult.IsMatch && regexMatchResult.NewValue != null)
                                {
                                    if (!additionalCrossRefDOISet.Contains(regexMatchResult.NewValue))
                                    {
                                        logFile.WriteLine("Matched DOI: " + doi + " -> " + regexMatchResult.NewValue);
                                    }
                                    additionalCrossRefDOISet.Add(regexMatchResult.NewValue);
                                }
                            });
                        });
                        var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
                        DataProcessor.CrossRefFoundDOICache.Update(additionalCrossRefDOISet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);
                        logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
                        logFile.Close();
                    }
                    */

            /*
                    public static void UpdateSmallCacheUsingISBN(string dataFolderPath)
                    {
                        var logFilePath = dataFolderPath + "/auto_generated/log/update_small_cache_using_isbn.log";
                        var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

                        var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
                        var isbnDictionary = CSVFunctions.ReadCSVAasDictionary(isbnFilePath);

                        var additionalCrossRefDOISet = new HashSet<string>();

                        var doiElementDict = LoadSmallCache(dataFolderPath);
                        doiElementDict.ToList().ForEach((v) =>
                        {
                            v.Value.ISBNList.ForEach((w) =>
                            {
                                if (isbnDictionary.ContainsKey(w))
                                {
                                    var foundDOI = isbnDictionary[w];
                                    if (!doiElementDict.ContainsKey(foundDOI))
                                    {
                                        if (!additionalCrossRefDOISet.Contains(foundDOI))
                                        {
                                            logFile.WriteLine("Matched DOI: " + v.Value.DOI + " -> " + foundDOI);
                                        }
                                        additionalCrossRefDOISet.Add(foundDOI);
                                    }

                                }
                            });
                        });

                        var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
                        DataProcessor.CrossRefFoundDOICache.Update(additionalCrossRefDOISet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);
                        logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
                        logFile.Close();

                    }
                    */




        }
}
