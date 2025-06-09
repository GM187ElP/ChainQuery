using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public class SqlFromField
{
    public string? Table { get; set; }
    public string? Alias { get; set; }
    public SqlQuery? SubQuery { get; set; } // for derived tables
}
