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
        public List<string> ContainerTitleList { get; set; } = new List<string>();
        public List<string> FullNameList { get; set; } = new List<string>();
        public List<string> YearList { get; set; } = new List<string>();
        public List<string> MonthList { get; set; } = new List<string>();
        public List<string> VolumeList { get; set; } = new List<string>();
        public List<string> TypeList { get; set; } = new List<string>();
        //public List<string> TitleSizeList { get; set; } = new List<string>();
        public List<string> CompressedFullNameList { get; set; } = new List<string>();
        public List<string> CompressedTitleList { get; set; } = new List<string>();
        public List<string> CompressedDOIReferenceList { get; set; } = new List<string>();
        //public List<string> DOIReferenceSizeList { get; set; } = new List<string>();
        public List<string> DOIFlagList { get; set; } = new List<string>();

        public static LightweightDOIElementComponent Build(Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            LightweightDOIElementComponent r = new LightweightDOIElementComponent();

            List<DOIElement> primaryDOIElementList = primaryDOIElementDict.Values.ToList();
            primaryDOIElementList.Sort((a, b) => a.DOI.CompareTo(b.DOI));

            List<DOIElement> secondaryDOIElementList = secondaryDOIElementDict.Values.ToList();
            secondaryDOIElementList.Sort((a, b) => a.DOI.CompareTo(b.DOI));

            HashSet<string> unknownDOISet = new HashSet<string>();
            primaryDOIElementList.ForEach((v) =>
            {
                v.DOIReferences.ForEach((v) =>
                {
                    unknownDOISet.Add(v);
                });
            });
            List<string> unknownDOIList = unknownDOISet.ToList();
            unknownDOIList.Sort((a, b) => a.CompareTo(b));


            List<DOIElement> mergedDOIElementList = primaryDOIElementList.Concat(secondaryDOIElementList).ToList();
            Dictionary<string, int> doiReferenceMapper = new Dictionary<string, int>();
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

            }
            HashSet<string> fullNameHashSet = new HashSet<string>();

            mergedDOIElementList.ForEach((v) =>
            {
                v.Authors.ForEach((v) =>
                {
                    if (!fullNameHashSet.Contains(v.TryGetFullName()))
                    {
                        fullNameHashSet.Add(v.TryGetFullName());
                        r.FullNameList.Add(v.TryGetFullName());
                    }
                });
            });
            r.FullNameList.Sort((a, b) => a.CompareTo(b));
            Dictionary<string, int> fullNameToIndexMapper = new Dictionary<string, int>();
            for (var i = 0; i < r.FullNameList.Count; i++)
            {
                fullNameToIndexMapper[r.FullNameList[i]] = i;
            }

            mergedDOIElementList.ForEach((v) =>
            {
                var compStr = String.Join(",", v.Authors.Select((v) => fullNameToIndexMapper[v.TryGetFullName()].ToString()));
                r.CompressedFullNameList.Add(compStr);
            });

            mergedDOIElementList.ForEach((v) =>
            {
                r.ContainerTitleList.Add(v.ContainerTitle);
            });






            HashSet<string> wordHashSet = new HashSet<string>();
            mergedDOIElementList.ForEach((v) =>
            {
                v.Title.Split(" ").ToList().ForEach((w) =>
                {
                    if (!wordHashSet.Contains(w))
                    {
                        wordHashSet.Add(w);
                        r.WordList.Add(w);
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
                    r.DOIList.Add(v.DOI);
                    r.YearList.Add(v.Year);
                    r.MonthList.Add(v.Month);
                    r.VolumeList.Add(v.Volume);
                    r.TypeList.Add(v.Type);
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

                    var compStr2 = String.Join(",", v.Title.Split(" ").Select((w) => wordToIndexMapper[w].ToString()));
                    r.CompressedTitleList.Add(compStr2);
                    counter++;
                });
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
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_full_name.csv.gz", CompressedFullNameList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_title.csv.gz", CompressedTitleList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/compressed_doi_reference.csv.gz", CompressedDOIReferenceList);
            CSVFunctions.WriteCSVByGZip(outputFolder + "/doi_flag.csv.gz", DOIFlagList);
        }
    }
}
