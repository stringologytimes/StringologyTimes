using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace DataProcessor
{
    public class AuthorInfo
    {
        public string FullName { get; set; } = "";
        public string GivenName { get; set; } = "";
        public string FamilyName { get; set; } = "";
        public string ORCID { get; set; } = "";

        public string to_JSON_Line()
        {
            List<string> dataList = new List<string>();
            dataList.Add(JsonSerializer.Serialize(this.FullName));
            dataList.Add(JsonSerializer.Serialize(this.GivenName));
            dataList.Add(JsonSerializer.Serialize(this.FamilyName));
            dataList.Add(JsonSerializer.Serialize(this.ORCID));
            string dataString = "[" + string.Join(",", dataList) + "]";
            return dataString;
        }

        public static List<AuthorInfo> ParseFromCrossRefJSONL(Dictionary<string, string> dictFromJSONL, string doiType)
        {

            var authorInfoList = new List<AuthorInfo>();

            if (doiType == "CrossRef:book")
            {
                if (!dictFromJSONL.ContainsKey("editor"))
                {
                    //logFile.WriteLine("Warning: Editor is not found: " + dictFromJSONL["DOI"]);
                    //dictFromJSONL.ToList().ForEach((v) => Console.WriteLine(v.Key + " : " + v.Value));
                    //throw new Exception("Author is not found");
                }
                else
                {
                    var editorArray = JsonLib.CreateArrayFromJSONL(dictFromJSONL["editor"]);
                    foreach (var editorJSON in editorArray)
                    {
                        var authorInfo = new AuthorInfo();
                        var editorDict = JsonLib.CreateDictionaryFromJSONL(editorJSON);
                        if (editorDict.ContainsKey("ORCID"))
                        {
                            authorInfo.ORCID = editorDict["ORCID"];

                        }
                        if (editorDict.ContainsKey("given"))
                        {
                            authorInfo.GivenName = editorDict["given"];
                        }
                        if (editorDict.ContainsKey("family"))
                        {
                            authorInfo.FamilyName = editorDict["family"];
                        }
                        if (editorDict.ContainsKey("name"))
                        {
                            authorInfo.FullName = editorDict["name"];
                        }
                        authorInfoList.Add(authorInfo);
                    }
                }


            }
            else
            {
                if (!dictFromJSONL.ContainsKey("author"))
                {
                    //logFile.WriteLine("Warning: Editor is not found: " + dictFromJSONL["DOI"]);
                }
                else
                {
                    var authorArray = JsonLib.CreateArrayFromJSONL(dictFromJSONL["author"]);
                foreach (var authorJSON in authorArray)
                {
                    var authorInfo = new AuthorInfo();
                    var authorDict = JsonLib.CreateDictionaryFromJSONL(authorJSON);
                    if (authorDict.ContainsKey("ORCID"))
                    {
                        authorInfo.ORCID = authorDict["ORCID"];

                    }
                    if (authorDict.ContainsKey("given"))
                    {
                        authorInfo.GivenName = authorDict["given"];
                    }
                    if (authorDict.ContainsKey("family"))
                    {
                        authorInfo.FamilyName = authorDict["family"];
                    }
                    if (authorDict.ContainsKey("name"))
                    {
                        authorInfo.FullName = authorDict["name"];
                    }
                    authorInfoList.Add(authorInfo);
                }
                    
                }
                

            }
            //logFile.Close();





            return authorInfoList;
        }
        public string TryGetFullName()
        {
            if (this.FullName != "")
            {
                return this.FullName;
            }
            else
            {
                return this.GivenName + " " + this.FamilyName;
            }
        }
        public static List<AuthorInfo> ParseFromDataCiteJSONL(Dictionary<string, string> dictFromJSONL)
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

    }
}