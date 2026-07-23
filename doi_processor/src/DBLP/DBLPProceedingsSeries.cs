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


    class DBLPProceedingsSeries
    {
        public string SeriesTitle { get; set; } = "";
        public Dictionary<string, DBLPProceedings> Series { get; set; } = new Dictionary<string, DBLPProceedings>();

        public Dictionary<string, string> ProceedingsDOIToKeyMapper { get; set; } = new Dictionary<string, string>();

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
            if (this.Series.ContainsKey(proceedings.key))
            {
                throw new Exception("Key is already exists: " + proceedings.key);
            }
            this.Series.Add(proceedings.key, proceedings);
            if(proceedings.DOI.Length > 0)
            {
                this.ProceedingsDOIToKeyMapper.Add(proceedings.DOI, proceedings.key);
            }
        }
        public string ComputeFullName()
        {
            throw new Exception("Not implemented");
        }

        public bool CanSummarize()
        {
            return false;
        }

        public KeyValuePair<int, int> GetMinimumYearAndMonth()
        {
            int minimum_year = 9999;
            int minimum_month = 12;
            foreach (var proceedings in this.Series.Values)
            {
                if (proceedings.Year < minimum_year)
                {
                    minimum_year = proceedings.Year ?? 0;
                    minimum_month = proceedings.Month ?? 0;
                }
            }
            return new KeyValuePair<int, int>(minimum_year, minimum_month);
        }

    }



}

