using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public record SqlCondition : IHasOperator,  IHasValues, IHasField, IHasComparison, IHasValue, IHasNegate, IHasExpression
{
    public SqlConditionOperator? Operator { get; set; }
    public List<SqlCondition> Conditions { get; set; } = new();
    public List<object> Values { get; set; } = new();  // Use object to support any value types
    public string? Field { get; set; }
    public SqlComparisonType? Comparison { get; set; }
    public object? Value { get; set; }
    public bool? Negate { get; set; }
    public SqlExpression? Expression { get; set; }
}