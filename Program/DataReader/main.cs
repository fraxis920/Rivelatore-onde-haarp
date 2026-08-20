using System;

namespace DataReader
{
    class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText("program.log", string.Empty);
            try
            {
                ReadData reader = new ReadData();
                reader.CheckPort();
                Lexer lexer = new Lexer();
            }
            catch (Exception ex)
            {
                File.AppendAllText("program.log", $"[Error]: {ex.Message}\n");
            }
        }
    }
}