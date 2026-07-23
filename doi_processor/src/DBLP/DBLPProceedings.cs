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
        public string SeriesTitle { get; set; } = "";
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
            dataList.Add(JsonSerializer.Serialize(this.SeriesTitle));
            dataList.Add(JsonSerializer.Serialize(this.Title));
            dataList.Add(JsonSerializer.Serialize(this.Year?.ToString() ?? ""));
            dataList.Add(JsonSerializer.Serialize(this.Month?.ToString() ?? ""));
            dataList.Add(JsonSerializer.Serialize(this.Volume));

            dataList.Add(JsonSerializer.Serialize(this.DOIList));

            /*

        if (this.DOI.Length > 0)
        {
            dataList.Add(JsonSerializer.Serialize(new List<string>()));

        }
        else
        {

            dataList.Add(JsonSerializer.Serialize(this.DOIList));

        }
        */


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
            dblpProceedings.SeriesTitle = dataList[2].ToString() ?? "";
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


            dblpProceedings.SeriesTitle = x.Element("booktitle")?.Value ?? "";

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
}