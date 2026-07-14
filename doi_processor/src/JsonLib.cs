using System.Text;
using System.IO.Compression;
using System;
using System.IO;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;

namespace DataProcessor
{
    class JsonLib
    {
        /// <summary>
        /// 指定された .gz を省メモリで展開し、行を遅延列挙します。
        /// </summary>
        /// <param name="gzPath">入力 .gz ファイルパス</param>
        /// <param name="encoding">テキストのエンコーディング（null なら UTF-8 + BOM検出）</param>
        /// <param name="bufferSize">内部バッファサイズ（例: 64KB〜1MB）</param>
        public static IEnumerable<string> ReadLinesFromGzip(
            string gzPath,
            Encoding? encoding = null,
            int bufferSize = 1024 * 1024)
        {
            if (gzPath is null) throw new ArgumentNullException(nameof(gzPath));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // yield return を使うので iterator ブロック内で using var を使う
            using var file = new FileStream(
                gzPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                FileOptions.SequentialScan);

            using var gzip = new GZipStream(file, CompressionMode.Decompress, leaveOpen: false);

            // encoding が null の場合は UTF-8 を基本に BOM を検出
            using var reader = new StreamReader(
                gzip,
                encoding ?? Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: bufferSize,
                leaveOpen: false);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static string? GetValueFromJSONL(string jsonlString, string key)
        {
            var dict = new Dictionary<string, string>();
            try
            {
                var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonlString);
                if (keyValuePairs != null)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        dict[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }
                if (dict.ContainsKey(key))
                {
                    return dict[key];
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }

        }

        public static string[] CreateArrayFromJSONL(string jsonlString)
        {
            var list = new List<string>();
            try
            {
                var objects = System.Text.Json.JsonSerializer.Deserialize<object[]>(jsonlString);
                if (objects != null)
                {
                    foreach (var obj in objects)
                    {
                        list.Add(obj.ToString() ?? "");
                    }
                }
                return list.ToArray();

            }
            catch
            {
                return list.ToArray();
            }
        }


        public static Dictionary<string, string> CreateDictionaryFromJSONL(string jsonlString)
        {
            var dict = new Dictionary<string, string>();
            try
            {
                var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonlString);
                if (keyValuePairs != null)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        dict[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }
                return dict;

            }
            catch
            {
                return dict;
            }

        }


        /**
        * JSONLを表す文字列jsonStringをパースしてJSONL中のkey-valueペアを格納するDictionary<string, string>を返す
        */
        public static List<Dictionary<string, string>> ProcessJSONL(string jsonlString, bool appendInputLine = false)
        {
            var dicts = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(jsonlString))
            {
                return dicts;
            }

            // 各行毎にパース
            var lines = jsonlString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var dict = new Dictionary<string, string>();

                // 各行をJSONとしてパース
                try
                {
                    var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(line);

                    if (keyValuePairs != null)
                    {
                        foreach (var kvp in keyValuePairs)
                        {
                            dict[kvp.Key] = kvp.Value?.ToString() ?? "";
                        }
                    }
                    if (appendInputLine)
                    {
                        dict["input_line"] = line;
                    }

                    dicts.Add(dict.ToDictionary());

                }
                catch
                {
                    // パースに失敗した場合はスキップ
                    continue;
                }
            }

            return dicts;
        }

        public static Dictionary<string, string> LoadJSONLAsDictionary(string jsonlFilePath, string keyName)
        {
            var dict = new Dictionary<string, string>();
            var jsonlString = File.ReadAllText(jsonlFilePath);
            var lines = jsonlString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var line_dict = new Dictionary<string, string>();
                var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                if (keyValuePairs != null)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        if(kvp.Key == keyName && kvp.Value != null)
                        {
                            var doi = kvp.Value?.ToString() ?? "";

                            dict[doi] = line;
                        }
                    }
                }
            }
            return dict;
        }



        public static void Save(Dictionary<string, string> foundJSONLMap, string dicPath)
        {
            Console.WriteLine("Saving to " + dicPath);
            var foundExternalCrossRefMapWriter = new StreamWriter(dicPath, false, Encoding.UTF8);
            foundJSONLMap.ToList().ForEach((v) =>
            {
                foundExternalCrossRefMapWriter.WriteLine(v.Value);
            });
            foundExternalCrossRefMapWriter.Close();
        }


    }
}