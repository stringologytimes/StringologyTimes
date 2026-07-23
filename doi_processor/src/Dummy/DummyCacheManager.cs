using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
namespace DataProcessor
{
    class DummyCacheManager
    {
        public static string GetDummyCacheFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/dummy_cache";
        }
        public static string GetDummyCacheFilePath(string dataFolderPath)
        {
            var dummyDirectoryPath = GetDummyCacheFolderPath(dataFolderPath);
            if (!Directory.Exists(dummyDirectoryPath))
            {
                Directory.CreateDirectory(dummyDirectoryPath);
            }
            return dummyDirectoryPath + "/doi_element.jsonl";
        }
        public static string GetDOIAliasListFilePath(string dataFolderPath)
        {
            var dummyDirectoryPath = GetDummyCacheFolderPath(dataFolderPath);
            if (!Directory.Exists(dummyDirectoryPath))
            {
                Directory.CreateDirectory(dummyDirectoryPath);
            }

            return dummyDirectoryPath + "/doi_alias_list.tsv";
        }

        
    }
}
