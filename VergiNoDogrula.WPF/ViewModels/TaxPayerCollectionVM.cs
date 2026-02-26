using System.Collections.ObjectModel;
using System.Windows.Input;
using VergiNoDogrula.WPF.Commands;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal class TaxPayerCollectionVM : AbstractViewModel
    {
        private TaxPayerVM? _selectedItem;

        public TaxPayerCollectionVM()
        {
            TaxPayers = new ObservableCollection<TaxPayerVM>();
            AddTaxPayerCommand = new AddTaxPayerCommand();
        }

        public ObservableCollection<TaxPayerVM> TaxPayers { get; }

        public TaxPayerVM? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public ICommand AddTaxPayerCommand { get; }
    }
}
