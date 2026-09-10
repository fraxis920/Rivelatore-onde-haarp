namespace DataHandler
{
    public class Token
    {
        public TokenType Type;     
        public string Value = string.Empty;      
        public int Position;     
        public int line;      
        public int column;     
    }
}