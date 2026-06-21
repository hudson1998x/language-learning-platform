using LLE.Kernel.DataQL.Tokeniser;

namespace Tests.DataQL;

public sealed class TokenArrayBuilderTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEndOfFileOnly()
    {
        var tokens = TokenArrayBuilder.Parse("");

        Assert.Single(tokens);
        Assert.Equal(TokenKind.EndOfFile, tokens[0].Kind);
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEndOfFile()
    {
        var tokens = TokenArrayBuilder.Parse("   \t\n  ");

        Assert.Single(tokens);
        Assert.Equal(TokenKind.EndOfFile, tokens[0].Kind);
    }

    [Fact]
    public void Parse_Identifier_ReturnsIdentifierToken()
    {
        var tokens = TokenArrayBuilder.Parse("UserId");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal(0, tokens[0].StartPosition);
        Assert.Equal(6, tokens[0].Length);
    }

    [Fact]
    public void Parse_KeywordAnd_ReturnsAndToken()
    {
        var tokens = TokenArrayBuilder.Parse("and");

        Assert.Equal(TokenKind.And, tokens[0].Kind);
    }

    [Fact]
    public void Parse_KeywordOr_ReturnsOrToken()
    {
        var tokens = TokenArrayBuilder.Parse("or");

        Assert.Equal(TokenKind.Or, tokens[0].Kind);
    }

    [Fact]
    public void Parse_KeywordNot_ReturnsNotToken()
    {
        var tokens = TokenArrayBuilder.Parse("not");

        Assert.Equal(TokenKind.Not, tokens[0].Kind);
    }

    [Fact]
    public void Parse_KeywordIn_ReturnsInToken()
    {
        var tokens = TokenArrayBuilder.Parse("in");

        Assert.Equal(TokenKind.In, tokens[0].Kind);
    }

    [Fact]
    public void Parse_KeywordLike_ReturnsLikeToken()
    {
        var tokens = TokenArrayBuilder.Parse("like");

        Assert.Equal(TokenKind.Like, tokens[0].Kind);
    }

    [Fact]
    public void Parse_KeywordsAreCaseInsensitive_UsesOrdinalIgnoreCase()
    {
        var tokens = TokenArrayBuilder.Parse("AND OR NOT IN LIKE");

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenKind.And, tokens[0].Kind);
        Assert.Equal(TokenKind.Or, tokens[1].Kind);
        Assert.Equal(TokenKind.Not, tokens[2].Kind);
        Assert.Equal(TokenKind.In, tokens[3].Kind);
        Assert.Equal(TokenKind.Like, tokens[4].Kind);
    }

    [Fact]
    public void Parse_NumericLiteral_ReturnsLiteralToken()
    {
        var tokens = TokenArrayBuilder.Parse("42");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.Literal, tokens[0].Kind);
        Assert.Equal(0, tokens[0].StartPosition);
        Assert.Equal(2, tokens[0].Length);
    }

    [Fact]
    public void Parse_DecimalLiteral_ReturnsLiteralToken()
    {
        var tokens = TokenArrayBuilder.Parse("3.14");

        Assert.Equal(TokenKind.Literal, tokens[0].Kind);
        Assert.Equal(4, tokens[0].Length);
    }

    [Fact]
    public void Parse_StringLiteral_ReturnsLiteralToken()
    {
        var tokens = TokenArrayBuilder.Parse("\"John Doe\"");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.Literal, tokens[0].Kind);
        Assert.Equal(1, tokens[0].StartPosition); // after opening quote
        Assert.Equal(8, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorEquals_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse("=");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(1, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse("!=");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(2, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse(">");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(1, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEquals_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse(">=");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(2, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse("<");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(1, tokens[0].Length);
    }

    [Fact]
    public void Parse_OperatorLessThanOrEquals_ReturnsOperatorToken()
    {
        var tokens = TokenArrayBuilder.Parse("<=");

        Assert.Equal(TokenKind.Operator, tokens[0].Kind);
        Assert.Equal(2, tokens[0].Length);
    }

    [Fact]
    public void Parse_Dot_ReturnsDotToken()
    {
        var tokens = TokenArrayBuilder.Parse(".");

        Assert.Equal(TokenKind.Dot, tokens[0].Kind);
    }

    [Fact]
    public void Parse_OpenParen_ReturnsOpenParenToken()
    {
        var tokens = TokenArrayBuilder.Parse("(");

        Assert.Equal(TokenKind.OpenParen, tokens[0].Kind);
    }

    [Fact]
    public void Parse_CloseParen_ReturnsCloseParenToken()
    {
        var tokens = TokenArrayBuilder.Parse(")");

        Assert.Equal(TokenKind.CloseParen, tokens[0].Kind);
    }

    [Fact]
    public void Parse_Comma_ReturnsCommaToken()
    {
        var tokens = TokenArrayBuilder.Parse(",");

        Assert.Equal(TokenKind.Comma, tokens[0].Kind);
    }

    [Fact]
    public void Parse_Parameter_ReturnsParameterToken()
    {
        var tokens = TokenArrayBuilder.Parse(":userId");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.Parameter, tokens[0].Kind);
        Assert.Equal(1, tokens[0].StartPosition); // after colon
        Assert.Equal(6, tokens[0].Length);
    }

    [Fact]
    public void Parse_ParameterWithUnderscore_ReturnsParameterToken()
    {
        var tokens = TokenArrayBuilder.Parse(":my_param");

        Assert.Equal(TokenKind.Parameter, tokens[0].Kind);
        Assert.Equal(8, tokens[0].Length);
    }

    [Fact]
    public void Parse_ComplexExpression_TokenizesCorrectly()
    {
        var tokens = TokenArrayBuilder.Parse("Age > 21 and (Status = \"Active\" or Status = \"Pending\")");

        Assert.Equal(14, tokens.Count);

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);  // Age
        Assert.Equal(TokenKind.Operator, tokens[1].Kind);    // >
        Assert.Equal(TokenKind.Literal, tokens[2].Kind);     // 21
        Assert.Equal(TokenKind.And, tokens[3].Kind);         // and
        Assert.Equal(TokenKind.OpenParen, tokens[4].Kind);   // (
        Assert.Equal(TokenKind.Identifier, tokens[5].Kind);  // Status
        Assert.Equal(TokenKind.Operator, tokens[6].Kind);    // =
        Assert.Equal(TokenKind.Literal, tokens[7].Kind);     // "Active"
        Assert.Equal(TokenKind.Or, tokens[8].Kind);          // or
        Assert.Equal(TokenKind.Identifier, tokens[9].Kind);  // Status
        Assert.Equal(TokenKind.Operator, tokens[10].Kind);   // =
        Assert.Equal(TokenKind.Literal, tokens[11].Kind);    // "Pending"
        Assert.Equal(TokenKind.CloseParen, tokens[12].Kind); // )
        Assert.Equal(TokenKind.EndOfFile, tokens[13].Kind);
    }

    [Fact]
    public void Parse_InClause_TokenizesCorrectly()
    {
        var tokens = TokenArrayBuilder.Parse("Status in (1, 2, 3)");

        Assert.Equal(10, tokens.Count);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);    // Status
        Assert.Equal(TokenKind.In, tokens[1].Kind);            // in
        Assert.Equal(TokenKind.OpenParen, tokens[2].Kind);     // (
        Assert.Equal(TokenKind.Literal, tokens[3].Kind);       // 1
        Assert.Equal(TokenKind.Comma, tokens[4].Kind);         // ,
        Assert.Equal(TokenKind.Literal, tokens[5].Kind);       // 2
        Assert.Equal(TokenKind.Comma, tokens[6].Kind);         // ,
        Assert.Equal(TokenKind.Literal, tokens[7].Kind);       // 3
        Assert.Equal(TokenKind.CloseParen, tokens[8].Kind);    // )
    }

    [Fact]
    public void Parse_DottedField_TokenizesCorrectly()
    {
        var tokens = TokenArrayBuilder.Parse("Address.City = \"NYC\"");

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);  // Address
        Assert.Equal(TokenKind.Dot, tokens[1].Kind);         // .
        Assert.Equal(TokenKind.Identifier, tokens[2].Kind);  // City
        Assert.Equal(TokenKind.Operator, tokens[3].Kind);    // =
        Assert.Equal(TokenKind.Literal, tokens[4].Kind);     // "NYC"
    }

    [Fact]
    public void Parse_UnterminatedString_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => TokenArrayBuilder.Parse("\"unterminated"));
        Assert.Contains("Unterminated string literal", ex.Message);
    }

    [Fact]
    public void Parse_ExclamationMarkWithoutEquals_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => TokenArrayBuilder.Parse("!"));
        Assert.Contains("Unexpected character '!'", ex.Message);
    }

    [Fact]
    public void Parse_InvalidCharacter_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => TokenArrayBuilder.Parse("@"));
        Assert.Contains("Unexpected character '@'", ex.Message);
    }

    [Fact]
    public void Parse_IdentifierWithUnderscore_ReturnsIdentifierToken()
    {
        var tokens = TokenArrayBuilder.Parse("_myField");

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal(8, tokens[0].Length);
    }

    [Fact]
    public void Parse_IdentifierStartingWithUnderscoreContainsDigits_ReturnsIdentifier()
    {
        var tokens = TokenArrayBuilder.Parse("_field_1");

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal(8, tokens[0].Length);
    }

    [Fact]
    public void Parse_LoneExclamation_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => TokenArrayBuilder.Parse("x ! y"));
        Assert.Contains("Unexpected character '!'", ex.Message);
    }

    [Fact]
    public void Parse_NotKeywordThenIn_TokenizesSeparately()
    {
        var tokens = TokenArrayBuilder.Parse("not in");

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenKind.Not, tokens[0].Kind);
        Assert.Equal(TokenKind.In, tokens[1].Kind);
    }

    [Fact]
    public void Parse_StringWithSpecialCharacters_ReturnsLiteral()
    {
        var tokens = TokenArrayBuilder.Parse("\"hello_world-123\"");

        Assert.Equal(TokenKind.Literal, tokens[0].Kind);
        Assert.Equal(15, tokens[0].Length);
    }
}
