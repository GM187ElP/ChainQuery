using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using _1.Schema;


namespace _1.SqlBuilder;



public class SqlBuilder
{
    private readonly Dictionary<string, Dictionary<string, PropertySchema>> _schema;
    private readonly IModel _model;

    public SqlBuilder(Dictionary<string, Dictionary<string, PropertySchema>> schema, IModel model)
    {
        _schema = schema;
        _model = model;
    }

    private string GetTableName(string entityName)
    {
        var entityType = _model.GetEntityTypes()
            .FirstOrDefault(et => et.ClrType.Name == entityName);
        return entityType?.GetTableNameSafe() ?? entityName; // fallback if not found
    }

    public string BuildQuery(JObject json)
    {
        var root = json.Properties().First();
        string rootEntity = root.Name;
        string rootTable = GetTableName(rootEntity);

        var columns = new List<string>();
        var joins = new List<string>();

        BuildSelect((JObject)root.Value, rootEntity, "t0", 0, columns, joins);

        string selectClause = string.Join(", ", columns);
        string joinClause = string.Join(" ", joins);

        return $"SELECT {selectClause} FROM {rootTable} t0 {joinClause}";
    }

    private void BuildSelect(
        JObject fields,
        string currentEntity,
        string currentAlias,
        int depth,
        List<string> columns,
        List<string> joins)
    {
        foreach (var field in fields.Properties())
        {
            if (field.Value.Type == JTokenType.Boolean && field.Value.Value<bool>())
            {
                columns.Add($"{currentAlias}.{field.Name} AS [{currentEntity}.{field.Name}]");
            }
            else if (field.Value.Type == JTokenType.Object)
            {
                string childEntity = field.Name;
                string childTable = GetTableName(childEntity);
                string childAlias = $"t{depth + 1}";

                // Normalize both entity names to match the schema dictionary
                string childEntityName = _model.GetEntityNameFromTableName(childEntity) ?? childEntity;
                string currentEntityName = _model.GetEntityNameFromTableName(currentEntity) ?? currentEntity;

                if (!_schema.TryGetValue(childEntityName, out var childSchema))
                    throw new InvalidOperationException($"Schema not found for entity '{childEntityName}'.");

                string fkName = "";
                //childSchema.First(p => p.Value.RefEntityType?.Name == currentEntityName).Key;

                joins.Add($"LEFT JOIN {childTable} {childAlias} ON {childAlias}.{fkName} = {currentAlias}.Id");

                BuildSelect((JObject)field.Value, childEntity, childAlias, depth + 1, columns, joins);
            }
        }
    }
}

public static class EfCoreExtensions
{
    public static string GetTableNameSafe(this IEntityType entityType)
    {
        return entityType.GetTableName() ?? entityType.ClrType.Name;
    }

    public static string? GetEntityNameFromTableName(this IModel model, string tableName)
    {
        var entityType = model.GetEntityTypes()
            .FirstOrDefault(et => et.GetTableName()?.Equals(tableName, StringComparison.OrdinalIgnoreCase) == true);

        return entityType?.ClrType.Name;
    }

}


