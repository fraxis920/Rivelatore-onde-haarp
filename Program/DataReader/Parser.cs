namespace DataReader
{
    class Parser
    {
        private List<Token> TokenList;

        public void Set(List<Token> tokenList)
        {
            TokenList = tokenList;
        }

        private int Cursor = 0;
        private Token Current => TokenList[Cursor];
        private Token Advance() => TokenList[Cursor++];    
        private Token PeekToken(int offset) => TokenList[Cursor + offset];
        private bool Check(TokenType type) => Current.Type == type;
        private Token Expect(TokenType token, string msg) 
        {
            if (!Check(token)) throw new Exception($"[Error] {msg}");
            return Advance();
        }

        public void StartParsing()
        {
            var Data = new List<Data>();
        }
    }
}