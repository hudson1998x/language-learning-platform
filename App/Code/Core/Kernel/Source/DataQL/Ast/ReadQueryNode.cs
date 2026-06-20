using LLE.Kernel.DataQL.Enums;

namespace LLE.Kernel.DataQL.Ast
{
    public class ReadQueryNode : AstNode
    {
        public required string TableName { get; init; }
        
        public AstNode? Where;

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
            => visitor.Visit(this);
    }
}