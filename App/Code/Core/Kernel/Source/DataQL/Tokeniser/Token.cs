namespace LLE.Kernel.DataQL.Tokeniser
{
    public struct Token(TokenKind kind, int start)
    {
        public TokenKind Kind = kind;

        public int StartPosition = start;
    
        public int Length;
    }
}