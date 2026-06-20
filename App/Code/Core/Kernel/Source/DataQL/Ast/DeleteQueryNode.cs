namespace LLE.Kernel.DataQL.Ast;

public class DeleteQueryNode : AstNode
{
    public required string TableName { get; init; }
        
    public AstNode? Where;
}