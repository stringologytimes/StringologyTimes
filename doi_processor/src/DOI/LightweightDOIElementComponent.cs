using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;

namespace DataProcessor
{
    public class LightweightDOIElementComponent
    {
        public List<string> DOIList { get; set; } = new List<string>();
        public List<string> WordList { get; set; } = new List<string>();
        public List<string> SeriesTitleList { get; set; } = new List<string>();
        public List<string> ContainerDOIList { get; set; } = new List<string>();
        public List<string> ContainerTitleList { get; set; } = new List<string>();
        public List<string> FullNameList { get; set; } = new List<string>();
        public List<string> YearList { get; set; } = new List<string>();
        public List<string> MonthList { get; set; } = new List<string>();
        public List<string> VolumeList { get; set; } = new List<string>();
        public List<string> TypeList { get; set; } = new List<string>();
        public List<string> SourceList { get; set; } = new List<string>();
        //public List<string> TitleSizeList { get; set; } = new List<string>();
        public List<string> CompressedFullNameList { get; set; } = new List<string>();
        public List<string> CompressedTitleList { get; set; } = new List<string>();
        public List<string> CompressedDOIReferenceList { get; set; } = new List<string>();
        //public List<string> DOIReferenceSizeList { get; set; } = new List<string>();
        public List<string> DOIFlagList { get; set; } = new List<string>();

        public List<string> TagList { get; set; } = new List<string>();
        public List<string> TagListOfEachElement { get; set; } = new List<string>();

        public static string SanitizeWord(string word)
        {
            return word.Replace("\n", "");
        }

        public static LightweightDOIElementComponent Build(Dictionary<string, DOIElement> doiElementDict)
        {
            LightweightDOIElementComponent r = new LightweightDOIElementComponent();
            HashSet<string> knownDOISet = new HashSet<string>();

            var strFunc = (DOIElement a) => { return (a.IsPrimary ? "0" : "1") + a.DOI; };

            List<DOIElement> doiElementList = doiElementDict.Values.ToList();
            doiElementList.Sort((a, b) => strFunc(a).CompareTo(strFunc(b)));

            doiElementList.ForEach((v) =>
            {
                knownDOISet.Add(v.DOI);
            });

            HashSet<string> unknownDOISet = new HashSet<string>();
            doiElementList.ForEach((v) =>
            {
                v.DOIReferences.ForEach((v) =>
                {
                    if (!knownDOISet.Contains(v))
                    {
                        unknownDOISet.Add(v);
                    }
                });
            });
            List<string> unknownDOIList = unknownDOISet.ToList();
            unknownDOIList.Sort((a, b) => a.CompareTo(b));

            List<DOIElement> mergedDOIElementList = doiElementList.ToList();
            unknownDOIList.ForEach((v) =>
            {
                mergedDOIElementList.Add(new DOIElement() { DOI = v });
            });


            var primaryDOIElementCount = doiElementList.Count(v => v.IsPrimary);
            var secondaryDOIElementCount = doiElementList.Count(v => !v.IsPrimary);

            Dictionary<string, int> doiReferenceMapper = new Dictionary<string, int>();


            for (var i = 0; i < mergedDOIElementList.Count; i++)
            {
                if (i < primaryDOIElementCount)
                {
                    r.DOIFlagList.Add("1");
                }
                else if (i < primaryDOIElementCount + secondaryDOIElementCount)
                {
                    r.DOIFlagList.Add("0");
                }
                else
                {
                    r.DOIFlagList.Add("-1");
                }
                r.DOIList.Add(mergedDOIElementList[i].DOI);
                doiReferenceMapper[mergedDOIElementList[i].DOI] = i;

            }




            /*

                        {
                            primaryDOIElementList.ForEach((v) =>
                            {
                                r.DOIList.Add(v.DOI);
                            });
                            secondaryDOIElementList.ForEach((v) =>
                            {
                                r.DOIList.Add(v.DOI);
                            });
                            unknownDOIList.ForEach((v) =>
                            {
                                r.DOIList.Add(v);
                            });
                        }
                        */


/*
            {
                var counter = 0;

                primaryDOIElementList.ForEach((v) =>
                {
                    r.DOIFlagList.Add("1");
                    doiReferenceMapper[v.DOI] = counter++;
                });
                secondaryDOIElementList.ForEach((v) =>
                {
                    r.DOIFlagList.Add("0");
                    doiReferenceMapper[v.DOI] = counter++;
                });
                unknownDOIList.ForEach((v) =>
                {
                    r.DOIFlagList.Add("-1");
                    doiReferenceMapper[v] = counter++;
                });

                Console.WriteLine("DOI Count: " + counter);
                Console.WriteLine("Primary DOI Count: " + primaryDOIElementList.Count);
                Console.WriteLine("Secondary DOI Count: " + secondaryDOIElementList.Count);
                Console.WriteLine("Unknown DOI Count: " + unknownDOIList.Count);
            }
            */
            HashSet<string> fullNameHashSet = new HashSet<string>();

            mergedDOIElementList.ForEach((v) =>
            {
                v.Authors.ForEach((v) =>
                {
                    string fullName = v.TryGetFullName();
                    if (fullName.IndexOf("\n") >= 0)
                    {
                        throw new Exception("Full name contains newline: " + fullName);
                    }
                    if (!fullNameHashSet.Contains(fullName))
                    {
                        fullNameHashSet.Add(fullName);
                        r.FullNameList.Add(fullName);
                    }
                });
            });
            r.FullNameList.Sort((a, b) => a.CompareTo(b));
            Dictionary<string, int> fullNameToIndexMapper = new Dictionary<string, int>();
            for (var i = 0; i < r.FullNameList.Count; i++)
            {
                fullNameToIndexMapper[r.FullNameList[i]] = i;
            }

            //var tmp_counter = 0;

            mergedDOIElementList.ForEach((v) =>
            {
                var compStr = String.Join(",", v.Authors.Select((v) => fullNameToIndexMapper[v.TryGetFullName()].ToString()));
                r.CompressedFullNameList.Add(compStr);
                r.SeriesTitleList.Add(v.SeriesTitle);
                //r.SeriesTitleList.Add((tmp_counter++).ToString());
                r.ContainerDOIList.Add(v.ContainerDOI);
                r.ContainerTitleList.Add(v.ContainerTitle);
            });


            HashSet<string> wordHashSet = new HashSet<string>();
            mergedDOIElementList.ForEach((v) =>
            {
                v.Title.Split(" ").ToList().ForEach((w) =>
                {
                    var sanitizedWord = SanitizeWord(w);
                    if (!wordHashSet.Contains(sanitizedWord))
                    {
                        wordHashSet.Add(sanitizedWord);
                        r.WordList.Add(sanitizedWord);
                    }
                });
            });
            r.WordList.Sort((a, b) => a.CompareTo(b));


            Dictionary<string, int> wordToIndexMapper = new Dictionary<string, int>();
            for (var i = 0; i < r.WordList.Count; i++)
            {
                wordToIndexMapper[r.WordList[i]] = i;
            }


            {

                var counter = 0;
                mergedDOIElementList.ForEach((v) =>
                {
                    var isPrimary = r.DOIFlagList[counter] == "1";
                    r.YearList.Add(v.Year);
                    r.MonthList.Add(v.Month);
                    r.VolumeList.Add(v.Volume);
                    r.TypeList.Add(v.Type);
                    r.SourceList.Add(v.Source);
                    //r.TitleSizeList.Add(v.Title.Length.ToString());
                    //r.CompressedTitleList.Add(v.Title.Length.ToString());
                    if (isPrimary)
                    {
                        var compStr1 = String.Join(",", v.DOIReferences.Select((v) => doiReferenceMapper[v].ToString()));
                        r.CompressedDOIReferenceList.Add(compStr1);
                    }
                    else
                    {
                        r.CompressedDOIReferenceList.Add("");
                    }

                    var compStr2 = String.Join(",", v.Title.Split(" ").Select((w) => wordToIndexMapper[SanitizeWord(w)].ToString()));
                    r.CompressedTitleList.Add(compStr2);
                    counter++;
                });
            }

            HashSet<string> tagHashSet = new HashSet<string>();
            mergedDOIElementList.ForEach((v) =>
            {
                v.Tags.ForEach((v) =>
                {
                    if (!tagHashSet.Contains(v))
                    {
                        tagHashSet.Add(v);
                    }
                });
            });

            r.TagList = tagHashSet.ToList();
            r.TagList.Sort((a, b) => a.CompareTo(b));

            Dictionary<string, int> tagToIndexMapper = new Dictionary<string, int>();
            for (var i = 0; i < r.TagList.Count; i++)
            {
                tagToIndexMapper[r.TagList[i]] = i;
            }

            mergedDOIElementList.ForEach((v) =>
            {
                var compStr = String.Join(",", v.Tags.Select((v) => tagToIndexMapper[v].ToString()));
                r.TagListOfEachElement.Add(compStr);
            });

            if (r.SeriesTitleList.Count != r.DOIList.Count)
            {
                Console.WriteLine("SeriesTitleList.Count: " + r.SeriesTitleList.Count);
                Console.WriteLine("DOIList.Count: " + r.DOIList.Count);
                throw new Exception("SeriesTitleList.Count != DOIList.Count");
            }

            if (r.ContainerDOIList.Count != r.DOIList.Count)
            {
                Console.WriteLine("ContainerDOIList.Count: " + r.ContainerDOIList.Count);
                Console.WriteLine("DOIList.Count: " + r.DOIList.Count);
                throw new Exception("ContainerDOIList.Count != DOIList.Count");
            }

            if (r.ContainerTitleList.Count != r.DOIList.Count)
            {
                Console.WriteLine("ContainerTitleList.Count: " + r.ContainerTitleList.Count);
                Console.WriteLine("DOIList.Count: " + r.DOIList.Count);
                throw new Exception("ContainerTitleList.Count != DOIList.Count");
            }

            return r;
        }
        public void OutputByGZip(string outputFolder)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }


            CSVFunctions.WriteCSVByGZip(outputFolder + "/doi.csv.gz", DOIList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/word.csv.gz", WordList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/container_title.csv.gz", ContainerTitleList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/full_name.csv.gz", FullNameList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/year.csv.gz", YearList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/month.csv.gz", MonthList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/volume.csv.gz", VolumeList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/type.csv.gz", TypeList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/source.csv.gz", SourceList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_full_name.csv.gz", CompressedFullNameList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/container_doi.csv.gz", ContainerDOIList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/series_title.csv.gz", SeriesTitleList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_title.csv.gz", CompressedTitleList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_doi_reference.csv.gz", CompressedDOIReferenceList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/doi_flag.csv.gz", DOIFlagList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/tag.csv.gz", TagList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/tag_of_each_element.csv.gz", TagListOfEachElement);
        }
    }
}
