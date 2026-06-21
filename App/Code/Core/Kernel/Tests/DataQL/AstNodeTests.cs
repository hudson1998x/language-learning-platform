using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Enums;

namespace Tests.DataQL;

public sealed class AstNodeTests
{
    // ─── FilterNode ───────────────────────────────────────────────────

    [Fact]
    public void FilterNode_ConstructedWithProperties_PropertiesMatch()
    {
        var node = new FilterNode
        {
            ColumnName = "Age",
            Operator = FilterOperator.GreaterThan,
            Value = 21
        };

        Assert.Equal("Age", node.ColumnName);
        Assert.Equal(FilterOperator.GreaterThan, node.Operator);
        Assert.Equal(21, node.Value);
        Assert.Equal(OperationKind.Read, node.OperationKind); // default
    }

    [Fact]
    public void FilterNode_AllOperators_AreDefined()
    {
        Assert.Equal(0, (int)FilterOperator.Equals);
        Assert.Equal(1, (int)FilterOperator.NotEquals);
        Assert.Equal(2, (int)FilterOperator.LessThan);
        Assert.Equal(3, (int)FilterOperator.LessThanOrEquals);
        Assert.Equal(4, (int)FilterOperator.GreaterThan);
        Assert.Equal(5, (int)FilterOperator.GreaterThanOrEquals);
        Assert.Equal(6, (int)FilterOperator.In);
        Assert.Equal(7, (int)FilterOperator.NotIn);
        Assert.Equal(8, (int)FilterOperator.Like);
        Assert.Equal(9, (int)FilterOperator.NotLike);
    }

    [Fact]
    public void FilterNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new FilterNode
        {
            ColumnName = "X",
            Operator = FilterOperator.Equals,
            Value = 1
        };

        var result = node.Accept(new TestVisitor());
        Assert.Equal("FilterNode", result);
    }

    // ─── LogicalNode ──────────────────────────────────────────────────

    [Fact]
    public void LogicalNode_AndOperator_CreatesCorrectNode()
    {
        var left = new FilterNode { ColumnName = "A", Operator = FilterOperator.Equals, Value = 1 };
        var right = new FilterNode { ColumnName = "B", Operator = FilterOperator.Equals, Value = 2 };

        var node = new LogicalNode
        {
            Operator = LogicalOperator.And,
            Left = left,
            Right = right
        };

        Assert.Equal(LogicalOperator.And, node.Operator);
        Assert.Same(left, node.Left);
        Assert.Same(right, node.Right);
    }

    [Fact]
    public void LogicalNode_OrOperator_CreatesCorrectNode()
    {
        var node = new LogicalNode
        {
            Operator = LogicalOperator.Or,
            Left = new FilterNode { ColumnName = "X", Operator = FilterOperator.Equals, Value = 1 },
            Right = new FilterNode { ColumnName = "Y", Operator = FilterOperator.Equals, Value = 2 }
        };

        Assert.Equal(LogicalOperator.Or, node.Operator);
    }

    [Fact]
    public void LogicalNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new LogicalNode
        {
            Operator = LogicalOperator.And,
            Left = new FilterNode { ColumnName = "A", Operator = FilterOperator.Equals, Value = 1 },
            Right = new FilterNode { ColumnName = "B", Operator = FilterOperator.Equals, Value = 2 }
        };

        var result = node.Accept(new TestVisitor());
        Assert.Equal("LogicalNode", result);
    }

    // ─── UnaryNode ─────────────────────────────────────────────────────

    [Fact]
    public void UnaryNode_NotOperator_CreatesCorrectNode()
    {
        var operand = new FilterNode
        {
            ColumnName = "Active",
            Operator = FilterOperator.Equals,
            Value = true
        };

        var node = new UnaryNode
        {
            Operator = UnaryOperator.Not,
            Operand = operand
        };

        Assert.Equal(UnaryOperator.Not, node.Operator);
        Assert.Same(operand, node.Operand);
    }

    [Fact]
    public void UnaryNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new UnaryNode
        {
            Operator = UnaryOperator.Not,
            Operand = new FilterNode { ColumnName = "X", Operator = FilterOperator.Equals, Value = 1 }
        };

        var result = node.Accept(new TestVisitor());
        Assert.Equal("UnaryNode", result);
    }

    // ─── ReadQueryNode ─────────────────────────────────────────────────

    [Fact]
    public void ReadQueryNode_DefaultProperties_AreSetCorrectly()
    {
        var node = new ReadQueryNode
        {
            TableName = "Users",
            EntityType = typeof(string)
        };

        Assert.Equal("Users", node.TableName);
        Assert.Equal(typeof(string), node.EntityType);
        Assert.Equal(OperationKind.Read, node.OperationKind);
        Assert.Null(node.Where);
        Assert.Null(node.OrderBy);
        Assert.Null(node.Pagination);
        Assert.False(node.IsCount);
    }

    [Fact]
    public void ReadQueryNode_WithWhere_StoresFilter()
    {
        var filter = new FilterNode
        {
            ColumnName = "Id",
            Operator = FilterOperator.Equals,
            Value = 1
        };

        var node = new ReadQueryNode
        {
            TableName = "Items",
            Where = filter
        };

        Assert.Same(filter, node.Where);
    }

    [Fact]
    public void ReadQueryNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new ReadQueryNode { TableName = "T" };
        var result = node.Accept(new TestVisitor());
        Assert.Equal("ReadQueryNode", result);
    }

    // ─── WriteQueryNode ────────────────────────────────────────────────

    [Fact]
    public void WriteQueryNode_ConstructedWithProperties_PropertiesMatch()
    {
        var payload = new { Name = "Alice" };

        var node = new WriteQueryNode
        {
            TableName = "Users",
            Payload = payload,
            Where = null
        };

        Assert.Equal("Users", node.TableName);
        Assert.Same(payload, node.Payload);
        Assert.Null(node.Where);
        Assert.Equal(OperationKind.Read, node.OperationKind); // default
    }

    [Fact]
    public void WriteQueryNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new WriteQueryNode
        {
            TableName = "T",
            Payload = new { }
        };

        var result = node.Accept(new TestVisitor());
        Assert.Equal("WriteQueryNode", result);
    }

    // ─── DeleteQueryNode ───────────────────────────────────────────────

    [Fact]
    public void DeleteQueryNode_ConstructedWithProperties_PropertiesMatch()
    {
        var payload = new { Id = 1 };

        var node = new DeleteQueryNode
        {
            TableName = "Users",
            Payload = payload
        };

        Assert.Equal("Users", node.TableName);
        Assert.Same(payload, node.Payload);
        Assert.Null(node.Where);
        Assert.Equal(OperationKind.Read, node.OperationKind); // default
    }

    [Fact]
    public void DeleteQueryNode_AcceptsVisitor_ReturnsResult()
    {
        var node = new DeleteQueryNode
        {
            TableName = "T",
            Payload = new { }
        };

        var result = node.Accept(new TestVisitor());
        Assert.Equal("DeleteQueryNode", result);
    }

    // ─── Pagination ────────────────────────────────────────────────────

    [Fact]
    public void Pagination_DefaultValues_AreSetCorrectly()
    {
        var pagination = new Pagination();

        Assert.Equal(1, pagination.PageNo);
        Assert.Equal(10, pagination.Limit);
    }

    [Fact]
    public void Pagination_CanSetProperties()
    {
        var pagination = new Pagination
        {
            PageNo = 3,
            Limit = 25
        };

        Assert.Equal(3, pagination.PageNo);
        Assert.Equal(25, pagination.Limit);
    }

    // ─── SortOption ────────────────────────────────────────────────────

    [Fact]
    public void SortOption_DefaultValues_AreSetCorrectly()
    {
        var sort = new SortOption();

        Assert.Equal(string.Empty, sort.Field);
        Assert.True(sort.Ascending);
    }

    [Fact]
    public void SortOption_CanSetProperties()
    {
        var sort = new SortOption
        {
            Field = "Name",
            Ascending = false
        };

        Assert.Equal("Name", sort.Field);
        Assert.False(sort.Ascending);
    }

    // ─── OperationKind ─────────────────────────────────────────────────

    [Fact]
    public void OperationKind_Values_AreCorrect()
    {
        Assert.Equal(0, (int)OperationKind.Read);
        Assert.Equal(1, (int)OperationKind.Write);
        Assert.Equal(2, (int)OperationKind.Delete);
    }

    // ─── Visitor ───────────────────────────────────────────────────────

    private sealed class TestVisitor : IAstVisitor<string>
    {
        public string Visit(ReadQueryNode node) => "ReadQueryNode";
        public string Visit(WriteQueryNode node) => "WriteQueryNode";
        public string Visit(DeleteQueryNode node) => "DeleteQueryNode";
        public string Visit(FilterNode node) => "FilterNode";
        public string Visit(LogicalNode node) => "LogicalNode";
        public string Visit(UnaryNode node) => "UnaryNode";
    }
}
