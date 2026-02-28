using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using VergiNoDogrula.Data;
using VergiNoDogrula.Models;
using VergiNoDogrula.WPF.Commands;
using VergiNoDogrula.WPF.Helpers;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal class TaxPayerCollectionVM : AbstractCollectionVM<TaxPayerVM>
    {
        private readonly ITaxPayerRepository _repository;

        public TaxPayerCollectionVM(ITaxPayerRepository repository)
        {
            _repository = repository;

            AddTaxPayerCommand = new AddTaxPayerCommand();
            SaveTaxPayerCommand = new SaveTaxPayerCommand();
            DeleteTaxPayerCommand = new DeleteTaxPayerCommand();
        }


        public string Status
        {
            get => _status ?? string.Empty;
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }
        private string? _status;

        public ICommand AddTaxPayerCommand { get; }
        public ICommand DeleteTaxPayerCommand { get; }
        public ICommand SaveTaxPayerCommand { get; }

        protected override void OnSearchStringChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                CollectionFiltered = Collection;
                return;
            }

            if (SearchString.IsNumeric())
            {
                CollectionFiltered = new ObservableCollection<TaxPayerVM>(
                    Collection.Where(tp => tp.TaxNumber.Contains(SearchString)));
            }
            else
            {
                var lowerSearch = SearchString.ToLower();
                CollectionFiltered = new ObservableCollection<TaxPayerVM>(
                    Collection.Where(tp => tp.Title.ToLower().Contains(lowerSearch)));
            }
        }

        protected override void OnSelectedItemChanging(TaxPayerVM? newSelectedItem)
        {
            if (_selectedItem != null)
            {
                _selectedItem.ErrorsChanged -= OnSelectedItemErrorsChanged;
            }
        }

        protected override void OnSelectedItemChanged(TaxPayerVM? oldSelectedItem)
        {
            if (_selectedItem != null)
            {
                _selectedItem.ErrorsChanged += OnSelectedItemErrorsChanged;
            }
            Status = string.Empty;
            RaiseCommandsCanExecuteChanged();
        }

        private void OnSelectedItemErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            if (SaveTaxPayerCommand is AbstractCommand saveCommand)
            {
                saveCommand.RaiseCanExecuteChanged();
            }
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            if (AddTaxPayerCommand is AbstractCommand addCommand)
            {
                addCommand.RaiseCanExecuteChanged();
            }
            if (SaveTaxPayerCommand is AbstractCommand saveCommand)
            {
                saveCommand.RaiseCanExecuteChanged();
            }
            if (DeleteTaxPayerCommand is AbstractCommand deleteCommand)
            {
                deleteCommand.RaiseCanExecuteChanged();
            }
        }

        public async Task AddNewAsync(TaxPayerVM taxPayer)
        {
            if (taxPayer == null)
                return;

            CollectionFiltered.Add(taxPayer);
            SelectedItem = taxPayer;
            await SaveCurrentAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var taxPayers = await _repository.GetAllAsync();
                CollectionFiltered.Clear();
                foreach (var taxPayer in taxPayers)
                {
                    CollectionFiltered.Add(new TaxPayerVM(taxPayer));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SaveCurrentAsync()
        {
            if (SelectedItem == null || SelectedItem.HasErrors)
                return;

            try
            {
                var taxPayer = new TaxPayer(SelectedItem.Title, SelectedItem.TaxNumber);
                await _repository.SaveAsync(taxPayer);
                Status = "Kayıt başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DeleteSelectedAsync()
        {
            if (SelectedItem == null)
                return;

            var result = MessageBox.Show("Seçili kaydı silmek istediğinizden emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var deleted = await _repository.DeleteAsync(SelectedItem.TaxNumber);
                if (deleted)
                {
                    CollectionFiltered.Remove(SelectedItem);
                    SelectedItem = null;
                    Status = "Kayıt başarıyla silindi.";
                }
                else
                {
                    MessageBox.Show("Kayıt bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
