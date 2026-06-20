using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;

namespace LLE.SQLiteAdapter;

public class SQLiteAdapter : IDatabaseAdapter
{
    public Task<object> ExecuteQuery(AstNode node)
    {
        throw new NotImplementedException();
    }
}