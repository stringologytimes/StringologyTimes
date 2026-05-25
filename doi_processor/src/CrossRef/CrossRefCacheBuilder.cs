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

            DataProcessor.CrossRefGZFileToDOICache.Build(dataFolderPath);

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(dataFolderPath);
            }

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

            var mapperFromTitleToDOI = new Dictionary<string, string>();
            doiElementDict.ToList().ForEach((v) =>
            {
                if (v.Value.Title.Length > 0)
                {
                    mapperFromTitleToDOI[v.Value.Title] = v.Value.DOI;
                }
            });

            doiElementDict.ToList().ForEach((v) =>
            {
                if (mapperFromTitleToDOI.ContainsKey(v.Value.ContainerTitle))
                {
                    v.Value.ContainerDOI = mapperFromTitleToDOI[v.Value.ContainerTitle];
                }
            });


            return doiElementDict;
        }

        public static void UpdateSmallCacheUsingContainerTitle(string dataFolderPath)
        {
            var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);
            var crossRefDicFromContainerTitleToDOI = DataProcessor.CrossRefDOIToGZFileCache.BuildDictionaryFromContainerTitleToDOI(filePath);

            var additionalCrossRefDOISet = new HashSet<string>();

            var doiElementDict = LoadSmallCache(dataFolderPath);
            doiElementDict.ToList().ForEach((v) =>
            {
                if (crossRefDicFromContainerTitleToDOI.ContainsKey(v.Value.ContainerTitle))
                {
                    additionalCrossRefDOISet.Add(v.Value.ContainerDOI);
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



    }
}
