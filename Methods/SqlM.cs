using _1.Methods.Builders;
using _1.Methods.Query;

namespace _1.Methods;


public static class SqlM
{
    public static SqlQueryBuilder Query() => new();
    public static SqlMutationBuilder Mutation() => new();
    public static SqlMergeBuilder Merge() => new();

    public static class Variable
    {
        public static SqlVariableDeclarationBuilder Declare() => new();
        public static SqlVariableAssignmentBuilder Assign() => new();
    }

    public static class Transactional
    {
        public static SqlTransactionBuilder Transaction() => new();
        public static SqlCursorBuilder Cursor() => new();
    }

    public static SqlDefinitionBuilder Definition() => new();
    public static SqlUtilityBuilder Utility() => new();
    public static SqlSetBuilder Set() => new();
    public static SqlRequestBuilder Request() => new();
}

