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

            CrossRefSubMapperBuilders.BuildISBNMapper(dataFolderPath);
            CrossRefSubMapperBuilders.BuildISSNMapper(dataFolderPath);
            CrossRefSubMapperBuilders.BuildTitleMapper(dataFolderPath);
            CrossRefSubMapperBuilders.BuildAliasMapper(dataFolderPath);
            //MinorCache.BuildTitleFile(dataFolderPath);
            CrossRefSubMapperBuilders.BuildTypeListFile(dataFolderPath);
            //BuildBookCache(dataFolderPath, crossRefDoiListFolderPath);

        }


        public static async Task UpdateSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, string mailAddress)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating SmallCache(CrossRef) [START]");
            CommonFunctions.IncrementParagraphCounter();
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            // Build Found DOI Cache
            CrossRefLocalCache.Update(doiCacheInfoDict, crossRefSmallCache, dataFolderPath, crossrefFolderInfo.FullName);
            CrossRefLocalCache.UpdateDOICacheStatus(doiCacheInfoDict, crossRefSmallCache);
            await CrossRefExternalCache.Update(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, mailAddress);
            CrossRefExternalCache.UpdateDOICache(doiCacheInfoDict, crossRefSmallCache);

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating SmallCache(CrossRef) [END]");

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
