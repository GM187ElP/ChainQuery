using _1.Methods.Query.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1.Methods.Query.Classes;

public record SqlOrderBy : IHasField, IHasDescending
{
    public bool Descending { get; set; } = false;
    public string? Field { get; set; }
}
