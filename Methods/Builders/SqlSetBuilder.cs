using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlSetBuilder
{
    private SqlSet _set = new();
    public SqlSetBuilder Variable(string variable) { _set = _set with { Variable = variable }; return this; }
    public SqlSetBuilder Value(string value) { _set = _set with { Value = value }; return this; }
    public SqlSet Build() => _set;
}
