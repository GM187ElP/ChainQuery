using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlMutationBuilder
{
    private SqlMutation _mutation = new();
    public SqlMutationBuilder Into(string table) { _mutation = _mutation with { Table = table }; return this; }
    public SqlMutationBuilder WithValues(Dictionary<string, object> values) { _mutation = _mutation with { Values = values }; return this; }
    public SqlMutation Build() => _mutation;
}
