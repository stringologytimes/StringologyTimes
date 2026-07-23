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
    
    class DBLPProceedingsSeriesSummary
    {
        public string BookTitle { get; set; } = "";
        public string FullName { get; set; } = "";
        public int Count { get; set; } = 0;

        public List<string> DOIPrefixList { get; set; } = new List<string>();

        public static DBLPProceedingsSeriesSummary Build(DBLPProceedingsSeries series)
        {
            var summary = new DBLPProceedingsSeriesSummary();
            summary.BookTitle = series.SeriesTitle;
            summary.FullName = series.ComputeFullName();
            summary.Count = series.Series.Count;
            return summary;
        }

        public string ToJSONLine()
        {
            return JsonSerializer.Serialize(this);
        }

        public static void Save(List<DBLPProceedingsSeriesSummary> summaryList, string outputFilePath)
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