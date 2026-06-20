namespace LLE.Kernel.DataQL.Ast;

public class FilterNode : AstNode
{
    public required string ColumnName { get; init; }
    
    public FilterOperator Operator { get; init; }
    
    public required object Value { get; init; }
}

public enum FilterOperator
{
    Equals,
    NotEquals,
    LessThan,
    LessThanOrEquals,
    GreaterThan,
    GreaterThanOrEquals,
    In,
    NotIn,
    Like,
    NotLike
} 