using _1.Methods.Query.Classes;

namespace _1.Methods.Query.Interfaces;

public interface IHasAlias
{
    string? Alias { get; set; }
}

public interface IHasAggregate
{
    SqlAggregateFunction? AggregateFunction { get; set; }
}

public interface IHasExpression
{
    SqlExpression? Expression { get; set; }
}
public interface IHasField
{
    string? Field { get; set; }
}
public interface IHasWindow
{
    public SqlWindowFunction? Window { get; set; }
}


public interface IHasValue
{
    object? Value { get; set; }
}

public interface IHasValues
{
    List<object> Values { get; set; }
}

public interface IHasComparison
{
    SqlComparisonType? Comparison { get; set; }
}

public interface IHasNegate
{
    bool? Negate { get; set; }
}



public interface IHasOperator
{
    SqlConditionOperator? Operator { get; set; }
}

public interface IHasConditions
{
    List<SqlCondition> Conditions { get; set; }
}

public interface IBaseExpression
{
    SqlExpressionType? Type { get; set; }
}

public interface ICaseExpression
{
    List<(string When, string Then)>? CaseWhenThen { get; set; }
    string? CaseElse { get; set; }
}

public interface ICoalesceExpression
{
    List<string>? CoalesceValues { get; set; }
}

public interface IDefaultExpression
{
    string? Default { get; set; }
}

public interface IHasWindowFunction
{
    SqlWindowFunctionType Function { get; set; }
}

public interface IHasPartitionBy
{
    List<string>? PartitionBy { get; set; }
}

public interface IHasOrderBy
{
    List<SqlOrderBy>? OrderBy { get; set; }
}
public interface IHasDescending
{
    public bool Descending { get; set; } 
}