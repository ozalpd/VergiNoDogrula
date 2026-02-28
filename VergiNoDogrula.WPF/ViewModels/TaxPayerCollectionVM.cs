using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using VergiNoDogrula.Data;
using VergiNoDogrula.Models;
using VergiNoDogrula.WPF.Commands;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal class TaxPayerCollectionVM : AbstractViewModel
    {
        private TaxPayerVM? _selectedItem;
        private readonly ITaxPayerRepository _repository;

        public TaxPayerCollectionVM(ITaxPayerRepository repository)
        {
            _repository = repository;
            TaxPayers = new ObservableCollection<TaxPayerVM>();
            AddTaxPayerCommand = new AddTaxPayerCommand();
            SaveTaxPayerCommand = new SaveTaxPayerCommand();
            DeleteTaxPayerCommand = new DeleteTaxPayerCommand();
        }

        public ObservableCollection<TaxPayerVM> TaxPayers { get; }

        public TaxPayerVM? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.ErrorsChanged -= OnSelectedItemErrorsChanged;
                    }

                    _selectedItem = value;

                    if (_selectedItem != null)
                    {
                        _selectedItem.ErrorsChanged += OnSelectedItemErrorsChanged;
                    }

                    RaisePropertyChanged(nameof(SelectedItem));
                    Status = string.Empty;
                    RaiseCommandsCanExecuteChanged();
                }
            }
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

            TaxPayers.Add(taxPayer);
            SelectedItem = taxPayer;
            await SaveCurrentAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var taxPayers = await _repository.GetAllAsync();
                TaxPayers.Clear();
                foreach (var taxPayer in taxPayers)
                {
                    TaxPayers.Add(new TaxPayerVM(taxPayer));
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
                    TaxPayers.Remove(SelectedItem);
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
