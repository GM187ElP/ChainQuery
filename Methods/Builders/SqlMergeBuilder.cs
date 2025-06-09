using _1.Methods.Query;

namespace _1.Methods.Builders;

public class SqlMergeBuilder
{
    private SqlMerge _merge = new();
    public SqlMergeBuilder Target(string target) { _merge = _merge with { TargetTable = target }; return this; }
    public SqlMergeBuilder Source(string source) { _merge = _merge with { SourceTable = source }; return this; }
    public SqlMergeBuilder On(SqlCondition condition) { _merge = _merge with { OnCondition = condition }; return this; }
    public SqlMergeBuilder Insert(Dictionary<string, object> values) { _merge = _merge with { InsertValues = values }; return this; }
    public SqlMergeBuilder Update(Dictionary<string, object> values) { _merge = _merge with { UpdateValues = values }; return this; }
    public SqlMergeBuilder DeleteUnmatched(bool delete) { _merge = _merge with { DeleteWhenNotMatched = delete }; return this; }
    public SqlMerge Build() => _merge;
}
