using System;
using System.Windows;

namespace DataHandler
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
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
