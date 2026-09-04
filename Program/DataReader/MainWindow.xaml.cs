using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DataHandler
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // inizializziamo il ViewModel principale
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.AddLog("INFO", "Applicazione avviata ed operativa.");
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            // switch al volo tra tema chiaro e scuro
            _viewModel.ToggleTheme();
        }

        // qua gestiamo il drag della finestra senza bloccare i click sui menu a tendina o pulsanti
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.OriginalSource is DependencyObject originalSource)
                {
                    DependencyObject? parent = originalSource;
                    while (parent != null && parent != sender)
                    {
                        // se abbiamo cliccato su una combo o un bottone lasciamo fare a WPF
                        if (parent is ComboBox || parent is ToggleButton || parent is Button || parent is ComboBoxItem)
                        {
                            return;
                        }
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                }

                // altrimenti spostiamo la finestra normalmente
                DragMove();
            }
        }

        // chiusura rapida dal pallino rosso in alto a sinistra
        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // riduci a icona
        private void WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // ingrandisci o ripristina
        private void WindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
