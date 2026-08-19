using System;

namespace DataReader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ReadData reader = new ReadData();
                reader.CheckPort();
                Lexer lexer = new Lexer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }
    }
}