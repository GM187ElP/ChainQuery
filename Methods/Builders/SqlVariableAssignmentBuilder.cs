using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlVariableAssignmentBuilder
{
    private SqlVariableAssignment _assignment = new();
    public SqlVariableAssignmentBuilder Name(string name) { _assignment = _assignment with { Name = name }; return this; }
    public SqlVariableAssignmentBuilder Value(object? value) { _assignment = _assignment with { Value = value }; return this; }
    public SqlVariableAssignment Build() => _assignment;
}
