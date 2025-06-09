using _1.Schema;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.SqlBuilder;

public class Json
{
    public static List<Dictionary<(string key, string path), JToken>> matches = new();

    public static void JsonReader(JToken token, HashSet<string> keysToFind, string currentPath = "")
    {
        if (token is JObject obj)
        {
            var foundAtThisLevel = new Dictionary<(string key, string path), JToken>();

            foreach (var prop in obj.Properties())
            {
                var upperKey = prop.Name.ToUpperInvariant();

                var matchingKey = keysToFind.FirstOrDefault(x => x.Replace(" ", "") == upperKey);
                if (matchingKey != null)
                {
                    foundAtThisLevel[(matchingKey, currentPath)] = prop.Value;
                }
            }

            if (foundAtThisLevel.Count > 0)
            {
                matches.Add(foundAtThisLevel);
            }

            foreach (var prop in obj.Properties())
            {
                var nextPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}.{prop.Name}";
                JsonReader(prop.Value, keysToFind, nextPath);
            }
        }
        else if (token is JArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var nextPath = $"{currentPath}[{i}]";
                JsonReader(arr[i], keysToFind, nextPath);
            }
        }
    }

    public static List<string> GetAllEntitiesFromMatches()
    {
        var entities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dict in matches)
        {
            foreach (var token in dict.Values)
            {
                if (token.Type == JTokenType.String)
                {
                    var value = token.ToString();
                    if (value.Contains('.'))
                    {
                        var entity = value.Split('.')[0];
                        entities.Add(entity);
                    }
                }
                else if (token.Type == JTokenType.Array)
                {
                    foreach (var item in token)
                    {
                        if (item.Type == JTokenType.String)
                        {
                            var value = item.ToString();
                            if (value.Contains('.'))
                            {
                                var entity = value.Split('.')[0];
                                entities.Add(entity);
                            }
                        }
                    }
                }
            }
        }

        return entities.ToList();
    }

    public static string? FindExactSecondLevelKey(
    Dictionary<string, Dictionary<string, PropertySchema>> schemaStructure,
    List<string> targetEntities)
    {
        var normalizedTarget = targetEntities
            .Select(e => e.ToLowerInvariant())
            .OrderBy(e => e)
            .ToList();

        foreach (var outerPair in schemaStructure)
        {
            var innerKeys = outerPair.Value?.Keys;
            if (innerKeys == null) continue;

            foreach (var innerKey in innerKeys)
            {
                var keyParts = innerKey
                    .Split('_')
                    .Select(p => p.ToLowerInvariant())
                    .Where(p => p != "unboundclass")
                    .OrderBy(p => p)
                    .ToList();

                if (keyParts.SequenceEqual(normalizedTarget))
                {
                    return innerKey;
                }
            }
        }

        return null;
    }

    //public void SqlBuilder()
    //{
    //    var top= GetValue("TOP")==null?"":$"TOP({int.Parse(GetValue("TOP"))})";
    //    var select = $@"SELECT {GetValue("SELECT",',')}";
    //    var from= $@"FROM {GetValue("SELECT").First()}";
    //}

    //public static string? GetValue(string key, char? delimiter = null)
    //{
    //    return matches[key].Values.Aggregate().
    //}

}
