using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlUtilityBuilder
{
    private SqlUtility _utility = new();
    //public SqlUtilityBuilder Type(SqlUtilityType type) { _utility = _utility with { Type = type }; return this; }
    public SqlUtilityBuilder Procedure(string name, List<object> parameters) { _utility = _utility with { ProcedureName = name, Parameters = parameters }; return this; }
    public SqlUtilityBuilder Command(string text) { _utility = _utility with { CommandText = text }; return this; }
    public SqlUtilityBuilder Wait(WaitForOptions options) { _utility = _utility with { WaitForOptions = options }; return this; }
    public SqlUtility Build() => _utility;
}
