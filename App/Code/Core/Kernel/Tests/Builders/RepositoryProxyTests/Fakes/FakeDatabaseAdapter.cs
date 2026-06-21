using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;

namespace Tests.Builders.RepositoryProxyTests.Fakes;

public sealed class FakeDatabaseAdapter : IDatabaseAdapter
{
    public List<AstNode> ReceivedNodes { get; } = [];
    public Func<AstNode, Task<object>>? OnExecuteQuery { get; set; }

    public Task<object> ExecuteQuery(AstNode node)
    {
        ReceivedNodes.Add(node);
        return OnExecuteQuery?.Invoke(node) ?? Task.FromResult<object>(null!);
    }
}
