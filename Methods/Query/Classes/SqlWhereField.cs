using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public abstract class SqlWhereFieldBase { }

public class SqlWhereCondition : SqlWhereFieldBase,
    IHasField, IHasValue, IHasValues, IHasComparison, IHasNegate, IHasExpression
{
    public string? Field { get; set; }
    public object? Value { get; set; }
    public List<object> Values { get; set; } = new();
    public SqlComparisonType? Comparison { get; set; }
    public bool? Negate { get; set; }
    public SqlExpression? Expression { get; set; }
}

public class SqlWhereGroup : SqlWhereFieldBase,
    IHasOperator, IHasNegate
{
    public SqlConditionOperator? Operator { get; set; } // AND / OR
    public List<SqlWhereFieldBase> Conditions { get; set; } = new();
    public bool? Negate { get; set; } // NOT (...)
}

