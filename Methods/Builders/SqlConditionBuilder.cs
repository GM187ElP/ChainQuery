using _1.Methods.Query;

namespace _1.Methods.Builders;

//public class SqlConditionBuilder
//{
//    private SqlCondition _sqlCondition = new();

//    public SqlConditionBuilder Field(string field)
//    {
//        _sqlCondition.Field = field;
//        return this;
//    }

//    public SqlConditionBuilder Compare(SqlComparisonType comparison, object? value)
//    {
//        _sqlCondition.Comparison = comparison;
//        _sqlCondition.Value = value;
//        return this;
//    }

//    public SqlConditionBuilder IsEqualTo(object? value) => Compare(SqlComparisonType.Equal, value);
//    public SqlConditionBuilder IsNotEqualTo(object? value) => Compare(SqlComparisonType.NotEqual, value);
//    public SqlConditionBuilder IsGreaterThan(object? value) => Compare(SqlComparisonType.GreaterThan, value);
//    public SqlConditionBuilder IsLessThan(object? value) => Compare(SqlComparisonType.LessThan, value);
//    public SqlConditionBuilder IsIn(params object[] values)
//    {
//        _sqlCondition.Comparison = SqlComparisonType.In;
//        _sqlCondition.Values = values.ToList();
//        return this;
//    }

//    public SqlConditionBuilder IsNull()
//    {
//        _sqlCondition.Comparison = SqlComparisonType.IsNull;
//        return this;
//    }

//    public SqlConditionBuilder IsNotNull()
//    {
//        _sqlCondition.Comparison = SqlComparisonType.IsNotNull;
//        return this;
//    }

//    public SqlConditionBuilder Not()
//    {
//        _sqlCondition.Negate = true;
//        return this;
//    }

//    public SqlConditionBuilder Group(SqlConditionOperator op, params SqlCondition[] conditions)
//    {
//        _sqlCondition.Operator = op;
//        _sqlCondition.Conditions = conditions.ToList();
//        return this;
//    }

//    public SqlCondition Build() => _sqlCondition;

//    // AND grouping: x => x.And(x => ..., x => ...)
//    public SqlConditionBuilder And(params Func<SqlConditionBuilder, SqlConditionBuilder>[] builders)
//    {
//        var group = new SqlCondition
//        {
//            Operator = SqlConditionOperator.And,
//            Conditions = builders.Select(b => b(new SqlConditionBuilder()).Build()).ToList()
//        };
//        _sqlCondition = group;
//        return this;
//    }

//    // OR grouping: x => x.Or(x => ..., x => ...)
//    public SqlConditionBuilder Or(params Func<SqlConditionBuilder, SqlConditionBuilder>[] builders)
//    {
//        var group = new SqlCondition
//        {
//            Operator = SqlConditionOperator.Or,
//            Conditions = builders.Select(b => b(new SqlConditionBuilder()).Build()).ToList()
//        };
//        _sqlCondition = group;
//        return this;
//    }

//    // LIKE
//    public SqlConditionBuilder Like(string pattern)
//    {
//        _sqlCondition.Operator = SqlConditionOperator.Like;
//        _sqlCondition.Value = pattern;
//        return this;
//    }

//    // BETWEEN
//    public SqlConditionBuilder Between(object start, object end)
//    {
//        _sqlCondition.Operator = SqlConditionOperator.Between;
//        _sqlCondition.Values = new List<object> { start, end };
//        return this;
//    }

//    // EXISTS (used for subquery conditions)
//    public SqlConditionBuilder Exists(SqlQuery query)
//    {
//        _sqlCondition.Operator = SqlConditionOperator.Exists;
//        _sqlCondition.Value = query;
//        return this;
//    }

//    // Raw expression (for edge cases)
//    public SqlConditionBuilder RawExpression(string expression)
//    {
//        _sqlCondition.Expression = new SqlExpression
//        {
//            Type = null,
//            Default = expression
//        };
//        return this;
//    }


//}
