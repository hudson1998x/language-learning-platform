namespace LLE.Kernel.DataQL.Tokeniser
{
    public enum TokenKind : byte
    {
        Identifier,     // UserId, AllowedUsers

        Parameter,      // :userId

        Literal,        // 123, "John", true

        Operator,       // =, !=, >, <, >=, <=
        Dot,            // .

        And,            // and
        Or,             // or
        Not,            // not

        In,             // in

        OpenParen,      // (
        CloseParen,     // )

        Comma,          // ,

        EndOfFile,
    }
}