using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public record SqlExpression : IBaseExpression, ICaseExpression, ICoalesceExpression, IDefaultExpression
{
    public SqlExpressionType? Type { get; set; }
    public List<(string When, string Then)>? CaseWhenThen { get; set; }
    public string? CaseElse { get; set; }
    public List<string>? CoalesceValues { get; set; }
    public string? Default { get; set; }
}
