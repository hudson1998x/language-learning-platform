namespace LLE.Kernel.DataQL.Ast;

public interface IAstVisitor<out TResult>
{
    TResult Visit(ReadQueryNode node);
    TResult Visit(WriteQueryNode node);
    TResult Visit(DeleteQueryNode node);
    TResult Visit(FilterNode node);
    TResult Visit(LogicalNode node);
    TResult Visit(UnaryNode node);
}
