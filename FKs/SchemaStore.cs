using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _1.Schema;

namespace _1.FKs;

public static class SchemaStore
{
    public static SchemaBuilder Instance { get; private set; }

    public static void Initialize(Dictionary<string, (string tableName, Dictionary<(string foreignKey, string foreignKeytableName), string>? foreignKeys)> fks)
    {
        if (Instance == null)
            Instance = new SchemaBuilder(fks);
    }
}
