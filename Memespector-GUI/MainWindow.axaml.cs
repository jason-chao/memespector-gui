using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Memespector_GUI
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.Closing += MainWindow_Closing;
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;
            if (viewModel != null)
                if (viewModel.IsInvocationInProgress)
                {
                    e.Cancel = true;
                    Utilities.ShowInfoDialog("Invocation in progress", $"The images are being processed.{System.Environment.NewLine + System.Environment.NewLine}If you have to close this application, please terminate the process using the task manager of the OS.", this);
                }
        }
    }
}
