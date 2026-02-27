using System.IO;
using System.Windows;
using VergiNoDogrula.Data;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeDataContext();
            InitializeComponent();
        }

        private async void InitializeDataContext()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VergiNoDogrula");
            Directory.CreateDirectory(appDataPath);

            var databasePath = Path.Combine(appDataPath, "taxpayers.db");
            var repository = new SqliteTaxPayerRepository(databasePath);
            var viewModel = new TaxPayerCollectionVM(repository);

            DataContext = viewModel;
            await viewModel.LoadDataAsync();
        }
    }
}