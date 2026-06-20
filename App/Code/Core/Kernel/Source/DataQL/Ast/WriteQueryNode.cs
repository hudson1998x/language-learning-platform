namespace LLE.Kernel.DataQL.Ast;

public class WriteQueryNode : AstNode
{
    public required string TableName { get; init; }
    
    public required object Payload { get; init; }
    
    public AstNode? Where { get; init; }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        => visitor.Visit(this);
}