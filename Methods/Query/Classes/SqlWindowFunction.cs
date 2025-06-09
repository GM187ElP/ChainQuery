using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public class SqlWindowFunction :
    IHasWindowFunction,
    IHasPartitionBy,
    IHasOrderBy
{
    public SqlWindowFunctionType Function { get; set; }
    public List<string>? PartitionBy { get; set; }
    public List<SqlOrderBy>? OrderBy { get; set; }
}