

namespace DataHandler
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error
    }

    public static class DebugLogger
    {
        private static readonly object LockObject = new();

        private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, "program.log");

        // Configurazione
        public static bool Enabled { get; set; } = true;
        public static bool ConsoleOutput { get; set; } = true;
        public static bool FileOutput { get; set; } = true;

        // Livello minimo visualizzato
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        // Inizializzazione
        public static void Initialize(bool clearLog = false)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);

                if (clearLog && File.Exists(LogFile))
                    File.Delete(LogFile);

                Log(LogLevel.Info, "Logger", "Logger inizializzato");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGGER ERROR] {ex.Message}");
            }
        }

        // Metodo principale
        public static void Log(LogLevel level, string source, string message)
        {
            if (!Enabled)
                return;

            if (level < MinimumLevel)
                return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            int threadId = Environment.CurrentManagedThreadId;

            string output = $"[{timestamp}] [{level.ToString().ToUpper()}] [{source}] [Thread:{threadId}] {message}";

            lock (LockObject)
            {
                if (ConsoleOutput)
                    Console.WriteLine(output);

                if (FileOutput)
                {
                    try
                    {
                        File.AppendAllText(LogFile, output + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LOGGER ERROR] {ex.Message}");
                    }
                }
            }
        }

        // TRACE
        public static void Trace(string source, string message)
            => Log(LogLevel.Trace, source, message);

        // DEBUG
        public static void Debug(string source, string message)
            => Log(LogLevel.Debug, source, message);

        // INFO
        public static void Info(string source, string message)
            => Log(LogLevel.Info, source, message);

        // WARNING
        public static void Warning(string source, string message)
            => Log(LogLevel.Warning, source, message);

        // ERROR
        public static void Error(string source, string message)
            => Log(LogLevel.Error, source, message);

        // ERROR con eccezione
        public static void Error(string source, string message, Exception exception)
        {
            string errorMessage =
                $"{message} | Exception: {exception.GetType().Name} | " +
                $"Message: {exception.Message} | StackTrace: {exception.StackTrace}";

            Log(LogLevel.Error, source, errorMessage);
        }

        // ENTER funzione
        public static void Enter(string source, string function)
            => Debug(source, $"ENTER {function}()");

        // EXIT funzione
        public static void Exit(string source, string function)
            => Debug(source, $"EXIT {function}()");

        // ENTER con parametri
        public static void Enter(string source, string function, string parameters)
            => Debug(source, $"ENTER {function}() | Parameters: {parameters}");

        // EXIT con risultato
        public static void Exit(string source, string function, object? result)
            => Debug(source, $"EXIT {function}() | Result: {result}");

        // Log di un Token
        public static void Token(Token token)
            => Debug("Lexer", $"Token Created [Type: {token.Type} | Value: {token.Value}]");

        // Log di una riga ricevuta
        public static void ReceivedLine(string line)
            => Debug("Reader", $"Received: \"{line}\"");

        // Log di una riga inviata
        public static void SentLine(string line)
            => Debug("Reader", $"Sent: \"{line}\"");

        // Log lista token
        public static void TokenList(System.Collections.Generic.List<Token> tokens)
        {
            Debug("Lexer", $"TokenList Count: {tokens.Count}");

            for (int i = 0; i < tokens.Count; i++)
            {
                Debug("Lexer", $"Token[{i}] Type={tokens[i].Type} Value='{tokens[i].Value}'");
            }
        }

        // Log risultato controllo
        public static void Completion(string dataType, bool result)
            => Debug("Parser", $"{dataType} Complete = {result}");

        // Log valori booleani
        public static void Check(string source, string name, bool value)
            => Debug(source, $"{name} = {value}");

        // Log generico di dati
        public static void Data(string source, string name, object? value)
            => Debug(source, $"{name} = {value}");
    }
}