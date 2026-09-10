using System;
using System.Windows;

namespace DataHandler
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // FIX: senza questa chiamata la cartella "Logs" non viene mai creata e
            // il logger fallisce silenziosamente (l'eccezione finiva solo su Console,
            // invisibile in un'app WPF). Deve essere la primissima cosa che facciamo.
            DebugLogger.Initialize(clearLog: true);

            // Handle unhandled exceptions gracefully
            DispatcherUnhandledException += (s, args) =>
            {
                DebugLogger.Error("App", "Unhandled WPF exception", args.Exception);
                MessageBox.Show($"Si è verificato un errore imprevisto:\n{args.Exception.Message}", 
                                "Errore Applicazione", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}
