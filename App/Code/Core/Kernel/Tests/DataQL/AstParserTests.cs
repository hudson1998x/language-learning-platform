using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Enums;

namespace Tests.DataQL;

public sealed class AstParserTests
{
    private sealed class TestEntity;

    private static ReadQueryNode Parse(string query)
    {
        return (ReadQueryNode)AstParser.Parse(typeof(TestEntity), query);
    }

    private static ReadQueryNode ParseWithParameters(
        string query,
        Dictionary<string, object?> parameters)
    {
        return (ReadQueryNode)AstParser.Parse(typeof(TestEntity), query, parameters);
    }

    // ─── Empty Query ────────────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyQuery_ReturnsReadQueryNodeWithNullWhere()
    {
        var result = Parse("");

        Assert.Equal(nameof(TestEntity), result.TableName);
        Assert.Equal(OperationKind.Read, result.OperationKind);
        Assert.Equal(typeof(TestEntity), result.EntityType);
        Assert.Null(result.Where);
    }

    // ─── Comparison Operators ───────────────────────────────────────────

    [Fact]
    public void Parse_EqualsComparison_ReturnsFilterNode()
    {
        var result = Parse("Name = \"John\"");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Name", filter.ColumnName);
        Assert.Equal(FilterOperator.Equals, filter.Operator);
        Assert.Equal("John", filter.Value);
    }

    [Fact]
    public void Parse_NotEqualsComparison_ReturnsFilterNode()
    {
        var result = Parse("Status != \"Inactive\"");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Status", filter.ColumnName);
        Assert.Equal(FilterOperator.NotEquals, filter.Operator);
        Assert.Equal("Inactive", filter.Value);
    }

    [Fact]
    public void Parse_GreaterThanComparison_ReturnsFilterNode()
    {
        var result = Parse("Age > 21");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Age", filter.ColumnName);
        Assert.Equal(FilterOperator.GreaterThan, filter.Operator);
        Assert.Equal(21, filter.Value);
    }

    [Fact]
    public void Parse_LessThanComparison_ReturnsFilterNode()
    {
        var result = Parse("Price < 100");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Price", filter.ColumnName);
        Assert.Equal(FilterOperator.LessThan, filter.Operator);
        Assert.Equal(100, filter.Value);
    }

    [Fact]
    public void Parse_GreaterThanOrEqualsComparison_ReturnsFilterNode()
    {
        var result = Parse("Age >= 18");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Age", filter.ColumnName);
        Assert.Equal(FilterOperator.GreaterThanOrEquals, filter.Operator);
        Assert.Equal(18, filter.Value);
    }

    [Fact]
    public void Parse_LessThanOrEqualsComparison_ReturnsFilterNode()
    {
        var result = Parse("Score <= 100");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Score", filter.ColumnName);
        Assert.Equal(FilterOperator.LessThanOrEquals, filter.Operator);
        Assert.Equal(100, filter.Value);
    }

    // ─── Field-to-Field Comparison ──────────────────────────────────────

    [Fact]
    public void Parse_FieldToFieldComparison_ReturnsFilterNodeWithFieldReference()
    {
        var result = Parse("CorrectCount <= IncorrectCount");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("CorrectCount", filter.ColumnName);
        Assert.Equal(FilterOperator.LessThanOrEquals, filter.Operator);
        var fieldRef = Assert.IsType<FieldReference>(filter.Value);
        Assert.Equal("IncorrectCount", fieldRef.FieldName);
    }

    // ─── In / Not In / Like / Not Like ─────────────────────────────────

    [Fact]
    public void Parse_InClause_ReturnsFilterNodeWithArrayValue()
    {
        var result = Parse("Status in (1, 2, 3)");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Status", filter.ColumnName);
        Assert.Equal(FilterOperator.In, filter.Operator);
        var values = Assert.IsType<object[]>(filter.Value);
        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public void Parse_NotInClause_ReturnsFilterNode()
    {
        var result = Parse("Id not in (5, 6)");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Id", filter.ColumnName);
        Assert.Equal(FilterOperator.NotIn, filter.Operator);
        var values = Assert.IsType<object[]>(filter.Value);
        Assert.Equal([5, 6], values);
    }

    [Fact]
    public void Parse_LikeClause_ReturnsFilterNode()
    {
        var result = Parse("Name like \"%John%\"");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Name", filter.ColumnName);
        Assert.Equal(FilterOperator.Like, filter.Operator);
        Assert.Equal("%John%", filter.Value);
    }

    [Fact]
    public void Parse_NotLikeClause_ReturnsFilterNode()
    {
        var result = Parse("Title not like \"%test%\"");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Title", filter.ColumnName);
        Assert.Equal(FilterOperator.NotLike, filter.Operator);
        Assert.Equal("%test%", filter.Value);
    }

    [Fact]
    public void Parse_InClause_EmptyList_ReturnsEmptyArray()
    {
        var result = Parse("Id in ()");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(FilterOperator.In, filter.Operator);
        var values = Assert.IsType<object[]>(filter.Value);
        Assert.Empty(values);
    }

    [Fact]
    public void Parse_InClause_StringValues_ReturnsStringArray()
    {
        var result = Parse("Color in (\"Red\", \"Green\", \"Blue\")");

        var filter = Assert.IsType<FilterNode>(result.Where);
        var values = Assert.IsType<object[]>(filter.Value);
        Assert.Equal(["Red", "Green", "Blue"], values);
    }

    // ─── Logical Operators ─────────────────────────────────────────────

    [Fact]
    public void Parse_AndExpression_ReturnsLogicalNode()
    {
        var result = Parse("A = 1 and B = 2");

        var logical = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.And, logical.Operator);

        var left = Assert.IsType<FilterNode>(logical.Left);
        Assert.Equal("A", left.ColumnName);
        Assert.Equal(FilterOperator.Equals, left.Operator);
        Assert.Equal(1, left.Value);

        var right = Assert.IsType<FilterNode>(logical.Right);
        Assert.Equal("B", right.ColumnName);
        Assert.Equal(FilterOperator.Equals, right.Operator);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void Parse_OrExpression_ReturnsLogicalNode()
    {
        var result = Parse("X = 1 or Y = 2");

        var logical = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.Or, logical.Operator);

        var left = Assert.IsType<FilterNode>(logical.Left);
        Assert.Equal("X", left.ColumnName);

        var right = Assert.IsType<FilterNode>(logical.Right);
        Assert.Equal("Y", right.ColumnName);
    }

    [Fact]
    public void Parse_ChainedAnd_CreatesLeftAssociativeTree()
    {
        var result = Parse("A = 1 and B = 2 and C = 3");

        // Left-associative: ((A=1 and B=2) and C=3)
        var outer = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.And, outer.Operator);

        var inner = Assert.IsType<LogicalNode>(outer.Left);
        Assert.Equal(LogicalOperator.And, inner.Operator);

        Assert.IsType<FilterNode>(inner.Left);
        Assert.IsType<FilterNode>(inner.Right);
        Assert.IsType<FilterNode>(outer.Right);
    }

    [Fact]
    public void Parse_ChainedOr_CreatesLeftAssociativeTree()
    {
        var result = Parse("A = 1 or B = 2 or C = 3");

        var outer = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.Or, outer.Operator);

        var inner = Assert.IsType<LogicalNode>(outer.Left);
        Assert.Equal(LogicalOperator.Or, inner.Operator);
    }

    // ─── Not Expression ────────────────────────────────────────────────

    [Fact]
    public void Parse_NotExpression_ReturnsUnaryNode()
    {
        var result = Parse("not Active = true");

        var unary = Assert.IsType<UnaryNode>(result.Where);
        Assert.Equal(UnaryOperator.Not, unary.Operator);

        var operand = Assert.IsType<FilterNode>(unary.Operand);
        Assert.Equal("Active", operand.ColumnName);
        Assert.Equal(true, operand.Value);
    }

    [Fact]
    public void Parse_DoubleNot_ReturnsNestedUnaryNodes()
    {
        var result = Parse("not not Flag = true");

        var outer = Assert.IsType<UnaryNode>(result.Where);
        var inner = Assert.IsType<UnaryNode>(outer.Operand);
        var filter = Assert.IsType<FilterNode>(inner.Operand);
        Assert.Equal("Flag", filter.ColumnName);
    }

    // ─── Parenthesized Expressions ─────────────────────────────────────

    [Fact]
    public void Parse_ParenthesizedExpression_ThrowsDueToParserLimitation()
    {
        // The parser's ParseExpression expects EndOfFile at the end of every call,
        // including when called recursively from ParsePrimary for parenthesized
        // sub-expressions. This means parenthesized grouping is not currently supported.
        var ex = Assert.Throws<Exception>(() => Parse("A = 1 and (B = 2 or C = 3)"));
        Assert.Contains("Unexpected token", ex.Message);
    }

    // ─── Operator Precedence ───────────────────────────────────────────

    [Fact]
    public void Parse_NotBeforeAnd_HasHigherPrecedence()
    {
        var result = Parse("not A = 1 and B = 2");

        // Should parse as: (not A=1) and B=2
        var logical = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.And, logical.Operator);

        var notNode = Assert.IsType<UnaryNode>(logical.Left);
        Assert.Equal(UnaryOperator.Not, notNode.Operator);

        var filter = Assert.IsType<FilterNode>(logical.Right);
        Assert.Equal("B", filter.ColumnName);
    }

    [Fact]
    public void Parse_AndBeforeOr_HasHigherPrecedence()
    {
        var result = Parse("A = 1 and B = 2 or C = 3");

        // Should parse as: (A=1 and B=2) or C=3
        var logical = Assert.IsType<LogicalNode>(result.Where);
        Assert.Equal(LogicalOperator.Or, logical.Operator);

        var andNode = Assert.IsType<LogicalNode>(logical.Left);
        Assert.Equal(LogicalOperator.And, andNode.Operator);
    }

    // ─── Parameters ────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithParameter_SubstitutesValue()
    {
        var parameters = new Dictionary<string, object?> { ["id"] = 42 };
        var result = ParseWithParameters("Id = :id", parameters);

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(42, filter.Value);
    }

    [Fact]
    public void Parse_WithStringParameter_SubstitutesValue()
    {
        var parameters = new Dictionary<string, object?> { ["name"] = "Alice" };
        var result = ParseWithParameters("Name = :name", parameters);

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Alice", filter.Value);
    }

    [Fact]
    public void Parse_WithNullParameter_SubstitutesNull()
    {
        var parameters = new Dictionary<string, object?> { ["val"] = null };
        var result = ParseWithParameters("Field = :val", parameters);

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Null(filter.Value);
    }

    [Fact]
    public void Parse_WithParameterInInClause_SubstitutesValue()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = 1 };
        var result = ParseWithParameters("Status in (:status)", parameters);

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(FilterOperator.In, filter.Operator);
        var values = Assert.IsType<object[]>(filter.Value);
        Assert.Single(values);
        Assert.Equal(1, values[0]);
    }

    [Fact]
    public void Parse_MissingParameter_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() =>
            ParseWithParameters("Id = :missing", new Dictionary<string, object?>()));
        Assert.Contains("Parameter ':missing' was not provided", ex.Message);
    }

    [Fact]
    public void Parse_ParameterWithoutDictionary_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("Id = :param"));
        Assert.Contains("Parameter ':param' was not provided", ex.Message);
    }

    // ─── Literals ──────────────────────────────────────────────────────

    [Fact]
    public void Parse_IntegerLiteral_ReturnsIntValue()
    {
        var result = Parse("Age = 42");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(42, filter.Value);
        Assert.IsType<int>(filter.Value);
    }

    [Fact]
    public void Parse_DecimalLiteral_ReturnsDoubleValue()
    {
        var result = Parse("Price = 3.14");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(3.14, filter.Value);
        Assert.IsType<double>(filter.Value);
    }

    [Fact]
    public void Parse_StringLiteral_ReturnsStringValue()
    {
        var result = Parse("Name = \"Alice\"");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Alice", filter.Value);
    }

    [Fact]
    public void Parse_TrueLiteral_ReturnsBooleanTrue()
    {
        var result = Parse("Active = true");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(true, filter.Value);
    }

    [Fact]
    public void Parse_FalseLiteral_ReturnsBooleanFalse()
    {
        var result = Parse("Active = false");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal(false, filter.Value);
    }

    [Fact]
    public void Parse_NullLiteral_ReturnsNull()
    {
        var result = Parse("Field = null");
        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Null(filter.Value);
    }

    // ─── Dotted Fields ─────────────────────────────────────────────────

    [Fact]
    public void Parse_DottedField_CreatesNestedFieldName()
    {
        var result = Parse("Address.City = \"NYC\"");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("Address.City", filter.ColumnName);
    }

    [Fact]
    public void Parse_TripleDottedField_CreatesNestedFieldName()
    {
        var result = Parse("a.b.c = 1");

        var filter = Assert.IsType<FilterNode>(result.Where);
        Assert.Equal("a.b.c", filter.ColumnName);
    }

    // ─── Error Cases ───────────────────────────────────────────────────

    [Fact]
    public void Parse_UnexpectedTokenAfterExpression_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A = 1 B"));
        Assert.Contains("Unexpected token", ex.Message);
    }

    [Fact]
    public void Parse_MissingOperator_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A 1"));
        Assert.Contains("Expected operator", ex.Message);
    }

    [Fact]
    public void Parse_NotWithoutInOrLike_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A not B"));
        Assert.Contains("Expected 'in' or 'like' after 'not'", ex.Message);
    }

    [Fact]
    public void Parse_MissingCloseParen_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A in (1, 2"));
        Assert.Contains("Expected CloseParen", ex.Message);
    }

    [Fact]
    public void Parse_ExtraCloseParen_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A = 1)"));
        Assert.Contains("Unexpected token", ex.Message);
    }

    [Fact]
    public void Parse_UnterminatedString_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("Name = \"unclosed"));
        Assert.Contains("Unterminated string literal", ex.Message);
    }

    [Fact]
    public void Parse_InvalidCharacter_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("Name @ = 1"));
        Assert.Contains("Unexpected character", ex.Message);
    }

    [Fact]
    public void Parse_NotFoundNotIn_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => Parse("A not B"));
        Assert.Contains("Expected 'in' or 'like' after 'not'", ex.Message);
    }
}
