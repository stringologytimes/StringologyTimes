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
            CrossRefFoundDOICache.Update(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);


            var unknownDOIFilePath = CrossRefExternalFoundDOICache.GetUnknownDOIFilePath(dataFolderPath);
            var unknownDOIDictionary = CSVFunctions.ReadCSVAasDictionary(unknownDOIFilePath);
            await CrossRefExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOIDictionary, mailAddress);


            CSVFunctions.WriteCSVAsDictionary(unknownDOIFilePath, unknownDOIDictionary);


            Console.WriteLine("CrossRefSmallCache [END]");

        }

        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/load_small_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var doiElementDict = new Dictionary<string, DOIElement>();
            var crossRefDic = DataProcessor.CrossRefFoundDOICache.Load(dataFolderPath);
            crossRefDic.ToList().ForEach((v) =>
            {
                var doiElement = CrossRefParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });
            var crossRefExternalDic = DataProcessor.CrossRefExternalFoundDOICache.Load(dataFolderPath);
            crossRefExternalDic.ToList().ForEach((v) =>
            {
                var doiElement = CrossRefParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });

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


            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return doiElementDict;
        }


        public static void UpdateSmallCacheUsingContainerDOI(string dataFolderPath)
        {
            var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);
            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var titleDictionary = CSVFunctions.ReadCSVAasDictionary(titleFilePath);



            var additionalCrossRefDOISet = new HashSet<string>();

            var doiElementDict = LoadSmallCache(dataFolderPath);
            doiElementDict.ToList().ForEach((v) =>
            {
                if (v.Value.ContainerDOI.Length > 0)
                {
                    if (!additionalCrossRefDOISet.Contains(v.Value.ContainerDOI) && !doiElementDict.ContainsKey(v.Value.ContainerDOI))
                    {
                        additionalCrossRefDOISet.Add(v.Value.ContainerDOI);
                    }
                }
            });
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
            DataProcessor.CrossRefFoundDOICache.Update(additionalCrossRefDOISet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);
        }


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
