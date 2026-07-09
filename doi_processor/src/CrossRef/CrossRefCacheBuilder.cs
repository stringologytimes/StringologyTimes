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

            // Build Found DOI Cache
            CrossRefLocalCache.Update(doiCacheInfoDict, dataFolderPath, crossrefFolderInfo.FullName);
            CrossRefLocalCache.UpdateDOICache(doiCacheInfoDict, dataFolderPath);
            await CrossRefExternalCache.Update(dataFolderPath, doiCacheInfoDict, mailAddress);
            CrossRefExternalCache.UpdateDOICache(doiCacheInfoDict, dataFolderPath);

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


            var crossRefDic = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);
            var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);

            var mergedDic = new Dictionary<string, DOIElement>();

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "CrossRef")
                {
                    if (crossRefDic.ContainsKey(v.DOI))
                    {
                        var doiElement = CrossRefParser.Parse(crossRefDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else if (crossRefExternalDic.ContainsKey(v.DOI))
                    {
                        var doiElement = CrossRefParser.Parse(crossRefExternalDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else
                    {
                        var doiElement = new DOIElement() { DOI = v.DOI, Source = "CrossRef", IsPrimary = v.DOIRank == 0 };
                        mergedDic[v.DOI] = doiElement;
                    }
                }
            });

            mergedDic.ToList().ForEach((v) =>
            {
                v.Value.UpdateContainerDOI(doiCacheInfoDict[v.Key]);
            });


            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return mergedDic;
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
