using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO.Compression;
namespace DataProcessor
{
    class CSVFunctions
    {
        public static string SanityzeForTSVFormat(string line)
        {
            return line.Replace("\r", "").Replace("\n", "").Replace("\t", " ");
        }
        public static void WriteCSV(string filePath, List<string> lines)
        {
            Console.WriteLine("Writing: " + filePath);
            var copyLines = lines.ToList();
            copyLines.Sort((a, b) => a.CompareTo(b));
            File.WriteAllLines(filePath, lines);
            Console.WriteLine("Saved: " + filePath);
        }

        public static void WriteCSV(string filePath, HashSet<string> set)
        {
            WriteCSV(filePath, set.ToList());
        }
        public static void WriteCSV(string filePath, List<List<string>> table)
        {
            var list = new List<string>();
            var delimiter = filePath.EndsWith(".csv") ? "," : "\t";
            table.ForEach((row) =>
            {
                list.Add(String.Join(delimiter, row));
            });

            WriteCSV(filePath, list);
        }

        public static void WriteCSVAsDictionary(string filePath, Dictionary<string, string> dict)
        {
            var list = new List<string>();
            var delimiter = filePath.EndsWith(".csv") ? "," : "\t";
            dict.ToList().ForEach((v) =>
            {
                list.Add(v.Key + delimiter + v.Value);
            });

            WriteCSV(filePath, list);
        }

        public static void WriteCSVByGZip(string filePath, List<string> lines)
        {

            var linesString = String.Join("\n", lines);
            byte[] input1 = Encoding.UTF8.GetBytes(linesString);
            if (linesString.Length > 0)
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var gzip = new GZipStream(fs, CompressionLevel.Optimal))
                    {
                        gzip.Write(input1, 0, input1.Length);
                    }
                }

            }
            //Console.WriteLine("Saved: " + filePath);
            Console.WriteLine("Saved: " + filePath + " / " + lines.Count);

        }

        public static List<string> ReadCSV(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                return new List<string>();
            }
            else
            {
                return File.ReadAllLines(filePath).ToList();
            }
        }
        public static HashSet<string> ReadCSVAsHashSet(string filePath)
        {
            var list = ReadCSV(filePath);
            var set = new HashSet<string>();
            list.ForEach((v) =>
            {
                set.Add(v);
            });
            return set;
        }
        public static Dictionary<string, string> ReadCSVAasDictionary(string filePath)
        {
            var list = ReadCSV(filePath);
            var fileInfo = new FileInfo(filePath);
            var delimiter = fileInfo.Extension == ".csv" ? "," : "\t";

            var dict = new Dictionary<string, string>();
            list.ForEach((v) =>
            {
                var cols = v.Split(delimiter);
                if (cols.Length > 1)
                {
                    dict[cols[0]] = cols[1];
                }
            });
            return dict;
        }
        public static Dictionary<string, List<string>> ReadCSVAasMultiDictionary(string filePath)
        {
            var list = ReadCSV(filePath);
            var fileInfo = new FileInfo(filePath);
            var delimiter = fileInfo.Extension == ".csv" ? "," : "\t";

            var dict = new Dictionary<string, List<string>>();
            list.ForEach((v) =>
            {
                var cols = v.Split(delimiter);
                if (cols.Length > 1)
                {
                    var key = cols[0];
                    var value = cols[1];
                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = new List<string>();
                    }
                    dict[key].Add(value);
                }
            });
            return dict;
        }

        public static List<KeyValuePair<string, string>> ReadCSVAsKeyValuePairList(string filePath)
        {
            var list = ReadCSV(filePath);
            var fileInfo = new FileInfo(filePath);
            var delimiter = fileInfo.Extension == ".csv" ? "," : "\t";

            var keyValuePairList = new List<KeyValuePair<string, string>>();
            list.ForEach((v) =>
            {
                var cols = v.Split(delimiter);
                if (cols.Length > 1)
                {
                    keyValuePairList.Add(new KeyValuePair<string, string>(cols[0], cols[1]));
                }
            });
            return keyValuePairList;
        }
        public static List<string> ReadCSVAsList(string filePath)
        {
            var list = ReadCSV(filePath);
            var fileInfo = new FileInfo(filePath);
            var delimiter = fileInfo.Extension == ".csv" ? "," : "\t";

            var rowList = new List<string>();

            list.ForEach((v) =>
            {
                var cols = v.Split(delimiter);
                var row = new List<string>();
                foreach (var col in cols)
                {
                    row.Add(col);
                }
                rowList.Add(String.Join(delimiter, row));
            });
            return rowList;
        }
        
    }
}