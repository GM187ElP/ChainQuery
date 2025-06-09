using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public enum SqlUtilityType
{
    Execute,  // EXEC / EXECUTE stored procedures or dynamic SQL
    Use,      // USE database
    Print,    // PRINT message
    WaitFor   // WAITFOR DELAY / TIME
}
public enum SqlJoinType
{
    Inner,
    Left,
    Right,
    Full,
    Cross
}

public enum SqlAggregateFunction
{
    Sum,
    Avg,
    Min,
    Max,
    Count
}

public enum SqlDefinitionType
{
    Create,
    Alter,
    Drop,
    Truncate
}

public enum TransactionControlType
{
    BeginTransaction,
    Commit,
    Rollback,
    Savepoint
}

public enum IsolationLevel
{
    ReadUncommitted,
    ReadCommitted,
    RepeatableRead,
    Serializable,
    Snapshot
}
public enum SqlConditionOperator
{
    And,
    Or,
    Not,
    Like,
    In,
    Between,
    Exists
}

public enum SqlComparisonType
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    IsNull,
    IsNotNull,
    In
}

public enum SqlExpressionType
{
    Case,
    Coalesce,
    IsNull
}

public enum SqlWindowFunctionType
{
    RowNumber,
    Rank,
    DenseRank,
    Ntile,
    Lag,
    Lead
}

public enum SqlMutationType
{
    Insert,
    Update,
    Delete
}
public enum CursorOperation
{
    Declare,
    Open,
    FetchNext,
    Close,
    Deallocate
}
