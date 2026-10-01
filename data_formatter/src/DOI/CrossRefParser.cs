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
    public struct CrossRefDate
    {
        public string DateType { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public bool HasYear
        {
            get
            {
                return this.Year != 0;
            }
        }
        public bool HasMonth
        {
            get
            {
                return this.Month != 0;
            }
        }

        public CrossRefDate(int year, int month, string dateType)
        {
            this.Year = year;
            this.Month = month;
            this.DateType = dateType;
        }
        public CrossRefDate(int year, string dateType)
        {
            this.Year = year;
            this.Month = 0;
            this.DateType = dateType;
        }
        public CrossRefDate(string dateType)
        {
            this.Year = 0;
            this.Month = 0;
            this.DateType = dateType;
        }

        public static CrossRefDate ParseFromJSONL(string key, Dictionary<string, string> dict)
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
                        if (datePartsList[0].Count == 1)
                        {
                            return new CrossRefDate(datePartsList[0][0], key);
                        }
                        else
                        {
                            return new CrossRefDate(datePartsList[0][0], datePartsList[0][1], key);
                        }
                    }
                    else
                    {
                        return new CrossRefDate(key);
                    }
                }
                else
                {
                    return new CrossRefDate(key);
                }
            }
            else
            {
                return new CrossRefDate(key);
            }
        }



        public bool Procede(CrossRefDate other)
        {
            if (this.HasYear && other.HasYear)
            {
                if (this.Year < other.Year)
                {
                    return true;
                }
                else if (this.Year == other.Year)
                {
                    if (this.HasMonth && other.HasMonth)
                    {
                        return this.Month < other.Month;
                    }
                    else if (this.HasMonth && !other.HasMonth)
                    {
                        return true;
                    }
                    else if (!this.HasMonth && other.HasMonth)
                    {
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (this.HasYear && other.HasMonth)
            {
                return true;
            }
            else if (this.HasMonth && other.HasYear)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

    }


    public class CrossRefParser
    {
        public static KeyValuePair<int, int>? GetDataParts(Dictionary<string, string> dict, string key)
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
                        if (datePartsList[0].Count == 1)
                        {
                            return new KeyValuePair<int, int>(datePartsList[0][0], 0);
                        }
                        else
                        {
                            return new KeyValuePair<int, int>(datePartsList[0][0], datePartsList[0][1]);
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
            else
            {
                return null;
            }
        }
        public static CrossRefDate? GetYearMonthFromJSONL(Dictionary<string, string> dict)
        {
            var f1 = CrossRefDate.ParseFromJSONL("published", dict);
            var f2 = CrossRefDate.ParseFromJSONL("created", dict);
            var f3 = CrossRefDate.ParseFromJSONL("published-print", dict);
            var f4 = CrossRefDate.ParseFromJSONL("published-online", dict);

            var candidates = new List<CrossRefDate>();
            if(f1.HasYear)
            {
                candidates.Add(f1);
            }
            if(f2.HasYear)
            {
                candidates.Add(f2);
            }
            if(f3.HasYear)
            {
                candidates.Add(f3);
            }
            if(f4.HasYear)
            {
                candidates.Add(f4);
            }

            candidates.Sort((a, b) => a.Procede(b) ? -1 : 1);

            if(candidates.Count > 0)
            {
                return candidates[0];
            }
            else
            {
                return null;
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

            if (dict.ContainsKey("institution"))
            {

                var institutionArray = JsonLib.CreateArrayFromJSONL(dict["institution"]);
                if (institutionArray.Length > 0)
                {
                    var institutionDict = JsonLib.CreateDictionaryFromJSONL(institutionArray[0]);
                    if (institutionDict.ContainsKey("name"))
                    {
                        element.IdentifierTypeOrInstitution = institutionDict["name"];
                    }
                }
            }



            if (dict.ContainsKey("issue"))
            {
                element.Issue = dict["issue"];
            }
            else if (dict.ContainsKey("journal-issue"))
            {
                var journalIssueDict = JsonSerializer.Deserialize<Dictionary<string, object>>(dict["journal-issue"]);
                if (journalIssueDict != null && journalIssueDict.ContainsKey("issue"))
                {
                    var issue = journalIssueDict["issue"] as string;
                    if (issue != null && issue.Length > 0)
                    {
                        element.Issue = issue;
                    }
                    else
                    {
                        element.Issue = "";
                    }
                }

            }
            else
            {
                element.Issue = "";
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

            if (dict.ContainsKey("ISSN"))
            {
                var ISSNList = JsonSerializer.Deserialize<List<string>>(dict["ISSN"]);
                if (ISSNList != null)
                {
                    ISSNList.ForEach(issn =>
                    {
                        if (issn.Length > 0)
                        {
                            element.ISSNList.Add(ISBNConverter.ParseISSN(issn));
                        }
                    });
                }
            }

            element.Authors = AuthorInfo.ParseFromCrossRefJSONL(dict, element.Type);

            var containerTitleFlag = false;

            if (dict.ContainsKey("container-title"))
            {
                var containerTitleList = JsonSerializer.Deserialize<List<string>>(dict["container-title"]);
                if (containerTitleList != null && containerTitleList.Count > 0)
                {
                    element.ContainerTitle = string.Join("---", containerTitleList.ToArray());
                    //element.SeriesTitle = element.ContainerTitle;
                    containerTitleFlag = true;
                }
            }
            else if (dict.ContainsKey("title"))
            {
                var containerTitleList = JsonSerializer.Deserialize<List<string>>(dict["title"]);
                if (containerTitleList != null && containerTitleList.Count > 0)
                {
                    element.ContainerTitle = string.Join("---", containerTitleList.ToArray());
                    //element.SeriesTitle = element.ContainerTitle;
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
                                var doiString = doi.Value.GetString()!.ToLower();
                                if (DOIFunctions.IsValidDOI(doiString))
                                {
                                    element.DOIReferences.Add(doiString);
                                }
                                /*
                                else
                                {
                                    Console.WriteLine("Invalid DOI in reference: " + doiString);
                                }
                                */
                            }

                        }
                    }
                });
            }

            if (dict.ContainsKey("aliases"))
            {
                var aliasesList = JsonSerializer.Deserialize<List<string>>(dict["aliases"]);
                if (aliasesList != null && aliasesList.Count > 0)
                {
                    aliasesList.ForEach((v) =>
                    {
                        element.DOIAliasList.Add(v.ToLower());
                    });
                }
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

            var date = GetYearMonthFromJSONL(dict);
            if (date != null)
            {
                element.Year = date.Value.Year.ToString();
                element.Month = date.Value.Month.ToString();
            }
            else
            {
                dict.ToList().ForEach((v) => Console.WriteLine(v.Key + " : " + v.Value));
                throw new Exception("Year is not found");
            }

            /*

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
            */

            element.Source = "CrossRef";


            return element;
        }


        public static DOIElementX? LightweightParseFromJSONL(string jsonl)
        {
            var dict = JsonLib.CreateDictionaryFromJSONL(jsonl);

            if (dict.ContainsKey("DOI"))
            {
                var element = new DOIElementX();
                element.DOI = dict["DOI"];
                element.Type = "unknown";
                if (dict.ContainsKey("type"))
                {
                    element.Type = dict["type"];
                }
                element.Title = "";
                if (dict.ContainsKey("title"))
                {
                    var titleList = JsonSerializer.Deserialize<List<string>>(dict["title"]);
                    if (titleList != null && titleList.Count > 0)
                    {
                        element.Title = CSVFunctions.SanityzeForTSVFormat(titleList[0]);
                    }
                }

                if (dict.ContainsKey("ISBN"))
                {
                    var ISBNList = JsonSerializer.Deserialize<List<string>>(dict["ISBN"]);
                    if (ISBNList != null)
                    {
                        ISBNList.ForEach(isbn =>
                        {
                            if (isbn.Length > 0)
                            {
                                element.ISList.Add("ISBN:" + ISBNConverter.ParseISBN(isbn));
                            }
                        });
                    }
                }

                if (dict.ContainsKey("ISSN"))
                {
                    var ISSNList = JsonSerializer.Deserialize<List<string>>(dict["ISSN"]);
                    if (ISSNList != null)
                    {
                        ISSNList.ForEach(issn =>
                        {
                            if (issn.Length > 0)
                            {
                                element.ISList.Add("ISSN:" + ISBNConverter.ParseISSN(issn));
                            }
                        });
                    }
                }


                if (dict.ContainsKey("aliases"))
                {
                    var aliasesList = JsonSerializer.Deserialize<List<string>>(dict["aliases"]);
                    if (aliasesList != null && aliasesList.Count > 0)
                    {
                        aliasesList.ForEach((v) =>
                        {
                            element.ISList.Add("DOI_ALIAS:" + v.ToLower());
                        });
                    }
                }

                return element;
            }
            else
            {
                return null;
            }

        }
    }
}