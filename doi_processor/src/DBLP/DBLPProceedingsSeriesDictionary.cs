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

    class DBLPProceedingsSeriesDictionary
    {
        public Dictionary<string, DBLPProceedingsSeries> Series { get; set; } = new Dictionary<string, DBLPProceedingsSeries>();
        public Dictionary<string, string> KeyDictionary { get; set; } = new Dictionary<string, string>();
        //public Dictionary<string, string> DoiToSeriesTitleMapper { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, KeyValuePair<string, string>> DoiToSeriesTitleAndKeyMapper { get; set; } = new Dictionary<string, KeyValuePair<string, string>>();

        public Dictionary<string, string> ProceedingsDOIToKeyMapper { get; set; } = new Dictionary<string, string>();



        //public PrefixSet PrefixSet { get; set; } = new PrefixSet();


        public void Add(DBLPProceedings proceedings)
        {
            var bookTitle = proceedings.SeriesTitle;
            if (this.Series.ContainsKey(bookTitle))
            {
                this.Series[bookTitle].Add(proceedings);
            }
            else
            {
                this.Series[bookTitle] = new DBLPProceedingsSeries();
                this.Series[bookTitle].SeriesTitle = bookTitle;
                this.Series[bookTitle].Add(proceedings);
            }

            this.KeyDictionary.Add(proceedings.key, bookTitle);


            if (proceedings.DOI.Length > 0 && !this.ProceedingsDOIToKeyMapper.ContainsKey(proceedings.DOI))
            {
                this.ProceedingsDOIToKeyMapper.Add(proceedings.DOI, proceedings.key);
            }
        }
        public void BuildDoiToSeriesTitleAndKeyMapper()
        {
            foreach (var element in this.Series)
            {
                if (element.Value.SeriesTitle.Length > 0)
                {
                    foreach (var proceedings in element.Value.Series)
                    {
                        if (proceedings.Value.DOI.Length > 0)
                        {
                            if (!this.DoiToSeriesTitleAndKeyMapper.ContainsKey(proceedings.Value.DOI))
                            {
                                this.DoiToSeriesTitleAndKeyMapper.Add(proceedings.Value.DOI, new KeyValuePair<string, string>(element.Value.SeriesTitle, proceedings.Value.key));
                            }
                            else
                            {
                                var message = "Warning: Conflict DOI: " + proceedings.Value.DOI + " -> " + element.Value.SeriesTitle + " vs " + this.DoiToSeriesTitleAndKeyMapper[proceedings.Value.DOI].Key;
                                CommonFunctions.OutputSystemMessageFunction(message, ConsoleColor.Yellow);
                            }
                        }
                        foreach (var doi in proceedings.Value.DOIList)
                        {
                            if (doi.Length > 0)
                            {
                                if (!this.DoiToSeriesTitleAndKeyMapper.ContainsKey(doi))
                                {
                                    this.DoiToSeriesTitleAndKeyMapper.Add(doi, new KeyValuePair<string, string>(element.Value.SeriesTitle, proceedings.Value.key));

                                }
                                else
                                {
                                    if (this.DoiToSeriesTitleAndKeyMapper[doi].Key != element.Value.SeriesTitle)
                                    {
                                        Console.WriteLine("DOI: " + doi);
                                        Console.WriteLine("BookTitle: " + element.Value.SeriesTitle);
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
        /*

        public void BuildDoiToSeriesTitleMapper()
        {
            foreach (var element in this.Series)
            {
                if (element.Value.SeriesTitle.Length > 0)
                {
                    foreach (var proceedings in element.Value.Series)
                    {
                        if (proceedings.Value.DOI.Length > 0)
                        {
                            if (!this.DoiToSeriesTitleMapper.ContainsKey(proceedings.Value.DOI))
                            {
                                this.DoiToSeriesTitleMapper.Add(proceedings.Value.DOI, element.Value.SeriesTitle);
                            }
                            else
                            {
                                Console.WriteLine("Conflict DOI: " + proceedings.Value.DOI + " -> " + element.Value.SeriesTitle + " vs " + this.DoiToSeriesTitleMapper[proceedings.Value.DOI]);
                            }
                        }
                        foreach (var doi in proceedings.Value.DOIList)
                        {
                            if (doi.Length > 0)
                            {
                                if (!this.DoiToSeriesTitleMapper.ContainsKey(doi))
                                {
                                    this.DoiToSeriesTitleMapper.Add(doi, element.Value.SeriesTitle);

                                }
                                else
                                {
                                    if (this.DoiToSeriesTitleMapper[doi] != element.Value.SeriesTitle)
                                    {
                                        Console.WriteLine("DOI: " + doi);
                                        Console.WriteLine("BookTitle: " + element.Value.SeriesTitle);
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
        */
        public KeyValuePair<string, string>? SearchSeriesTitleAndKeyByDOI(string doi)
        {
            if (this.DoiToSeriesTitleAndKeyMapper.ContainsKey(doi))
            {
                return this.DoiToSeriesTitleAndKeyMapper[doi];
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
}

