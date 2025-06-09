using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public class SqlSelectField : IHasAlias, IHasAggregate, IHasExpression, IHasField, IHasWindow
{
    public string? Field { get; set; }              // e.g., "Name", "Age"
    public string? Alias { get; set; }              // e.g., "EmployeeName"
    public SqlAggregateFunction? AggregateFunction { get; set; } // e.g., SUM, COUNT
    public SqlExpression? Expression { get; set; } // custom expressions
    public SqlWindowFunction? Window { get; set; } // window functions like ROW_NUMBER()
}
