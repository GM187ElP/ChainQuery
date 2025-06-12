using _1.Schema;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// --- Enums and SqlCondition Classes ---

public enum Operator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    Like
}

public enum JoinType
{
    Inner,
    Left,
    Right,
    Full
}

public abstract class SqlCondition
{
    public abstract string ToSql();

    public static SingleCondition Single(string left, Operator op, string right) =>
        new SingleCondition(left, op, right);

    public static AndGroup And(params SqlCondition[] conditions) =>
        new AndGroup(conditions);

    public static OrGroup Or(params SqlCondition[] conditions) =>
        new OrGroup(conditions);
}

public class SingleCondition : SqlCondition
{
    public string Left { get; }
    public Operator Op { get; }
    public string Right { get; }

    public SingleCondition(string left, Operator op, string right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public override string ToSql()
    {
        var opStr = Op switch
        {
            Operator.Equal => "=",
            Operator.NotEqual => "<>",
            Operator.GreaterThan => ">",
            Operator.LessThan => "<",
            Operator.GreaterOrEqual => ">=",
            Operator.LessOrEqual => "<=",
            Operator.Like => "LIKE",
            _ => throw new NotSupportedException()
        };

        bool isValue = !(Right.Contains(".") || Right.Contains("("));
        string rightStr = isValue ? $"'{Right}'" : Right;

        return $"{Left} {opStr} {rightStr}";
    }
}

public class AndGroup : SqlCondition
{
    public List<SqlCondition> Conditions { get; }

    public AndGroup(IEnumerable<SqlCondition> conditions)
    {
        Conditions = conditions.ToList();
    }

    public override string ToSql() =>
        "(" + string.Join(" AND ", Conditions.Select(c => c.ToSql())) + ")";
}

public class OrGroup : SqlCondition
{
    public List<SqlCondition> Conditions { get; }

    public OrGroup(IEnumerable<SqlCondition> conditions)
    {
        Conditions = conditions.ToList();
    }

    public override string ToSql() =>
        "(" + string.Join(" OR ", Conditions.Select(c => c.ToSql())) + ")";
}

// --- QueryBuilder and Query<TDto> ---

public class QueryBuilder
{
    private readonly string _connectionString;
    private readonly SchemaBuilder _schemaBuilder;

    public QueryBuilder(string connectionString, SchemaBuilder schemaBuilder)
    {
        _connectionString = connectionString;
        _schemaBuilder = schemaBuilder;
    }

    public Query<TDto> Query<TDto>() where TDto : new()
        => new Query<TDto>(_connectionString, _schemaBuilder);
}

public class Query<TDto> where TDto : new()
{
    private readonly string _connectionString;
    private readonly SchemaBuilder _schemaBuilder;

    private string _fromTable = "";
    private readonly List<(string Expression, string? Alias)> _selectColumns = new();
    private readonly List<(string Table, JoinType JoinType, string OnClause)> _joins = new();
    private SqlCondition? _whereCondition;
    private bool _distinct = false;
    private string? _orderByColumn;
    private bool _orderByAscending = true;
    private int? _offset = null;
    private int? _fetch = null;

    internal Query(string connectionString, SchemaBuilder schemaBuilder)
    {
        _connectionString = connectionString;
        _schemaBuilder = schemaBuilder;
    }

    public Query<TDto> From(string tableName)
    {
        _fromTable = tableName;
        return this;
    }

    public Query<TDto> Select(params (string sourceColumn, Expression<Func<TDto, object>> dtoProperty)[] columns)
    {
        foreach (var (source, expr) in columns)
        {
            string alias = GetPropertyName(expr);
            _selectColumns.Add((source, alias));
        }
        return this;
    }

    public Query<TDto> Join(string tableName, JoinType joinType, string? fromTable = null)
    {
        _joins.Add((tableName, joinType, fromTable ?? _fromTable));
        return this;
    }

    public Query<TDto> Where(SqlCondition condition)
    {
        _whereCondition = condition;
        return this;
    }

    public Query<TDto> Distinct()
    {
        _distinct = true;
        return this;
    }

    public Query<TDto> OrderBy(string column, bool ascending = true)
    {
        _orderByColumn = column;
        _orderByAscending = ascending;
        return this;
    }

    public Query<TDto> Offset(int skip, int take)
    {
        _offset = skip;
        _fetch = take;
        return this;
    }

    public Query<TDto> Page(int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        return Offset(skip, pageSize);
    }

    public List<TDto> ToList()
    {
        var sql = ToExecutable();
        var data = ExecuteQuery(sql);
        return MapToDtos(data);
    }

    public TDto? FirstOrDefault() => ToList().FirstOrDefault();

    // ---- Internals ----

    private string GetPropertyName(Expression<Func<TDto, object>> expr)
    {
        if (expr.Body is MemberExpression member)
            return member.Member.Name;
        if (expr.Body is UnaryExpression unary && unary.Operand is MemberExpression member2)
            return member2.Member.Name;
        throw new InvalidOperationException("Expression must be a property access");
    }

    private List<Dictionary<string, object?>> ExecuteQuery(string sql)
    {
        var results = new List<Dictionary<string, object?>>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row[columnName] = value;
                        }
                        results.Add(row);
                    }
                }
            }
        }
        return results;
    }

    private List<TDto> MapToDtos(List<Dictionary<string, object?>> data)
    {
        var props = typeof(TDto).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var result = new List<TDto>();

        foreach (var row in data)
        {
            var obj = new TDto();
            foreach (var prop in props)
            {
                var key = row.Keys.FirstOrDefault(k =>
                    string.Equals(k, prop.Name, StringComparison.OrdinalIgnoreCase));
                if (key != null && row[key] != null && prop.CanWrite)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[key], prop.PropertyType));
                }
            }
            result.Add(obj);
        }
        return result;
    }

    public string ToExecutable()
    {
        // -- Begin your ToExecutable logic (as per your original code) --
        var involvedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { _fromTable };
        foreach (var (tableName, _, _) in _joins)
            involvedTables.Add(tableName);

        foreach (var item in _selectColumns.Select(x => x.Expression).ToList())
        {
            var part0 = item.Contains('.') ? item.Split('.')[0] : item;
            involvedTables.Add(part0);
        }
        involvedTables.Add(_fromTable);

        var pathMatch = _schemaBuilder.FindSchemaByTableNameFullPath(involvedTables.ToList());
        if (pathMatch == null)
            throw new InvalidOperationException("No path found connecting all involved tables.");

        var entity2TableNames = pathMatch.TableNames;
        string GetTableName(string table) => entity2TableNames.FirstOrDefault(t => t.Entity == table).TableName;  // get table names
        Dictionary<string, string> aliases = [];

        int count = 0;
        foreach (var item in entity2TableNames)
            SetAlias(item.TableName);

        void SetAlias(string key)
        {
            if (!aliases.Keys.Any(x => x == key))
            {
                var al = key.Substring(0, 1).ToLowerInvariant();
                if (aliases.Values.Any(x => x == al))
                {
                    al = "T" + count;
                    count++;
                }
                aliases.Add(key, al);
            }
        }

        string GetAlias(string key) => aliases[key];

        var fullPath = pathMatch.Path;
        string[] pathParts = [];

        var joinInfoDict = new Dictionary<string, SchemaMatchResult>(StringComparer.OrdinalIgnoreCase);

        if (fullPath.Count(x => x == '_') > 1)
        {
            pathParts = fullPath.Split('_', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var left = pathParts[i];
                var right = pathParts[i + 1];

                var keyForward = $"{left}_{right}";
                var keyReverse = $"{right}_{left}";

                var match = _schemaBuilder.FindSchemaByPathMatch(keyForward)
                          ?? _schemaBuilder.FindSchemaByPathMatch(keyReverse);

                if (match == null)
                    throw new InvalidOperationException($"Cannot resolve join info for path segment: {left}-{right}");

                joinInfoDict[$"{left}_{right}"] = match;
            }
        }
        else
        {
            var match = _schemaBuilder.FindSchemaByPathMatch(fullPath)
                ?? _schemaBuilder.FindSchemaByPathMatch(new string(fullPath.Reverse().ToArray()));

            if (match == null)
                throw new InvalidOperationException($"Cannot resolve join info for path segment: {fullPath}");

            joinInfoDict[fullPath] = match;
        }

        var sb = new StringBuilder();


        // Alias resolver
        //string GetAlias(string table)
        //{
        //    foreach (var (entity, tableName) in entity2TableNames)
        //    {
        //        if (tableName.Equals(table, StringComparison.OrdinalIgnoreCase))
        //            return entity.Substring(0, 2).ToLowerInvariant();
        //    }
        //    return table.Substring(0, 2).ToLowerInvariant();
        //}

        // SELECT
        if (_selectColumns.Count > 0)
        {
            var selectParts = new List<string>();
            foreach (var (expression, alias) in _selectColumns)
            {
                string entity;
                string column = null;

                var parts = expression.Split('.');
                if (parts.Length != 2)
                    column = "*";
                entity = parts[0];
                column = column ?? parts[1];

                var match = entity2TableNames.FirstOrDefault(x => x.Entity.Equals(entity, StringComparison.OrdinalIgnoreCase));
                if (match == default)
                    throw new InvalidOperationException($"Unknown entity in SELECT: {entity}");

                var tableAlias = GetAlias(GetTableName( match.TableName));
                var columnAlias = !string.IsNullOrEmpty(alias) ? alias : column;

                selectParts.Add($"{tableAlias}.{column.ToLowerInvariant()} AS [{alias}] ");
            }
            sb.AppendLine("SELECT " + string.Join(", ", selectParts));
        }
        else
        {
            sb.AppendLine("SELECT *");
        }

        // FROM
        var fromAlias = GetAlias(GetTableName(_fromTable));
        sb.AppendLine($"FROM {entity2TableNames.FirstOrDefault(x => x.Entity.Equals(_fromTable, StringComparison.OrdinalIgnoreCase)).TableName} AS {fromAlias} ");

        // JOINs
        for (int i = 0; i < pathParts.Length - 1; i++)
        {
            var left = pathParts[i];
            var right = pathParts[i + 1];
            var key = $"{left}_{right}";

            if (!joinInfoDict.TryGetValue(key, out var joinInfo))
                throw new InvalidOperationException($"Join info not found for key: {key}");

            var join = _joins.FirstOrDefault(j =>
                j.Table.Equals(right, StringComparison.OrdinalIgnoreCase) ||
                j.Table.Equals(left, StringComparison.OrdinalIgnoreCase));

            var joinKeyword = join.JoinType switch
            {
                JoinType.Left => "LEFT JOIN",
                JoinType.Right => "RIGHT JOIN",
                JoinType.Full => "FULL JOIN",
                _ => "INNER JOIN"
            };

            var joinFromAlias = GetAlias(join.OnClause);
            var joinTableAlias = GetAlias(join.Table);

            string on;
            if (join.Table == left)
                on = $"{joinTableAlias}.{joinInfo.ReferenceProperty.Split('.')[1]} = {joinFromAlias}.{joinInfo.Property}";
            else
                on = $"{joinTableAlias}.{joinInfo.Property} = {joinFromAlias}.{joinInfo.ReferenceProperty.Split('.')[1]}";

            sb.AppendLine($"{joinKeyword} {GetTableName(joinTableAlias)} AS {joinTableAlias} ON {on} ");
        }

        // WHERE
        if (_whereCondition != null)
            sb.AppendLine("WHERE " + _whereCondition.ToSql());

        // ORDER BY
        if (!string.IsNullOrEmpty(_orderByColumn))
        {
            var dir = _orderByAscending ? "ASC" : "DESC";
            sb.AppendLine($"ORDER BY {_orderByColumn} {dir}");
        }

        // Pagination
        if (_fetch.HasValue)
        {
            sb.AppendLine($"OFFSET {_offset.GetValueOrDefault(0)} ROWS");
            sb.AppendLine($"FETCH NEXT {_fetch.Value} ROWS ONLY");
        }

        return sb.ToString().Trim();
        // -- End your ToExecutable logic --
    }
}

