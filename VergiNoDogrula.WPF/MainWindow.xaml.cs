using System.Windows;
using System.Windows.Threading;
using VergiNoDogrula.Data;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.Services;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppSettings _appSettings = AppSettings.GetAppSettings();
        private bool _isClosingAfterBackup;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            SourceInitialized -= MainWindow_SourceInitialized;
            _appSettings.MainWindowPosition.SetWindowPositions(this);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            try
            {
                await InitializeDataContextAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _appSettings.MainWindowPosition.GetWindowPositions(this);

            if (_isClosingAfterBackup || !_appSettings.AutoBackupEnabled)
                return;

            e.Cancel = true;

            try
            {
                await AutoBackupHelper.RunAsync(byPassIsBackupDue: true);
            }
            finally
            {
                _isClosingAfterBackup = true;
                _ = Dispatcher.BeginInvoke(Close, DispatcherPriority.Background);
            }
        }

        private async Task InitializeDataContextAsync()
        {
            string databasePath = _appSettings.DatabasePath;
            var repository = new SqliteTaxPayerRepository(databasePath);
            var viewModel = new TaxPayerCollectionVM(repository);

            DataContext = viewModel;
            await viewModel.LoadDataAsync();
        }
    }
}