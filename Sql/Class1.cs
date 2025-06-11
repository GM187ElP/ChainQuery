using _1.FKs;
using _1.Schema;
using _1.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

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

public class Query
{
    private string _fromTable = "";
    private readonly List<(string Expression, string? Alias)> _selectColumns = new();
    private readonly List<(string Table, JoinType JoinType, string OnClause)> _joins = new();
    private SqlCondition? _whereCondition;
    private bool _distinct = false;
    private string? _orderByColumn;
    private bool _orderByAscending = true;
    private int? _offset = null;
    private int? _fetch = null;

    private Query() { }

    public static Query From(string tableName)
    {
        var qb = new Query();
        qb._fromTable = tableName;
        return qb;
    }

    public Query Select(params (string tableOrProperty, string? alias)[] columns)
    {
        foreach (var (tableOrProperty, alias) in columns)
        {
            if (string.IsNullOrEmpty(alias))
            {
                _selectColumns.Add((tableOrProperty, null));
            }
            else
            {
                _selectColumns.Add((tableOrProperty, alias));
            }
        }
        return this;
    }


    public Query Join(string tableName, JoinType joinType, string? fromTable = null)
    {
        _joins.Add((tableName, joinType, fromTable ?? _fromTable));
        return this;
    }

    public Query Where(SqlCondition condition)
    {
        _whereCondition = condition;
        return this;
    }

    public Query Distinct()
    {
        _distinct = true;
        return this;
    }

    public Query OrderBy(string column, bool ascending = true)
    {
        _orderByColumn = column;
        _orderByAscending = ascending;
        return this;
    }

    public Query Offset(int skip, int take)
    {
        _offset = skip;
        _fetch = take;
        return this;
    }

    public Query Page(int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        return Offset(skip, pageSize);
    }

    // --- New Methods ---
    public class ConditionDto
    {
        public string? Type { get; set; } // "Single", "And", "Or"
        public string? Left { get; set; }
        public string? Right { get; set; }
        public Operator? Operator { get; set; }
        public List<ConditionDto>? Children { get; set; }

        public static ConditionDto FromSqlCondition(SqlCondition cond)
        {
            return cond switch
            {
                SingleCondition s => new ConditionDto
                {
                    Type = "Single",
                    Left = s.Left,
                    Operator = s.Op,
                    Right = s.Right
                },
                AndGroup and => new ConditionDto
                {
                    Type = "And",
                    Children = and.Conditions.Select(FromSqlCondition).ToList()
                },
                OrGroup or => new ConditionDto
                {
                    Type = "Or",
                    Children = or.Conditions.Select(FromSqlCondition).ToList()
                },
                _ => throw new NotSupportedException("Unknown SqlCondition")
            };
        }

        public SqlCondition ToSqlCondition()
        {
            return Type switch
            {
                "Single" => new SingleCondition(Left!, Operator!.Value, Right!),
                "And" => new AndGroup(Children!.Select(c => c.ToSqlCondition())),
                "Or" => new OrGroup(Children!.Select(c => c.ToSqlCondition())),
                _ => throw new NotSupportedException("Unknown ConditionDto type")
            };
        }
    }

    // DTO class for serialization
    private class QueryDto
    {
        public string FromTable { get; set; } = "";
        public List<(string Expression, string? Alias)> SelectColumns { get; set; } = new();
        public List<(string Table, JoinType JoinType, string OnClause)> Joins { get; set; } = new();
        public ConditionDto? WhereCondition { get; set; } // ✅ structured
        public bool Distinct { get; set; }
        public string? OrderByColumn { get; set; }
        public bool OrderByAscending { get; set; }
        public int? Offset { get; set; }
        public int? Fetch { get; set; }
    }


    // Serialize to JSON for transport (frontend -> backend)
    public string ToTransport()
    {
        var dto = new QueryDto
        {
            FromTable = _fromTable,
            SelectColumns = _selectColumns,
            Joins = _joins,
            WhereCondition = _whereCondition != null ? ConditionDto.FromSqlCondition(_whereCondition) : null,
            Distinct = _distinct,
            OrderByColumn = _orderByColumn,
            OrderByAscending = _orderByAscending,
            Offset = _offset,
            Fetch = _fetch,
        };
        return JsonSerializer.Serialize(dto);
    }


    // Parse Query from JSON on backend
    public static Query ParseFromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<QueryDto>(json);
        if (dto == null) throw new Exception("Invalid query JSON");

        var query = new Query
        {
            _fromTable = dto.FromTable,
            _distinct = dto.Distinct,
            _orderByColumn = dto.OrderByColumn,
            _orderByAscending = dto.OrderByAscending,
            _offset = dto.Offset,
            _fetch = dto.Fetch,
            _whereCondition = dto.WhereCondition?.ToSqlCondition()
        };

        query._selectColumns.AddRange(dto.SelectColumns);
        query._joins.AddRange(dto.Joins);

        return query;
    }

    //var backQuery = Query
    //         .From(SN.Employee.Table)
    //         .Select((SN.BankAccount.Name, "b.name"), (SN.City.Name, "c.name"))
    //         .Join(SN.City.Table, JoinType.Left, SN.Employee.Table)
    //         .Join(SN.BankAccount.Table, JoinType.Left, SN.Employee.Table)
    //         .ToExecutable(schema);


    public string ToExecutable(SchemaBuilder schemaBuilder)
    {
        #region Init
        var involvedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { _fromTable };
        foreach (var (tableName, _, _) in _joins)
            involvedTables.Add(tableName);

        foreach (var item in _selectColumns.Select(x => x.Expression).ToList())
        {
            var part0 = item.Contains('.') ? item.Split('.')[0] : item;
            involvedTables.Add(part0);
        }
        involvedTables.Add(_fromTable);

        var pathMatch = schemaBuilder.FindSchemaByTableNameFullPath(involvedTables.ToList());
        if (pathMatch == null)
            throw new InvalidOperationException("No path found connecting all involved tables.");

        var entity2TableNames = pathMatch.TableNames;
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

                var match = schemaBuilder.FindSchemaByPathMatch(keyForward)
                          ?? schemaBuilder.FindSchemaByPathMatch(keyReverse);

                if (match == null)
                    throw new InvalidOperationException($"Cannot resolve join info for path segment: {left}-{right}");

                joinInfoDict[$"{left}_{right}"] = match;
            }
        }
        else
        {
            var match = schemaBuilder.FindSchemaByPathMatch(fullPath)
                ?? schemaBuilder.FindSchemaByPathMatch(new string(fullPath.Reverse().ToArray()));

            if (match == null)
                throw new InvalidOperationException($"Cannot resolve join info for path segment: {fullPath}");

            joinInfoDict[fullPath] = match;
        }


        #endregion

        var sb = new StringBuilder();

        // Alias resolver
        string GetAlias(string table)
        {
            foreach (var (entity, tableName) in entity2TableNames)
            {
                if (tableName.Equals(table, StringComparison.OrdinalIgnoreCase))
                    return entity.Substring(0, 1).ToLowerInvariant();
            }
            return table.Substring(0, 1).ToLowerInvariant();
        }

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

                var tableAlias = GetAlias(match.TableName);
                var columnAlias = !string.IsNullOrEmpty(alias) ? alias : column;

                selectParts.Add($"{match.TableName}.{column} AS {columnAlias} ");
            }
            sb.AppendLine("SELECT " + string.Join(", ", selectParts));
        }
        else
        {
            sb.AppendLine("SELECT *");
        }

        // FROM
        var fromAlias = GetAlias(_fromTable);
        sb.AppendLine($"FROM {entity2TableNames.FirstOrDefault(x => x.Entity.Equals(_fromTable, StringComparison.OrdinalIgnoreCase)).TableName} AS {fromAlias} ");

        var count = 0;
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

            var leftAlias = GetAlias(joinInfo.TableName);
            var rightAlias = GetAlias(joinInfo.ReferenceTable);

            sb.AppendLine($"{joinKeyword} {joinInfo.ReferenceTable} AS {rightAlias} ON {leftAlias}.{joinInfo.Property} = {rightAlias}.{joinInfo.ReferenceProperty.Split('.')[1]} ");
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
    }



}

