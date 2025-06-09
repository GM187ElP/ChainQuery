using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlVariableDeclarationBuilder
{
    private SqlVariableDeclaration _declaration = new();
    public SqlVariableDeclarationBuilder Name(string name) { _declaration = _declaration with { Name = name }; return this; }
    public SqlVariableDeclarationBuilder Type(string type) { _declaration = _declaration with { DataType = type }; return this; }
    public SqlVariableDeclarationBuilder Default(object? defaultValue) { _declaration = _declaration with { DefaultValue = defaultValue }; return this; }
    public SqlVariableDeclaration Build() => _declaration;
}
