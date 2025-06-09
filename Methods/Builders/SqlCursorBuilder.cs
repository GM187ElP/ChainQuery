using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlCursorBuilder
{
    private SqlCursor _cursor = new();
    public SqlCursorBuilder Name(string name) { _cursor = _cursor with { Name = name }; return this; }
    public SqlCursorBuilder For(string query) { _cursor = _cursor with { ForQuery = query }; return this; }
    //public SqlCursorBuilder Operation(CursorOperation? op) { _cursor = _cursor with { Operation = op }; return this; }
    public SqlCursor Build() => _cursor;
}
