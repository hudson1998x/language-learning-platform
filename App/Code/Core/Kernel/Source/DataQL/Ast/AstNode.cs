using LLE.Kernel.DataQL.Enums;

namespace LLE.Kernel.DataQL.Ast;

public abstract class AstNode
{
    public OperationKind OperationKind { get; init; }

    public Type? EntityType { get; set; }

    public abstract TResult Accept<TResult>(IAstVisitor<TResult> visitor);
}