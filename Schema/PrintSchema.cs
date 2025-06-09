using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Schema;

public class PrintSchema
{
    //public static void PrintSchemaAsCode(Dictionary<string, Dictionary<string, PropertySchema>> schema)
    //{

    //    foreach (var entity in schema)
    //    {
    //        Console.WriteLine($"[\"{entity.Key}\"] = ");
    //        Console.WriteLine("{");

    //        var props = entity.Value.ToList();
    //        for (int i = 0; i < props.Count; i++)
    //        {
    //            var prop = props[i];
    //            var typeName = GetFriendlyTypeName(prop.Value.Type);
    //            var nullable = prop.Value.Nullable ? "true" : "false";

    //            string line;
    //            if (prop.Value.Relation == null)
    //            {
    //                line = $"   [\"{prop.Key}\"] = {typeName}, {nullable}";
    //            }
    //            else
    //            {
    //                var rel = prop.Value.Relation;
    //                line = $"   [\"{prop.Key}\"] = {typeName}, {nullable}, new Relation({rel.Depth}, \"{rel.ReferenceProperty}\")";
    //            }

    //            // Add comma except for last property
    //            if (i < props.Count - 1)
    //                line += ",";

    //            Console.WriteLine(line);
    //        }

    //        Console.WriteLine("},");
    //        Console.WriteLine();
    //    }

    //}

    //private static string GetFriendlyTypeName(Type type)
    //{
    //    if (type == null)
    //        return "unknown";

    //    if (Nullable.GetUnderlyingType(type) is Type underlying)
    //        return GetFriendlyTypeName(underlying) + "?";

    //    if (type.IsGenericType)
    //    {
    //        var genericDef = type.GetGenericTypeDefinition();

    //        // Handle collections
    //        if (typeof(IEnumerable<>).IsAssignableFrom(genericDef) ||
    //            typeof(ICollection<>).IsAssignableFrom(genericDef) ||
    //            typeof(List<>).IsAssignableFrom(genericDef) ||
    //            type.Name.StartsWith("ICollection"))
    //        {
    //            var innerType = GetFriendlyTypeName(type.GetGenericArguments()[0]);
    //            return $"{innerType}[]";
    //        }

    //        // Fallback for other generic types
    //        var typeName = genericDef.Name.Split('`')[0];
    //        var args = type.GetGenericArguments()
    //            .Select(GetFriendlyTypeName);
    //        return $"{typeName}<{string.Join(", ", args)}>";
    //    }

    //    return type switch
    //    {
    //        { } t when t == typeof(int) => "int",
    //        { } t when t == typeof(string) => "string",
    //        { } t when t == typeof(bool) => "bool",
    //        { } t when t == typeof(double) => "double",
    //        { } t when t == typeof(float) => "float",
    //        { } t when t == typeof(decimal) => "decimal",
    //        { } t when t == typeof(long) => "long",
    //        { } t when t == typeof(short) => "short",
    //        { } t when t == typeof(byte) => "byte",
    //        { } t when t == typeof(char) => "char",
    //        _ => type.Name
    //    };
    //}
}
