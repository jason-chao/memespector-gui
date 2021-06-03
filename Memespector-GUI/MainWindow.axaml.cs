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
            {
                if (viewModel.IsInvocationInProgress)
                {
                    e.Cancel = true;
                    var dialogChoice = await Utilities.ShowOkCancelDialog("Invocation in progress", $"The images are being processed.  Some of the images will not be processed if you quit the application now.{System.Environment.NewLine + System.Environment.NewLine}Press \"OK\" if you are sure want to quit the application.", this);
                    if (dialogChoice == MessageBox.Avalonia.Enums.ButtonResult.Ok)
                    {
                        System.Environment.Exit(0);
                    }
                }
            }
        }

        private void resetComboBoxSelectedIndex(object sender, SelectionChangedEventArgs args)
        {
            (sender as ComboBox).SelectedItem = null;
        }
    }
}
