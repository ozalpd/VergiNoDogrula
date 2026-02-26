using System.Windows;
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
            InitializeComponent();
            DataContext = new TaxPayerCollectionVM();
        }
    }
}