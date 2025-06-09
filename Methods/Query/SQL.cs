using _1.Methods.Query.Classes;

namespace _1.Methods.Query;

public record SqlQuery
{
    public SqlWhereGroup? Where { get; set; }
    public List<SqlSelectField>? Select { get; set; } = new();
    public SqlFromField From { get; set; } = new();



    public string? WithAlias { get; set; }
    public SqlQuery? WithQuery { get; set; }
    public bool? Distinct { get; set; }
    public int? Top { get; set; }
    public List<SqlJoin> Joins { get; set; } = new();
    public List<string> GroupBy { get; set; } = new();
    public SqlCondition? Having { get; set; }
    public List<SqlOrderBy> OrderBy { get; set; } = new();
    public int? Offset { get; set; }
    public int? Fetch { get; set; }
    public List<SqlQuery>? Union { get; set; }
    public List<SqlQuery>? Intersect { get; set; }
}
public record SqlRequest
{
    public SqlQuery? Query { get; set; }
    public SqlMutation? Mutation { get; set; }
    public SqlMerge? Merge { get; set; }
    public SqlTransaction? Transaction { get; set; }
    public SqlDefinition? Definition { get; set; }
    public SqlUtility? Utility { get; set; }
    public SqlSet? Set { get; set; }
    public SqlVariableDeclaration? VariableDeclaration { get; set; }
    public SqlVariableAssignment? VariableAssignment { get; set; }
    public SqlCursor? Cursor { get; set; }
}

public record SqlMerge
{
    public string TargetTable { get; set; } = null!;
    public string SourceTable { get; set; } = null!;
    public SqlCondition OnCondition { get; set; } = null!;

    public Dictionary<string, object>? InsertValues { get; set; }
    public Dictionary<string, object>? UpdateValues { get; set; }
    public bool DeleteWhenNotMatched { get; set; } = false;

    public List<string>? OutputColumns { get; set; }  // Added OUTPUT clause support
}

public record SqlDefinition
{
    public SqlDefinitionType Type { get; set; }
    public string ObjectName { get; set; } = null!;  // Table, View, Index, etc.
    public string? Definition { get; set; }          // Optional raw DDL SQL body or script
    public bool IsTemporary { get; set; } = false;   // For temp tables (#temp)
}


public record SqlTransaction
{
    public TransactionControlType Type { get; set; }
    public string? Name { get; set; }  // For SAVEPOINT or named transactions
    public IsolationLevel? IsolationLevel { get; set; }  // Optional isolation level when starting
}




public record SqlJoin
{
    public SqlJoinType Type { get; set; } = SqlJoinType.Inner;
    public string Table { get; set; } = null!;
    public SqlCondition? On { get; set; }
}









public record SqlMutation
{
    public SqlMutationType Type { get; set; } = SqlMutationType.Insert;
    public string Table { get; set; } = null!;
    public Dictionary<string, object>? Values { get; set; }
    public SqlCondition? Where { get; set; }
}

public record SqlUtility
{
    public SqlUtilityType Type { get; set; }

    // For EXEC only: Procedure name + structured parameters
    public string? ProcedureName { get; set; }
    public List<object>? Parameters { get; set; }

    // For other commands like USE, PRINT, WAITFOR, raw command text if needed
    public string? CommandText { get; set; }

    public WaitForOptions? WaitForOptions { get; set; }  // For WAITFOR details
}


public record WaitForOptions
{
    public TimeSpan? Delay { get; set; }
    public TimeSpan? Time { get; set; }
}


// New: Support for SET commands (like SET NOCOUNT ON)
public record SqlSet
{
    public string Variable { get; set; } = null!;  // e.g. "NOCOUNT"
    public string Value { get; set; } = null!;     // e.g. "ON"
}

// New: Variables declaration
public record SqlVariableDeclaration
{
    public string Name { get; set; } = null!;     // e.g. "@myVar"
    public string DataType { get; set; } = null!; // e.g. "INT", "VARCHAR(50)"
    public object? DefaultValue { get; set; }
}

// New: Variable assignment
public record SqlVariableAssignment
{
    public string Name { get; set; } = null!;
    public object? Value { get; set; }
}

// New: Cursor support (basic)
public record SqlCursor
{
    public string Name { get; set; } = null!;
    public string ForQuery { get; set; } = null!;
    public CursorOperation? Operation { get; set; }
}

