namespace DataReader
{
    class Lexer
    {
        
        struct LexerCursor
        {
            public int Position;
            public int Line;
            public int Column;
        }
        LexerCursor Cursor = new LexerCursor
        {
            Position = 0,
            Line = 1,
            Column = 1
        };
       private string data = "";
       public Lexer(string data)
        {
            this.data = data;
        }
       private string word = "";
       private List<Token> Tokenlist = new List<Token>();
       private void Advance() => Cursor.Position++;
       private void StoreChar() => word += data[Cursor.Position];
       private char PeekToken(int offset) => data[Cursor.Position + offset];
        Dictionary<string, TokenType> Type = new Dictionary<string, TokenType>()
        {
            {"Frequenza", TokenType.Frequenza},
            {"Ampiezza", TokenType.Ampiezza},
            {"Durata", TokenType.Durata},
            {"Timestamp", TokenType.Timestamp},
            {"Type", TokenType.Type}
        };
        public void StartLexing()
        {
            
        }
    }
}