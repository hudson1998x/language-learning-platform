using LLE.Kernel.DataQL.Ast;

namespace LLE.Kernel.Contracts;

public interface IDatabaseAdapter
{
    public Task<object> ExecuteQuery(AstNode node);
}