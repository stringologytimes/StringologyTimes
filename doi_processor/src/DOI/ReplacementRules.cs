using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;

namespace DataProcessor
{
    public class ReplacementRules
    {
        public static void Apply1(string rulePath, Dictionary<string, DOIElement> primaryDOIElementDict, Dictionary<string, DOIElement> secondaryDOIElementDict)
        {
            if (File.Exists(rulePath))
            {
                var typeReplacementRules = CSVFunctions.ReadCSV(rulePath);
                var typeReplacementRulesDict = new Dictionary<string, string>();
                typeReplacementRules.ForEach((v) =>
                {
                    var cols = v.Split(",");
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    typeReplacementRulesDict[key] = value;
                    Console.WriteLine($"Added type replacement rule: {key} -> {value}");
                });
                primaryDOIElementDict.Values.ToList().ForEach((v) =>
                {
                    if (typeReplacementRulesDict.ContainsKey(v.Type))
                    {
                        v.Type = typeReplacementRulesDict[v.Type];
                    }
                });
                secondaryDOIElementDict.Values.ToList().ForEach((v) =>
                {
                    if (typeReplacementRulesDict.ContainsKey(v.Type))
                    {
                        v.Type = typeReplacementRulesDict[v.Type];
                    }
                });
            }
            else
            {
                Console.WriteLine("No type replacement rules file found: " + rulePath);
            }

            var typeHashSet = new HashSet<string>();
            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                typeHashSet.Add(v.Type);
            });
            secondaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                typeHashSet.Add(v.Type);
            });
            var typeList = typeHashSet.ToList();
            typeList.Sort();
            typeList.ForEach((v) =>
            {
                Console.WriteLine(v);
            });
            
        }
    }
}