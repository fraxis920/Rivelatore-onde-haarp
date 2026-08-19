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
       public void Set(string data)
        {
            this.data = data;
        }
       private string word = "";
       private List<Token> Tokenlist = new List<Token>();
       private void Advance() => Cursor.Position++;
       private char CurrentChar() => data[Cursor.Position];
       private void StoreChar() => word += data[Cursor.Position];
       private char PeekChar(int offset) => data[Cursor.Position + offset];
       private void ResetString() => word = "";
       private bool IsOutOfRange() => Cursor.Position+1 >= data.Length;
       private void Exeption(string error) => throw new Exception($"{error}");
        private void ResetCursor()
        {
            Cursor.Position = 0;
            Cursor.Line = 1;
            Cursor.Column = 1;
        }
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
            while (!IsOutOfRange())
            {
                SkipWhiteSpace();
                StoreChar();
                CeckWord();
                Advance();
            }
        }

        private void SkipWhiteSpace()
        {
            if(char.IsWhiteSpace(CurrentChar()))
                do
                {
                    Advance();
                } while(!IsOutOfRange() && char.IsWhiteSpace(PeekChar(1)));
        }
        private void CeckWord()
        {
            if(Type.TryGetValue(word, out TokenType type)) 
                if(PeekChar(1) == ':') 
                {
                    Advance(); 
                    StoreToken(type);     
                }
                else Exeption($"Error: Unexpected Char {PeekChar(1)}, Expected :");  

            if(char.IsNumber(CurrentChar())) StoreIntValue();
        }

        private void StoreIntValue()
        {
            while(!IsOutOfRange() && char.IsNumber(PeekChar(1)))
            {
                Advance();
                StoreChar();
            }
            StoreToken(TokenType.Value);
        }

        private void StoreToken(TokenType type)
        {
            Tokenlist.Add(new Token {Type = type, Value = word});
            File.AppendAllText("program.log", $" New Token[ Type: {type}  Value: {word}]\n");
            ResetString();
        }
    }
}