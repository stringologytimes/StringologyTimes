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
    
    class DBLPProceedingsCollectionSummary
    {
        public string BookTitle { get; set; } = "";
        public string FullName { get; set; } = "";
        public int Count { get; set; } = 0;

        public List<string> DOIPrefixList { get; set; } = new List<string>();

        public static DBLPProceedingsCollectionSummary Build(DBLPProceedingsCollection collection)
        {
            var summary = new DBLPProceedingsCollectionSummary();
            summary.BookTitle = collection.SeriesTitle;
            summary.FullName = collection.ComputeFullName();
            summary.Count = collection.Series.Count;
            return summary;
        }

        public string ToJSONLine()
        {
            return JsonSerializer.Serialize(this);
        }

        public static void Save(List<DBLPProceedingsCollectionSummary> summaryList, string outputFilePath)
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