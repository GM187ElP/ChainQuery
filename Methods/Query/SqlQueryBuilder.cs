using _1.Methods.Builders;
using _1.Methods.Query.Classes;
using System.Linq.Expressions;

namespace _1.Methods.Query;

// Builder stubs
public class SqlQueryBuilder
{
    private SqlQuery _query = new();

    public SqlQueryBuilder From(string table)
    {
        _query = _query with { From = new SqlFromField { Table = table } };
        return this;
    }

    //public SqlQueryBuilder Where(Func<SqlConditionBuilder, SqlCondition> builderFunc)
    //{
    //    var builder = builderFunc(new SqlConditionBuilder());
    //    //_query = _query with { Where = builder };
    //    return this;
    //}

    public SqlQueryBuilder Select(Func<ListSqlSelectFieldBuilder, ListSqlSelectFieldBuilder> builderFunc)
    {
        var builder = builderFunc(new ListSqlSelectFieldBuilder());
        _query = _query with { Select = builder.Build() };
        return this;
    }

    public SqlQueryBuilder Top(int count)
    {
        _query.Top = count;
        return this;
    }

    public SqlQuery Build() => _query;
}

public class ListSqlSelectFieldBuilder
{
    private List<SqlSelectField> _selectFieldList = [];

    public ListSqlSelectFieldBuilder SqlSelectField(
        string field, string?
        alias = null,
        SqlAggregateFunction? function = null,
        Func<SqlExpressionBuilder, SqlExpressionBuilder>? expressionFunc = null,
        Func<SqlWindowFunctionBuilder, SqlWindowFunctionBuilder>? windowFunctionFunc = null)
    {
        var expressionFuncBuilder = expressionFunc(new SqlExpressionBuilder());
        var windowFunctionFuncBuilder = windowFunctionFunc(new SqlWindowFunctionBuilder());
        _selectFieldList.Add(new Classes.SqlSelectField
        {
            Field = field,
            Alias = alias,
            AggregateFunction = function,


            Expression = expressionFuncBuilder.Build(),
            Window = windowFunctionFuncBuilder.Build()

        });
        return this;
    }

    public List<SqlSelectField> Build() => _selectFieldList;
}

public class SqlWindowFunctionBuilder
{
    private SqlWindowFunction _windowFunction = new();

    public SqlWindowFunctionBuilder Function(SqlWindowFunctionType function)
    {
        _windowFunction.Function = function;
        return this;
    }
    public SqlWindowFunctionBuilder OrderBy(Func<ListSqlOrderByBuilder, ListSqlOrderByBuilder> orderByListFunc)
    {
        var builder = orderByListFunc(new ListSqlOrderByBuilder());
        _windowFunction.OrderBy = _windowFunction.OrderBy = builder.Build();
        return this;
    }
    public SqlWindowFunctionBuilder PartitionBy(Func<ListStringBuilder, ListStringBuilder> partitionByListFunc)
    {
        var builder = partitionByListFunc(new ListStringBuilder());
        _windowFunction.PartitionBy = builder.Build();
        return this;
    }

    public SqlWindowFunction Build() => _windowFunction;
}

public class ListStringBuilder
{
    private List<string> _partitionByList = new();

    public ListStringBuilder PartitionBy(string value)
    {
        _partitionByList.Add(value);
        return this;
    }
    public List<string> Build() => _partitionByList;
}

public class ListSqlOrderByBuilder
{
    private List<SqlOrderBy> _orderByList = new();

    public ListSqlOrderByBuilder OrderBy(string field, bool? descending=null)
    {
        _orderByList.Add(new SqlOrderBy
        {
            Field = field,
            Descending = descending ?? false
        });
        return this;
    }
    public List<SqlOrderBy> Build() => _orderByList;
}

public class SqlExpressionBuilder
{
    private SqlExpression _expression = new();

    public SqlExpressionBuilder Type(SqlExpressionType type)
    {
        _expression.Type = type;
        return this;
    }

    public SqlExpressionBuilder CaseWhenThen(Func<ListWhenThen, ListWhenThen> caseWhenThenListFunc)
    {
        var builder = caseWhenThenListFunc(new ListWhenThen());
        _expression.CaseWhenThen = builder.Build();
        return this;
    }

    public SqlExpressionBuilder CaseElse(string caseElse)
    {
        _expression.CaseElse = caseElse;
        return this;
    }
    public SqlExpressionBuilder CoalesceValues(Func<ListStringBuilder, ListStringBuilder> valuesFunc)
    {
        var builder = valuesFunc(new ListStringBuilder());
        _expression.CoalesceValues = builder.Build();
        return this;
    }
    public SqlExpressionBuilder Default(string value)
    {
        _expression.Default = value;
        return this;
    }
    public SqlExpression Build() => _expression;
}

public class ListWhenThen
{
    private List<(string When, string Then)> _caseWhenThenList = new();

    public ListWhenThen caseWhenThen(string when, string then)
    {
        _caseWhenThenList.Add((when, then));
        return this;
    }

    public List<(string When, string Then)> Build() => _caseWhenThenList;
}