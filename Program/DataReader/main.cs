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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }
    }
}