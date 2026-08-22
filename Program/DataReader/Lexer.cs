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
       private bool IsUnwanted()
       {
            return CurrentChar() == '|' 
                   || CurrentChar() == '-' 
                   || CurrentChar() == '=' 
                   || CurrentChar() == '_' 
                   || CurrentChar() == '%'
                   || CurrentChar() == '[' 
                   ||CurrentChar() == ']'
                   || (CurrentChar() == 'V' && char.IsWhiteSpace(PeekChar(1)));
       } 
       //private void Exeption(string error) => throw new Exception($"{error}");
     /*   Dictionary<string, TokenType> Type = new Dictionary<string, TokenType>()
        {
            
        };*/

        public void StartLexing()
        {
            
            while (!IsOutOfRange())
            {
                SkipUnwantedChar();
                StoreChar();
                CeckWord();
                if(!IsOutOfRange())
                    Advance();
            }
        }

        private void SkipUnwantedChar()
        {
            if(char.IsWhiteSpace(CurrentChar()) || IsUnwanted())
                do
                {
                    Advance();
                } while(!IsOutOfRange() && (char.IsWhiteSpace(CurrentChar()) || IsUnwanted()));
        }

        private void CeckWord()
        {
            if(word == ":") 
                StoreToken(TokenType.Colon);

            else if(word == ".") 
                StoreToken(TokenType.Dot);

            else if(char.IsNumber(CurrentChar())) 
                StoreIntValue();

            else if(PeekChar(1) == ':' || PeekChar(1) == '.' || char.IsWhiteSpace(PeekChar(1))) 
                StoreToken(TokenType.Identifier);         
        }

        private void StoreIntValue()
        {
            bool Isfloat = false;
            while(!IsOutOfRange() && (char.IsNumber(PeekChar(1)) || PeekChar(1) == '.'))
            {
                Advance();
                StoreChar();
                if(CurrentChar() == '.') Isfloat = true;  
            }
            if(Isfloat) 
                StoreToken(TokenType.FloatValue);
            else 
                StoreToken(TokenType.IntValue);
        }

        private void StoreToken(TokenType type)
        {
            Tokenlist.Add(new Token {Type = type, Value = word});
            File.AppendAllText("program.log", $"\n[DEBUG] New Token Created [Type: {type}  Value: {word}]");
            ResetString();
        }
    }
}