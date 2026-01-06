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
            Console.WriteLine("Saved: " + filePath);

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
    }
}