namespace LLE.Kernel.DataQL.Ast;

public class LogicalNode : AstNode
{
    public required LogicalOperator Operator { get; init; }
    public required AstNode Left { get; init; }
    public required AstNode Right { get; init; }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        => visitor.Visit(this);
}

public enum LogicalOperator
{
    And,
    Or
}
