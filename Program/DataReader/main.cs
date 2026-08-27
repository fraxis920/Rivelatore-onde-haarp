using System;
using DataHandler;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DebugLogger.ConsoleOutput = false;
                DebugLogger.FileOutput = true;

                DebugLogger.Initialize(clearLog: true);
                
                ReadData reader = new ReadData();
                reader.CheckPort();
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Main", "Unhandled exception", ex);
            }
        }
    }
