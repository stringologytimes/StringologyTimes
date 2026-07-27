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
                        if(datePartsList[0].Count == 1)
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
        public static KeyValuePair<int, int> GetYearMonthFromJSONL(Dictionary<string, string> dict)
        {
            var f1 = GetDataParts(dict, "published");
            var f2 = GetDataParts(dict, "created");
            var f3 = GetDataParts(dict, "published-print");

            var candidate = new KeyValuePair<int, int>(9999, 9999);
            if (f1 != null && f1.Value.Key < candidate.Key)
            {
                candidate = f1.Value;
            }

            if (f2 != null && f2.Value.Key < candidate.Key)
            {
                candidate = f2.Value;
            }

            if (f3 != null && f3.Value.Key < candidate.Key)
            {
                candidate = f3.Value;
            }

            if(candidate.Key == 9999)
            {
                return new KeyValuePair<int, int>(0, 0);
            }
            else
            {
                return candidate;
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
                if(institutionArray.Length > 0){
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

            if(element.DOI == "10.1007/11605126")
            {

                CommonFunctions.OutputSystemMessageFunction("Yeartttt: " + element.Year + "/" + element.Month, ConsoleColor.Red);
                
            }

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