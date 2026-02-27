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
                    RaiseCommandsCanExecuteChanged();
                }
            }
        }

        public ICommand AddTaxPayerCommand { get; }
        public ICommand SaveTaxPayerCommand { get; }
        public ICommand DeleteTaxPayerCommand { get; }

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
                MessageBox.Show("Kayıt başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Kayıt başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
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
