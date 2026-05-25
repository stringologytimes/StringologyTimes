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
    public class DataCiteParser
    {
        public static List<AuthorInfo> AuthorInfoParse(Dictionary<string, string> dictFromJSONL)
        {
            var authorInfoList = new List<AuthorInfo>();
            var attributeDict = JsonLib.CreateDictionaryFromJSONL(dictFromJSONL["attributes"]);


            if (attributeDict.ContainsKey("creators"))
            {
                var creatorsArray = JsonLib.CreateArrayFromJSONL(attributeDict["creators"]);
                foreach (var creator in creatorsArray)
                {
                    var authorInfo = new AuthorInfo();
                    var creatorDict = JsonLib.CreateDictionaryFromJSONL(creator);
                    if (creatorDict.ContainsKey("nameIdentifiers"))
                    {
                        var nameIdentifiersArray = JsonLib.CreateArrayFromJSONL(creatorDict["nameIdentifiers"]);
                        if (nameIdentifiersArray.Length > 0)
                        {
                            var str = nameIdentifiersArray[0];
                            var nameIdentifierDict = JsonLib.CreateDictionaryFromJSONL(str);
                            if (nameIdentifierDict.ContainsKey("ORCID") && nameIdentifierDict.ContainsKey("nameIdentifier"))
                            {
                                authorInfo.ORCID = nameIdentifierDict["nameIdentifier"];
                            }

                        }

                    }
                    if (creatorDict.ContainsKey("givenName"))
                    {
                        authorInfo.GivenName = creatorDict["givenName"];
                    }
                    if (creatorDict.ContainsKey("familyName"))
                    {
                        authorInfo.FamilyName = creatorDict["familyName"];
                    }
                    var name = creatorDict["name"];
                    var nameList = name.Split(',').ToList();
                    if (nameList.Count == 2)
                    {
                        var fullName = nameList[1].Trim() + " " + nameList[0].Trim();
                        authorInfo.FullName = fullName;

                    }
                    else if (nameList.Count == 1)
                    {
                        authorInfo.FullName = name;
                    }
                    else
                    {
                        throw new Exception("Creator name is not valid: " + name);
                    }
                    authorInfoList.Add(authorInfo);
                }
            }
            return authorInfoList;
        }
        

        public static List<string> GetTagsFromDataCiteJSONL(string jsonlString)
        {

            var dict = JsonLib.CreateDictionaryFromJSONL(jsonlString);
            List<string> tags = new List<string>();


            if (dict.ContainsKey("attributes"))
            {
                var attributesDict = JsonLib.CreateDictionaryFromJSONL(dict["attributes"]);

                if (attributesDict.ContainsKey("subjects"))
                {
                    var subjectsArray = JsonLib.CreateArrayFromJSONL(attributesDict["subjects"]);

                    foreach (var subject in subjectsArray)
                    {
                        Console.WriteLine(subject);

                        var subjectDict = JsonLib.CreateDictionaryFromJSONL(subject);
                        var subjectValue = subjectDict["subject"];
                        tags.Add(subjectValue);
                    }
                }

            }


            return tags;
        }
        public static string GetTypeFromDataCiteJSONL(Dictionary<string, string> dict, Dictionary<string, string>? typesDict)
        {
            var type = "";
            if (dict.ContainsKey("type"))
            {
                type = dict["type"];
            }

            if (typesDict != null && typesDict.ContainsKey("resourceTypeGeneral"))
            {
                var resourceTypeGeneral = typesDict["resourceTypeGeneral"];
                if (resourceTypeGeneral != null)
                {
                    type = resourceTypeGeneral;
                }
            }
            else
            {
                type = "Unknown";
            }

            return type;
        }
        public static string GetTitleFromDataCiteJSONL(Dictionary<string, string> attributeDict)
        {
            var title = "";
            if (attributeDict.ContainsKey("titles"))
            {
                var titleList = JsonLib.CreateArrayFromJSONL(attributeDict["titles"]);
                if (titleList.Length > 0)
                {
                    title = JsonLib.GetValueFromJSONL(titleList[0], "title")!;
                }
                else
                {
                    title = $"Dummy Title";
                }
            }
            else
            {
                title = $"Dummy Title";
            }
            return title ?? "Dummy Title";
        }

        public static DOIElement Parse(string jsonlString)
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
            element.Type = GetTypeFromDataCiteJSONL(dict, typesDict);
            element.Title = GetTitleFromDataCiteJSONL(attributeDict);


            
            if (attributeDict.ContainsKey("relatedItems")) {
                var relatedItemsArray = JsonLib.CreateArrayFromJSONL(attributeDict["relatedItems"]);
                foreach (var relatedItem in relatedItemsArray)
                {
                    var relatedItemDict = JsonLib.CreateDictionaryFromJSONL(relatedItem);
                    if (relatedItemDict.ContainsKey("relationType") && relatedItemDict.ContainsKey("relatedItemIdentifier")) {
                        var relationType = relatedItemDict["relationType"];
                        var relatedItemIdentifier = relatedItemDict["relatedItemIdentifier"];
                        var relatedItemIdentifierDict = JsonLib.CreateDictionaryFromJSONL(relatedItemIdentifier);
                        if (relationType == "IsPublishedIn" && relatedItemIdentifierDict.ContainsKey("relatedItemIdentifierType") && relatedItemIdentifierDict["relatedItemIdentifierType"] == "DOI")
                        {
                            var relatedItemIdentifier2 = relatedItemIdentifierDict["relatedItemIdentifier"];
                            element.ContainerDOI = relatedItemIdentifier2.ToLower();
                        }
                    }
                }
            }
            

            element.Authors = AuthorInfoParse(dict);



            var currentDatePriority = 0;
            int year = 0;

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
                var isYear = int.TryParse(dateStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out year);


                if (currentDatePriority < datePriority && !isYear)
                {
                    var dateValue = DateTime.Parse(dateStr);
                    element.Year = dateValue.Year.ToString();
                    element.Month = dateValue.Month.ToString();
                    currentDatePriority = datePriority;
                }
            }
            if (currentDatePriority == 0 && year > 0)
            {
                element.Year = year.ToString();
                element.Month = "-1";
                currentDatePriority = 6;
            }

            if (currentDatePriority == 0)
            {
                Console.WriteLine($"Warning ({element.DOI}): Year is not found");
                element.Year = "-1";
                element.Month = "-1";
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

            /*
            if (typesDict.ContainsKey("resourceTypeGeneral"))
            {
                var resourceTypeGeneral = typesDict["resourceTypeGeneral"];
                if (resourceTypeGeneral != null)
                {
                    element.Type = $"DataCite:{resourceTypeGeneral}";
                }
                else
                {
                    throw new Exception("Resource Type General is not found");
                }
            }
            else
            {
                Console.WriteLine($"Warning ({element.DOI}): Resource Type General is not found");
                element.Type = $"DataCite:Unknown";

            }
            */

            element.Source = "DataCite";

            return element;

        }

    }
}
