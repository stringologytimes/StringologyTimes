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
    public class CrossRefParser
    {
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

        public static bool IsGroupType(string type)
        {
            if (type == "book" || type == "edited-book" || type == "journal" || type == "proceedings" || type == "journal-volume" || type == "book-series" || type == "proceedings-series")
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public static DOIElement Parse(string jsonlString)
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
                element.Type = $"{dict["type"]}";

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

            if (dict.ContainsKey("ISBN"))
            {
                var isbnList = JsonSerializer.Deserialize<List<string>>(dict["ISBN"]);


                if (isbnList != null && isbnList.Count > 0)
                {
                    for (int i = 0; i < isbnList.Count; i++)
                    {
                        var isbn = isbnList[i];
                        var isValid = ISBNConverter.IsValidIsbn10(isbn);
                        if (isValid)
                        {
                            var isbn13 = ISBNConverter.Isbn10ToIsbn13(isbn);
                            isbnList[i] = isbn13;
                        }
                    }


                    element.ISBNList = isbnList;
                }
            }

            /*
                        if (dict.ContainsKey("ISSN"))
                        {
                            var issnList = JsonSerializer.Deserialize<List<string>>(dict["ISSN"]);


                            if (issnList != null && issnList.Count > 0)
                            {
                                for (int i = 0; i < issnList.Count; i++)
                                {
                                    var issn = issnList[i];
                                        isbnList[i] = isbn13;
                                }


                                element.ISBNList = isbnList;
                            }
                        }
                        */




            element.Authors = AuthorInfo.ParseFromCrossRefJSONL(dict, element.Type);

            var containerTitleFlag = false;

            if (dict.ContainsKey("container-title"))
            {
                var containerTitleList = JsonSerializer.Deserialize<List<string>>(dict["container-title"]);
                if (containerTitleList != null && containerTitleList.Count > 0)
                {
                    element.ContainerTitle = string.Join("---", containerTitleList.ToArray());
                    element.SeriesTitle = element.ContainerTitle;
                    containerTitleFlag = true;
                }
            }
            else if (dict.ContainsKey("title"))
            {
                var containerTitleList = JsonSerializer.Deserialize<List<string>>(dict["title"]);
                if (containerTitleList != null && containerTitleList.Count > 0)
                {
                    element.ContainerTitle = string.Join("---", containerTitleList.ToArray());
                    element.SeriesTitle = element.ContainerTitle;
                    containerTitleFlag = true;
                }
            }



            if (!containerTitleFlag)
            {
                if (element.Type == "monograph" || element.Type == "posted-content" || element.Type == "book")
                {
                    element.ContainerTitle = "UNKNOWN";
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

            element.Source = "CrossRef";

            return element;
        }
    }
}