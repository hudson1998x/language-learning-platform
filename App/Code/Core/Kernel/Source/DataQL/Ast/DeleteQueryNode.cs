namespace LLE.Kernel.DataQL.Ast;

public class DeleteQueryNode : AstNode
{
    public required string TableName { get; init; }
        
    public required object Payload { get; init; }

    public AstNode? Where;

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        => visitor.Visit(this);
}