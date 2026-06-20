using LLE.Kernel.DataQL.Tokeniser;

namespace LLE.Kernel.DataQL.Ast;

public static class AstParser
{
    public static AstNode Parse(Type entityType, string query)
    {
        var tokens = TokenArrayBuilder.Parse(query);

        throw new NotImplementedException("AstParser not yet implemented.");
    }
}