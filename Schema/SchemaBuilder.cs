using System.Reflection;
using System.Text;

namespace _1.Schema;

public class SchemaBuilder
{
    public Dictionary<string, Dictionary<string, PropertySchema>> SchemaStructure = new();

    public SchemaBuilder(Dictionary<string, Dictionary<(string foreignKey, string foreignKeyTableName), (string property, string table)>?> fks)
        => SchemaStructure = CreateSchema(fks);

    private static Dictionary<string, Dictionary<string, PropertySchema>> CreateSchema(
        Dictionary<string, Dictionary<(string foreignKey, string foreignKeyTableName), (string property, string table)>?> fks)
    {
        var schema = new Dictionary<string, Dictionary<string, PropertySchema>>();
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var entity in fks.Keys)
        {
            schema[entity] = new();

            var entityType = assembly.GetTypes().FirstOrDefault(t => t.Name == entity);
            if (entityType == null) continue;

            var allProperties = entityType.GetProperties();
            var fkPropertyNames = new HashSet<string>();

            // Collect all FK property names for this entity
            if (fks[entity] != null)
            {
                foreach (var pair in fks[entity])
                    fkPropertyNames.Add(pair.Value.property);
            }

            // Add normal (non-FK) properties
            foreach (var prop in allProperties)
            {
                if (!fkPropertyNames.Contains(prop.Name))
                {
                    schema[entity][prop.Name] = new PropertySchema(
                        prop.PropertyType,
                        IsNullable(prop),
                        null,         // ReferenceProperty
                        null,         // ReferenceTable
                        null,         // Paths
                        null          // TableName: only set for FK
                    );
                }
            }

            // Add FK properties
            if (fks[entity] != null)
            {
                foreach (var ((foreignKey, tableName), (localPropName, localTableName)) in fks[entity])
                {
                    var localPropInfo = entityType.GetProperty(localPropName);
                    if (localPropInfo == null) continue;

                    var propType = localPropInfo.PropertyType;
                    var isNullable = IsNullable(localPropInfo);

                    TraverseFkChain(
                        entity,
                        localPropName,
                        foreignKey,
                        tableName,
                        localTableName,
                        new List<string>(),
                        schema,
                        fks,
                        propType,
                        isNullable
                    );
                }
            }
        }

        return schema;
    }

    private static void TraverseFkChain(
        string rootEntity,
        string localPropName,
        string fkTarget,
        string referenceTable,
        string localTableName,
        List<string> currentPath,
        Dictionary<string, Dictionary<string, PropertySchema>> schema,
        Dictionary<string, Dictionary<(string, string), (string, string)>?> fks,
        Type propType = null,
        bool propNullable = false)
    {
        string fkEntity = fkTarget.Split('.')[0];

        var aliasPath = string.Join('_', currentPath.Prepend(fkEntity));
        var fullKey = $"{aliasPath}_{rootEntity}";

        if (!schema[rootEntity].TryGetValue(localPropName, out var existing))
        {
            var ps = new PropertySchema(
                propType ?? typeof(int),
                propNullable,
                fkTarget,
                referenceTable,
                new List<string> { fullKey },
                localTableName // TableName only set for FK
            );
            schema[rootEntity][localPropName] = ps;
        }
        else
        {
            if (existing.ReferenceProperty == null)
                existing.ReferenceProperty = fkTarget;
            if (existing.ReferenceTable == null)
                existing.ReferenceTable = referenceTable;
            if (existing.TableName == null)
                existing.TableName = localTableName;

            existing.Paths ??= new List<string>();
            if (!existing.Paths.Contains(fullKey))
                existing.Paths.Add(fullKey);
        }

        if (fks.ContainsKey(fkEntity) && fks[fkEntity] != null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fkEntityType = assembly.GetTypes().FirstOrDefault(t => t.Name == fkEntity);

            foreach (var ((nextFk, nextTable), (nextLocalProp, nextLocalTable)) in fks[fkEntity])
            {
                Type subPropType = typeof(int);
                bool subPropNullable = false;

                if (fkEntityType != null)
                {
                    var propInfo = fkEntityType.GetProperty(nextLocalProp);
                    if (propInfo != null)
                    {
                        subPropType = propInfo.PropertyType;
                        subPropNullable = IsNullable(propInfo);
                    }
                }

                var newPath = new List<string>(new[] { fkEntity }.Concat(currentPath));
                TraverseFkChain(
                    rootEntity,
                    localPropName,
                    nextFk,
                    nextTable,
                    nextLocalTable,
                    newPath,
                    schema,
                    fks,
                    subPropType,
                    subPropNullable
                );
            }
        }
    }

    private static bool IsNullable(PropertyInfo prop)
    {
        if (!prop.PropertyType.IsValueType)
            return true;
        return Nullable.GetUnderlyingType(prop.PropertyType) != null;
    }

    public override string ToString() => PrintSchemaAsCode(SchemaStructure);

    private static string PrintSchemaAsCode(Dictionary<string, Dictionary<string, PropertySchema>> schema)
    {
        var sb = new StringBuilder();

        foreach (var entity in schema)
        {
            sb.AppendLine($"[\"{entity.Key}\"] =");
            sb.AppendLine("{");

            var props = entity.Value.ToList();
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                var typeName = GetFriendlyTypeName(prop.Value.Type);
                var nullable = prop.Value.Nullable ? "true" : "false";

                string line = $"   [\"{prop.Key}\"] = {typeName}, {nullable}";

                // Print TableName (child table) if present (i.e., only for FK)
                if (!string.IsNullOrEmpty(prop.Value.TableName))
                {
                    line += $", Table: \"{prop.Value.TableName}\"";
                }

                if (!string.IsNullOrEmpty(prop.Value.ReferenceProperty))
                {
                    line += $", \"{prop.Value.ReferenceProperty}\"";
                }

                if (!string.IsNullOrEmpty(prop.Value.ReferenceTable))
                {
                    line += $", \"{prop.Value.ReferenceTable}\"";
                }

                if (prop.Value.Paths?.Count > 0)
                {
                    var pathList = string.Join(", ", prop.Value.Paths.Select(p => $"\"{p}\""));
                    line += $", {{{pathList}}}";
                }

                if (i < props.Count - 1)
                    line += ",";

                sb.AppendLine(line);
            }

            sb.AppendLine("},");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == null)
            return "unknown";

        if (Nullable.GetUnderlyingType(type) is Type underlying)
            return GetFriendlyTypeName(underlying) + "?";

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();

            if (typeof(IEnumerable<>).IsAssignableFrom(genericDef) ||
                typeof(ICollection<>).IsAssignableFrom(genericDef) ||
                typeof(List<>).IsAssignableFrom(genericDef) ||
                type.Name.StartsWith("ICollection"))
            {
                var innerType = GetFriendlyTypeName(type.GetGenericArguments()[0]);
                return $"{innerType}[]";
            }

            var typeName = genericDef.Name.Split('`')[0];
            var args = type.GetGenericArguments()
                .Select(GetFriendlyTypeName);
            return $"{typeName}<{string.Join(", ", args)}>";
        }

        return type switch
        {
            { } t when t == typeof(int) => "int",
            { } t when t == typeof(string) => "string",
            { } t when t == typeof(bool) => "bool",
            { } t when t == typeof(double) => "double",
            { } t when t == typeof(float) => "float",
            { } t when t == typeof(decimal) => "decimal",
            { } t when t == typeof(long) => "long",
            { } t when t == typeof(short) => "short",
            { } t when t == typeof(byte) => "byte",
            { } t when t == typeof(char) => "char",
            _ => type.Name
        };
    }

    public SchemaMatchResult? FindSchemaByPathMatch(List<string> keys)
    {
        var expected = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);

        foreach (var entity in this.SchemaStructure)
        {
            foreach (var (propertyKey, propertySchema) in entity.Value)
            {
                if (propertySchema.Paths == null)
                    continue;

                foreach (var path in propertySchema.Paths)
                {
                    var parts = path.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    var partSet = new HashSet<string>(parts, StringComparer.OrdinalIgnoreCase);

                    if (partSet.SetEquals(expected))
                    {
                        return new SchemaMatchResult(
                            Property: propertyKey,
                            Type: propertySchema.Type,
                            IsNullable: propertySchema.Nullable,
                            ReferenceProperty: propertySchema.ReferenceProperty ?? "",
                            ReferenceTable: propertySchema.ReferenceTable ?? "",
                            TableName: propertySchema.TableName ?? "",
                            Path: path
                        );
                    }
                }
            }
        }

        return null;
    }

    public SchemaMatchResult? FindSchemaByPathMatch(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        foreach (var entity in SchemaStructure)
        {
            foreach (var (propertyKey, propertySchema) in entity.Value)
            {
                if (propertySchema.Paths == null)
                    continue;

                foreach (var existingPath in propertySchema.Paths)
                {
                    if (string.Equals(existingPath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        return new SchemaMatchResult(
                            Property: propertyKey,
                            Type: propertySchema.Type,
                            IsNullable: propertySchema.Nullable,
                            ReferenceProperty: propertySchema.ReferenceProperty ?? "",
                            ReferenceTable: propertySchema.ReferenceTable ?? "",
                            TableName: propertySchema.TableName ?? "",
                            Path: existingPath
                        );
                    }
                }
            }
        }

        return null;
    }
}

public class PropertySchema
{
    public PropertySchema(
        Type type,
        bool nullable,
        string? referenceProperty = null,
        string? referenceTable = null,
        List<string>? paths = null,
        string? tableName = null)
    {
        Type = type;
        Nullable = nullable;
        ReferenceProperty = referenceProperty;
        ReferenceTable = referenceTable;
        Paths = paths;
        TableName = tableName; // Only set for FK properties
    }

    public Type Type { get; set; }
    public bool Nullable { get; set; }
    public string? ReferenceProperty { get; set; }  // like "Employee.Id"
    public string? ReferenceTable { get; set; }     // like "Employees"
    public List<string>? Paths { get; set; }
    public string? TableName { get; set; }          // Only set for FK properties
}

public record SchemaMatchResult(
    string Property,
    Type Type,
    bool IsNullable,
    string ReferenceProperty,
    string ReferenceTable,
    string TableName,
    string Path);