﻿using _1.Schema;
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
    private readonly List<string> _selectColumns = new();
    private readonly List<(string Table, JoinType Type)> _joins = new();
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
                _selectColumns.Add(tableOrProperty);
            else
                _selectColumns.Add($"{tableOrProperty} AS {alias}");
        }
        return this;
    }

    public Query Join(string tableName, JoinType joinType)
    {
        _joins.Add((tableName, joinType));
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
        public List<string> SelectColumns { get; set; } = new();
        public List<(string Table, JoinType Type)> Joins { get; set; } = new();
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


    // Generate executable SQL string (simplified)
    public string ToExecutable(Dictionary<string, Dictionary<string, PropertySchema>> schema) 
    {
        var sb = new System.Text.StringBuilder();

        sb.Append("SELECT ");
        if (_distinct) sb.Append("DISTINCT ");
        sb.Append(_selectColumns.Count > 0 ? string.Join(", ", _selectColumns) : "*");

        sb.Append(" FROM ").Append(_fromTable);

        foreach (var (table, joinType) in _joins)
        {
            string joinStr = joinType switch
            {
                JoinType.Inner => "INNER JOIN",
                JoinType.Left => "LEFT JOIN",
                JoinType.Right => "RIGHT JOIN",
                JoinType.Full => "FULL JOIN",
                _ => throw new NotSupportedException()
            };
            sb.Append($" {joinStr} {table}");
        }

        if (_whereCondition != null)
        {
            sb.Append(" WHERE ").Append(_whereCondition.ToSql());
        }

        if (!string.IsNullOrEmpty(_orderByColumn))
        {
            sb.Append(" ORDER BY ").Append(_orderByColumn);
            sb.Append(_orderByAscending ? " ASC" : " DESC");
        }

        if (_fetch.HasValue)
        {
            if (!_offset.HasValue) _offset = 0;
            sb.Append($" OFFSET {_offset} ROWS FETCH NEXT {_fetch} ROWS ONLY");
        }

        return sb.ToString();
    }
}

