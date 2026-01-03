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
    public class DOIElement
    {
        public string DOI { get; set; } = "";
        public string Title { get; set; } = "";
        public string Authors { get; set; } = "";
        public string ContainerTitle { get; set; } = "";
        public string Type { get; set; } = "";
        public string Volume { get; set; } = "";
        public string Year { get; set; } = "";
        public string Month { get; set; } = "";

        public List<string> DOIReferences { get; set; } = new List<string>();
        public List<string> UnknownReferences { get; set; } = new List<string>();


        public static List<int>? GetDataParts(Dictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                var publishedDict = JsonLib.CreateDictionaryFromJSONL(value);
                if (publishedDict.ContainsKey("date-parts"))
                {
                    var dateParts = publishedDict["date-parts"];
                    var datePartsList = JsonSerializer.Deserialize<List<List<int>>>(dateParts);
                    if (datePartsList != null && datePartsList.Count > 0)
                    {
                        return datePartsList[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static KeyValuePair<int, int> GetYearMonthFromJSONL(Dictionary<string, string> dict)
        {
            var f1 = GetDataParts(dict, "published");
            var f2 = GetDataParts(dict, "created");
            if (f1 != null && f1.Count > 1)
            {
                return new KeyValuePair<int, int>(f1[0], f1[1]);
            }
            else if (f2 != null && f2.Count > 1)
            {
                return new KeyValuePair<int, int>(f2[0], f2[1]);
            }
            else if (f1 != null && f1.Count > 0)
            {
                return new KeyValuePair<int, int>(f1[0], -1);
            }
            else if (f2 != null && f2.Count > 0)
            {
                return new KeyValuePair<int, int>(f2[0], -1);
            }
            else
            {
                return new KeyValuePair<int, int>(-1, -1);
            }
        }
        /*
        public static int? GetMonthFromJSONL(Dictionary<string, string> dict)
        {
            var f1 = GetDataParts(dict, "published");
            var f2 = GetDataParts(dict, "created");
            if (f1 != null && f1.Count > 1) {
                return f1[1];
            }
            else if (f2 != null && f2.Count > 1) {
                return f2[1];
            }
            else {
                return null;
            }
        }
        */


        public static DOIElement ParseFromCrossRefJSONL(string jsonlString)
        {
            var dict = JsonLib.CreateDictionaryFromJSONL(jsonlString);

            var element = new DOIElement();
            if (dict.ContainsKey("DOI"))
            {
                element.DOI = dict["DOI"];
            }
            if (dict.ContainsKey("type"))
            {
                //element.Type = dict["type"];
                element.Type = $"CrossRef:{dict["type"]}";

            }
            else
            {
                Console.WriteLine(jsonlString);
                throw new Exception("Type is not found");
            }
            if (dict.ContainsKey("title"))
            {
                var titleList = JsonSerializer.Deserialize<List<string>>(dict["title"]);
                if (titleList != null && titleList.Count > 0)
                {
                    element.Title = titleList[0];
                }
                else
                {
                    throw new Exception("Title is not found");
                }
            }
            else
            {
                element.Title = $"Dummy Title: {element.DOI}";
            }

            var containerTitleFlag = false;

            if (dict.ContainsKey("container-title"))
            {
                var containerTitleList = JsonSerializer.Deserialize<List<string>>(dict["container-title"]);
                if (containerTitleList != null && containerTitleList.Count > 0)
                {
                    element.ContainerTitle = containerTitleList[0];
                    containerTitleFlag = true;
                }
            }
            if (!containerTitleFlag)
            {
                if (element.Type == "CrossRef:monograph" || element.Type == "CrossRef:posted-content" || element.Type == "CrossRef:book")
                {
                    element.ContainerTitle = "";
                }
                else
                {
                    Console.WriteLine(jsonlString);
                    Console.WriteLine(element.Type);
                    throw new Exception("Container Title is not found");

                }

            }

            if (dict.ContainsKey("volume"))
            {
                element.Volume = dict["volume"];
            }
            else
            {
                element.Volume = "";
            }

            if (dict.ContainsKey("reference"))
            {
                var referenceList = JsonSerializer.Deserialize<List<object>>(dict["reference"]);
                referenceList?.ForEach((v) =>
                {
                    string vString = v.ToString()!;
                    var referenceListLine = JsonSerializer.Deserialize<Dictionary<string, object>>(vString);
                    if (referenceListLine != null && referenceListLine.Count > 0)
                    {
                        if (referenceListLine.ContainsKey("DOI"))
                        {
                            var doi = referenceListLine["DOI"] as System.Text.Json.JsonElement?;
                            if (doi != null && doi.Value.ValueKind == JsonValueKind.String)
                            {
                                element.DOIReferences.Add(doi.Value.GetString()!.ToLower());
                            }

                        }
                    }
                });
            }

            /*
            if (dict.ContainsKey("Authors"))
            {
                element.Authors = dict["Authors"];
            }
            else
            {
                dict.ToList().ForEach((v) => Console.WriteLine(v.Key + " : " + v.Value));
                throw new Exception("Authors is not found");
            }
            */

            var yearMonth = GetYearMonthFromJSONL(dict);
            if (yearMonth.Key != -1)
            {
                element.Year = yearMonth.Key.ToString();
            }
            else
            {
                dict.ToList().ForEach((v) => Console.WriteLine(v.Key + " : " + v.Value));
                throw new Exception("Year is not found");
            }
            if (yearMonth.Value != -1)
            {
                element.Month = yearMonth.Value.ToString();
            }

            return element;
        }
        public static DOIElement ParseFromDataCiteJSONL(string jsonlString)
        {
            var dict = JsonLib.CreateDictionaryFromJSONL(jsonlString);
            var attributeDict = JsonLib.CreateDictionaryFromJSONL(dict["attributes"]);
            var typesDict = JsonLib.CreateDictionaryFromJSONL(attributeDict["types"]);
            var dateArray = JsonLib.CreateArrayFromJSONL(attributeDict["dates"]);

            var element = new DOIElement();
            if (dict.ContainsKey("id"))
            {
                element.DOI = dict["id"];
            }
            if (dict.ContainsKey("type"))
            {
                element.Type = dict["type"];
            }
            else
            {
                Console.WriteLine(jsonlString);
                throw new Exception("Type is not found");
            }
            //attributeDict.ToList().ForEach((v) => Console.WriteLine(v.Key));
            if (attributeDict.ContainsKey("titles"))
            {
                var titleList = JsonLib.CreateArrayFromJSONL(attributeDict["titles"]);
                if (titleList.Length > 0)
                {
                    var title = JsonLib.GetValueFromJSONL(titleList[0], "title");
                    element.Title = title!;
                }
                else
                {
                    throw new Exception("Title is not found");
                }
            }
            else
            {
                element.Title = $"Dummy Title: {element.DOI}";
            }

            var currentDatePriority = 0;

            foreach (var date in dateArray)
            {
                var dateDict = JsonLib.CreateDictionaryFromJSONL(date);
                var dataType = dateDict["dateType"];
                var datePriority = 0;
                if (dataType == "Issued")
                {
                    datePriority = 5;
                }
                else if (dataType == "Accepted")
                {
                    datePriority = 4;
                }
                else if (dataType == "Available")
                {
                    datePriority = 3;
                }
                else if (dataType == "Created")
                {
                    datePriority = 2;
                }
                else if (dataType == "Submitted")
                {
                    datePriority = 1;
                }
                string dateStr = dateDict["date"]!;
                int year = 0;
                var isYear = int.TryParse(dateStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out year);


                if (currentDatePriority < datePriority && !isYear)
                {
                    var dateValue = DateTime.Parse(dateStr);
                    element.Year = dateValue.Year.ToString();
                    element.Month = dateValue.Month.ToString();
                    currentDatePriority = datePriority;
                }
            }

            if (currentDatePriority == 0)
            {
                dateArray.ToList().ForEach((v) => Console.WriteLine(v));
                throw new Exception("Year is not found");
            }
            

            if (dict.ContainsKey("relationships"))
            {
                var relationshipsDict = JsonLib.CreateDictionaryFromJSONL(dict["relationships"]);
                if (relationshipsDict.ContainsKey("references"))
                {
                    var referencesDict = JsonLib.CreateDictionaryFromJSONL(relationshipsDict["references"]);

                    if (referencesDict.ContainsKey("data"))
                    {
                        var dataArray = JsonLib.CreateArrayFromJSONL(referencesDict["data"]);
                        foreach (var data in dataArray)
                        {
                            var dataDict = JsonLib.CreateDictionaryFromJSONL(data);
                            var type = dataDict["type"];
                            var id = dataDict["id"];
                            if (type == "dois")
                            {
                                element.DOIReferences.Add(id.ToLower());
                            }
                            else
                            {
                                element.UnknownReferences.Add(id);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine(jsonlString);
                    throw new Exception("References is not found");
                }
            }
            else
            {
                Console.WriteLine(jsonlString);
                throw new Exception("Relationships is not found");
            }

            



            if (attributeDict.ContainsKey("publicationYear"))
            {

            }

            var resourceTypeGeneral = typesDict["resourceTypeGeneral"];
            if (resourceTypeGeneral != null)
            {
                element.Type = $"DataCite:{resourceTypeGeneral}";
            }
            else
            {
                throw new Exception("Resource Type General is not found");
            }


            return element;

        }

        public string to_JSON_Line()
        {
            List<string> dataList = new List<string>();
            dataList.Add(JsonSerializer.Serialize(this.DOI));
            dataList.Add(JsonSerializer.Serialize(this.Type));
            dataList.Add(JsonSerializer.Serialize(this.Title));
            dataList.Add(JsonSerializer.Serialize(this.Year));
            dataList.Add(JsonSerializer.Serialize(this.Month));
            dataList.Add(JsonSerializer.Serialize(this.ContainerTitle));
            dataList.Add(JsonSerializer.Serialize(this.Volume));


            dataList.Add(JsonSerializer.Serialize(this.DOIReferences.ToArray()));
            dataList.Add(JsonSerializer.Serialize(this.UnknownReferences.ToArray()));

            string dataString = "[" + string.Join(",", dataList) + "]";
            return dataString;
        }

    }
}