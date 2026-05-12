using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DataProcessor
{
    class DBLPProceedings
    {
        public string DOI { get; set; } = "";
        public string key { get; set; } = "";
        public string BookTitle { get; set; } = "";
        public string Title { get; set; } = "";
        public int? Year { get; set; } = null;
        public int? Month { get; set; } = null;
        public string Volume { get; set; } = "";

        public List<string> DOIList { get; set; } = new List<string>();

        public string ToJSONLine()
        {
            List<object> dataList = new List<object>();
            dataList.Add(JsonSerializer.Serialize(this.DOI));
            dataList.Add(JsonSerializer.Serialize(this.key));
            dataList.Add(JsonSerializer.Serialize(this.BookTitle));
            dataList.Add(JsonSerializer.Serialize(this.Title));
            dataList.Add(JsonSerializer.Serialize(this.Year?.ToString() ?? ""));
            dataList.Add(JsonSerializer.Serialize(this.Month?.ToString() ?? ""));
            dataList.Add(JsonSerializer.Serialize(this.Volume));

            dataList.Add(JsonSerializer.Serialize(this.DOIList));

            string dataString = "[" + string.Join(",", dataList) + "]";
            return dataString;

        }
        public static DBLPProceedings BuildFromJSONLine(string jsonLine)
        {
            var dblpProceedings = new DBLPProceedings();
            var dataList = JsonSerializer.Deserialize<List<object>>(jsonLine);
            if (dataList == null)
            {
                throw new Exception("DataList is null");
            }


            dblpProceedings.DOI = dataList[0].ToString() ?? "";
            dblpProceedings.key = dataList[1].ToString() ?? "";

            if (dblpProceedings.key == "")
            {
                Console.WriteLine(dataList[1].ToString());
                throw new Exception("key is not found");
            }
            dblpProceedings.BookTitle = dataList[2].ToString() ?? "";
            dblpProceedings.Title = dataList[3].ToString() ?? "";
            var yearStr = dataList[4].ToString() ?? "";
            if (yearStr != "")
            {
                dblpProceedings.Year = int.Parse(yearStr);
            }
            else
            {
                dblpProceedings.Year = null;
            }
            var monthStr = dataList[5].ToString() ?? "";
            if (monthStr != "")
            {
                dblpProceedings.Month = int.Parse(monthStr);
            }
            else
            {
                dblpProceedings.Month = null;
            }

            dblpProceedings.Volume = dataList[6].ToString() ?? "";
            var doiListString = ((System.Text.Json.JsonElement)dataList[7]).Deserialize<List<string>>();
            if (doiListString != null && doiListString.Count > 0)
            {
                foreach (var doi in doiListString)
                {
                    dblpProceedings.DOIList.Add(doi);
                }
            }
            return dblpProceedings;
        }

        public static DBLPProceedings BuildFromXML(XElement x)
        {
            var dblpProceedings = new DBLPProceedings();



            var paperType = x.Name.ToString();
            if (paperType != "proceedings")
            {
                throw new Exception("Proceedings is not found");
            }
            dblpProceedings.key = x.Attribute("key")?.Value ?? "";
            if (dblpProceedings.key == "")
            {
                Console.WriteLine(x.ToString());
                throw new Exception("key is not found");
            }


            dblpProceedings.BookTitle = x.Element("booktitle")?.Value ?? "";

            var title = x.Element("title");
            if (title != null)
            {
                dblpProceedings.Title = title.Value;
            }
            var year = x.Element("year");
            if (year != null)
            {
                dblpProceedings.Year = int.Parse(year.Value);
            }

            var ee = x.Element("ee");
            if (ee != null)
            {
                var eeURL = ee.FirstNode?.ToString() ?? "";
                var doi = DBLPElement.getDOI(eeURL);
                if (doi != null)
                {
                    dblpProceedings.DOI = doi.ToLower();
                }
            }
            var volume = x.Element("volume");
            if (volume != null)
            {
                dblpProceedings.Volume = volume.Value;
            }





            return dblpProceedings;
        }

        public static string GetCommonPrefixDOI(string doi1, string doi2)
        {
            int minLength = Math.Min(doi1.Length, doi2.Length);
            int i = 0;
            while (i < minLength && doi1[i] == doi2[i])
            {
                i++;
            }
            return doi1.Substring(0, i);
        }

        public string ComputeFullName()
        {
            throw new Exception("Not implemented");
        }


        public int GetTitleType()
        {
            var tangos = this.Title.Split(", ");

            if (tangos.Length > 2)
            {
                var secondTango = tangos[1];
                if (Regex.IsMatch(secondTango, @"^[A-Z]+ \d{4}$"))
                {
                    return 4;
                }
            }

            if (Regex.IsMatch(this.Title, @"^Proceedings of the \d+(st|nd|rd|th)"))
            {
                return 1;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings of the \d{4} "))
            {
                return 2;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings \d{4} "))
            {
                return 2;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings of (the|The) (First|Second|Third|Fourth|Fifth|Sixth|Seventh|Eighth|Ninth|Tenth|Eleventh|Twelfth|Thirteenth|Fourteenth|Fifteenth|Sixteenth|Seventeenth|Eighteenth|Nineteenth|Twentieth) "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings of the Workshop on "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings of the ACM/IEEE "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"^Proceedings of the "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @", Proceedings of the "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @", Proceedings (from|From) "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"^International Symposium on "))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"International Workshop"))
            {
                return 3;
            }
            else if (Regex.IsMatch(this.Title, @"^\d+(st|nd|rd|th) "))
            {
                return 4;
            }
            else if (Regex.IsMatch(this.Title, @"^\d{4} "))
            {
                return 4;
            }
            else if (Regex.IsMatch(this.Title, @"^[A-Z]+ \d{4}(\s|,|:)"))
            {
                return 4;
            }
            else if (Regex.IsMatch(this.Title, @"^[A-Z]+ '\d{2}(\s|,|:)"))
            {
                return 6;
            }
            else if (Regex.IsMatch(this.Title, @"^[A-Z]+'\d{2}(\s|,|:)"))
            {
                return 6;
            }
            else if (Regex.IsMatch(this.Title, @"^[A-Z]+ \d{2}(\s|,|:)"))
            {
                return 7;
            }
            return 0;

        }


    }


    class DBLPProceedingsSeries
    {
        public string BookTitle { get; set; } = "";
        public Dictionary<string, DBLPProceedings> Series { get; set; } = new Dictionary<string, DBLPProceedings>();

        public bool ContainsKey(string key)
        {
            return this.Series.ContainsKey(key);
        }
        public DBLPProceedings GetProceedings(string key)
        {
            if (this.Series.ContainsKey(key))
            {
                return this.Series[key];
            }
            else
            {
                throw new Exception("Key is not found: " + key);
            }
        }

        /*
                public bool MatchDOIPrefix(string doi)
                {
                    foreach (var element in this.Series)
                    {
                        if (element.Value.CommonPrefixDOI.Length > 0)
                        {
                            if (element.Value.DOI.StartsWith(doi))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                */


        public List<string> ToJSONLines()
        {
            List<string> dataList = new List<string>();
            foreach (var element in this.Series)
            {
                dataList.Add(element.Value.ToJSONLine());
            }
            return dataList;
        }

        public void Add(DBLPProceedings proceedings)
        {
            if(this.Series.ContainsKey(proceedings.key)){
                throw new Exception("Key is already exists: " + proceedings.key);
            }
            this.Series.Add(proceedings.key, proceedings);
        }
        public string ComputeFullName()
        {
            throw new Exception("Not implemented");
        }

        public bool CanSummarize()
        {
            return false;
        }

    }

    class DBLPProceedingsSeriesDictionary
    {
        public Dictionary<string, DBLPProceedingsSeries> Series { get; set; } = new Dictionary<string, DBLPProceedingsSeries>();
        public Dictionary<string, string> KeyDictionary { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> DoiToBookTitleMapper { get; set; } = new Dictionary<string, string>();

        //public PrefixSet PrefixSet { get; set; } = new PrefixSet();


        public void Add(DBLPProceedings proceedings)
        {
            var bookTitle = proceedings.BookTitle;
            if (this.Series.ContainsKey(bookTitle))
            {
                this.Series[bookTitle].Add(proceedings);
            }
            else
            {
                this.Series[bookTitle] = new DBLPProceedingsSeries();
                this.Series[bookTitle].BookTitle = bookTitle;
                this.Series[bookTitle].Add(proceedings);
            }

            this.KeyDictionary.Add(proceedings.key, bookTitle);
        }
        public void BuildDoiToBookTitleMapper()
        {
            foreach (var element in this.Series)
            {
                if (element.Value.BookTitle.Length > 0)
                {
                    foreach (var proceedings in element.Value.Series)
                    {
                        foreach (var doi in proceedings.Value.DOIList)
                        {
                            if (doi.Length > 0)
                            {
                                if (!this.DoiToBookTitleMapper.ContainsKey(doi))
                                {
                                    this.DoiToBookTitleMapper.Add(doi, element.Value.BookTitle);

                                }
                                else
                                {
                                    if (this.DoiToBookTitleMapper[doi] != element.Value.BookTitle)
                                    {
                                        Console.WriteLine("DOI: " + doi);
                                        Console.WriteLine("BookTitle: " + element.Value.BookTitle);
                                        Console.WriteLine("--------------------------------");
                                        throw new Exception("DOI is not unique");
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        public string? SearchBookTitleByDOI(string doi)
        {
            if (this.DoiToBookTitleMapper.ContainsKey(doi))
            {
                return this.DoiToBookTitleMapper[doi];
            }
            else
            {
                return null;
            }
        }


        public bool ContainsBookTitle(string bookTitle)
        {
            return this.Series.ContainsKey(bookTitle);
        }
        public bool ContainsKey(string key)
        {
            return this.KeyDictionary.ContainsKey(key);
        }

        public DBLPProceedings GetProceedings(string key)
        {
            if (this.ContainsKey(key))
            {
                var bookTitle = this.KeyDictionary[key];
                return this.Series[bookTitle].GetProceedings(key);
            }
            else
            {
                throw new Exception("Key is not found: " + key);
            }
        }
        public void Save(string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                foreach (var element in this.Series.Values)
                {
                    var jsonLines = element.ToJSONLines();
                    foreach (var jsonLine in jsonLines)
                    {
                        writer.WriteLine(jsonLine);
                    }
                }
            }
        }

        public static DBLPProceedingsSeriesDictionary Load(string inputFilePath)
        {
            var proceedingsSeriesDictionary = new DBLPProceedingsSeriesDictionary();
            using (var reader = new StreamReader(inputFilePath, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }
                    var proceedings = DBLPProceedings.BuildFromJSONLine(line);
                    proceedingsSeriesDictionary.Add(proceedings);
                }
            }
            return proceedingsSeriesDictionary;
        }

    }

    class DBLPProceedingsSeriesSummary
    {
        public string BookTitle { get; set; } = "";
        public string FullName { get; set; } = "";
        public int Count { get; set; } = 0;

        public List<string> DOIPrefixList { get; set; } = new List<string>();

        public static DBLPProceedingsSeriesSummary Build(DBLPProceedingsSeries series)
        {
            var summary = new DBLPProceedingsSeriesSummary();
            summary.BookTitle = series.BookTitle;
            summary.FullName = series.ComputeFullName();
            summary.Count = series.Series.Count;
            return summary;
        }

        public string ToJSONLine()
        {
            return JsonSerializer.Serialize(this);
        }

        public static void Save(List<DBLPProceedingsSeriesSummary> summaryList, string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                foreach (var element in summaryList)
                {
                    writer.WriteLine(element.ToJSONLine());
                }
            }
        }
    }

}

