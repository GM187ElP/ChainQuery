using _1.Methods.Query;
using System.Reflection.Metadata.Ecma335;

namespace _1.Methods.Builders;

public class SqlRequestBuilder
{
    private SqlQuery? _query;
    private SqlMutation? _mutation;
    private SqlMerge? _merge;
    private SqlTransaction? _transaction;
    private SqlDefinition? _definition;
    private SqlUtility? _utility;
    private SqlSet? _set;
    private SqlVariableDeclaration? _variableDeclaration;
    private SqlVariableAssignment? _variableAssignment;
    private SqlCursor? _cursor;

    public SqlRequestBuilder SetQuery(SqlQuery query) { _query = query; return this; }
    public SqlRequestBuilder SetMutation(SqlMutation mutation) { _mutation = mutation; return this; }
    public SqlRequestBuilder SetMerge(SqlMerge merge) { _merge = merge; return this; }
    public SqlRequestBuilder SetTransaction(SqlTransaction transaction) { _transaction = transaction; return this; }
    public SqlRequestBuilder SetDefinition(SqlDefinition definition) { _definition = definition; return this; }
    public SqlRequestBuilder SetUtility(SqlUtility utility) { _utility = utility; return this; }
    public SqlRequestBuilder SetSet(SqlSet set) { _set = set; return this; }
    public SqlRequestBuilder SetVariableDeclaration(SqlVariableDeclaration declaration) { _variableDeclaration = declaration; return this; }
    public SqlRequestBuilder SetVariableAssignment(SqlVariableAssignment assignment) { _variableAssignment = assignment; return this; }
    public SqlRequestBuilder SetCursor(SqlCursor cursor) { _cursor = cursor; return this; }

    public SqlRequest Build()
    {
        return new SqlRequest
        {
            Query = _query,
            Mutation = _mutation,
            Merge = _merge,
            Transaction = _transaction,
            Definition = _definition,
            Utility = _utility,
            Set = _set,
            VariableDeclaration = _variableDeclaration,
            VariableAssignment = _variableAssignment,
            Cursor = _cursor
        };
    }
}
