using System.Windows;
using VergiNoDogrula.Data;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppSettings _appSettings = AppSettings.GetAppSettings();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
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