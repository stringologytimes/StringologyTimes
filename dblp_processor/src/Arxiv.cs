
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Json.Serialization;
using System.Text;
using System.IO;

namespace ArxivProcessor
{

    class VersionInfo
    {
        public string? version { get; set; }
        public string? created { get; set; }

    }
    class ArxivArticle
    {
        public string? id { get; set; }
        public string? submitter { get; set; }
        public string? authors { get; set; }
        public string? title { get; set; }
        public string? comments { get; set; }
        [JsonPropertyName("journal-ref")]
        public string? journalRef { get; set; }
        public string? doi { get; set; }
        [JsonPropertyName("report-no")]
        public string? reportNo { get; set; }
        public string? categories { get; set; }
        public string? license { get; set; }
        [JsonPropertyName("abstract")]
        public string? abstract_text { get; set; }
        //public string? versions{get;set;}
        public string? update_date { get; set; }
        public VersionInfo[]? versions { get; set; }

        public string toTSV()
        {
            var newTitle = this.title!.Replace("\r", "").Replace("\n", "");
            return $"{this.YearMonthDayString}\t{this.id}\t{newTitle}\t{this.categories}";
        }

        public long getUnixTime()
        {
            var exactDate = this.getCreatedTime();
            /*
            var strs = this.update_date!.Split("-");
            var year = int.Parse(strs[0]);
            var month = int.Parse(strs[1]);
            var day = int.Parse(strs[2]);
            */
            var date = new DateTime(exactDate.Year, exactDate.Month, exactDate.Day);

            return ((DateTimeOffset)date).ToUnixTimeSeconds();

        }
        public string YearMonthDayString
        {
            get
            {
                var date = this.getCreatedTime();
                return $"{date.Year}-{date.Month}-{date.Day}";
            }
        }
        public DateTime getCreatedTime()
        {
            foreach (var v in this.versions!)
            {
                if (v.version! == "v1")
                {
                    var date = DateTime.Parse(v.created!);
                    return date;
                }
            }
            throw new Exception("ParseError");
        }

        public static int Compare(ArxivArticle item1, ArxivArticle item2)
        {
            var time1 = item1.getUnixTime();
            var time2 = item2.getUnixTime();
            if (time1 != time2)
            {
                if (time1 < time2)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return item1.id!.CompareTo(item2.id!);
            }
        }



    }
    class Processor
    {
        public static void Process(string arxivJsonPath, string outputFilePath)
        {
            var options = new JsonSerializerOptions
            {
                // 日本語を変換するためのエンコード設定
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                // インデントを付ける
                WriteIndented = true
            };

            int counter = 0;

            var csDSArticles = new List<ArxivArticle>();

            foreach (string line in System.IO.File.ReadLines(arxivJsonPath))
            {
                //System.Console.WriteLine(line);
                //Console.WriteLine(person2.ToLightString());
                counter++;

                var article = JsonSerializer.Deserialize<ArxivArticle>(line, options);
                if (article != null && article.categories != null)
                {
                    if (article.categories.IndexOf("cs.DS") != -1)
                    {
                        csDSArticles.Add(article);
                    }
                }

                /*
                var px = line.IndexOf("cs.DS");
                if(px != -1){

                }
                */


                if (counter % 100000 == 0)
                {
                    Console.WriteLine($"{counter}/{csDSArticles.Count}");
                }

            }
            csDSArticles.Sort((a, b) => ArxivArticle.Compare(a, b));

            var lines = csDSArticles.Select((v) => v.toTSV());
            var outputStr = String.Join(System.Environment.NewLine, lines);
            File.WriteAllText(outputFilePath, outputStr, System.Text.Encoding.UTF8);
        }
    }
}