namespace LLE.Kernel.DataQL.Ast;

public class UnaryNode : AstNode
{
    public required UnaryOperator Operator { get; init; }
    public required AstNode Operand { get; init; }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        => visitor.Visit(this);
}

public enum UnaryOperator
{
    Not
}
