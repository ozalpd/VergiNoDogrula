using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using VergiNoDogrula.Data;
using VergiNoDogrula.i18n;
using VergiNoDogrula.Models;
using VergiNoDogrula.WPF.Commands;
using VergiNoDogrula.WPF.Extensions;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.Services;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal class TaxPayerCollectionVM : AbstractCollectionVM<TaxPayerVM>
    {
        private readonly ITaxPayerRepository _repository;
        private readonly IBackupService _backupService;

        public TaxPayerCollectionVM(ITaxPayerRepository repository)
        {
            _repository = repository;
            _backupService = new DatabaseBackupService(AppSettings.GetAppSettings());

            AddTaxPayerCommand = new AddTaxPayerCommand();
            CopyTaxNumberCommand = new CopyTaxNumberCommand();
            EmptySearchStringCommand = new EmptySearchStringCommand();
            DeleteTaxPayerCommand = new DeleteTaxPayerCommand();
            SaveTaxPayerCommand = new SaveTaxPayerCommand();
            BackupDatabaseCommand = new BackupDatabaseCommand();
            ShowBackupListCommand = new ShowBackupListCommand();
            ShowAboutCommand = new ShowAboutCommand();
        }

        public IBackupService BackupService => _backupService;

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
        public ICommand CopyTaxNumberCommand { get; }
        public ICommand DeleteTaxPayerCommand { get; }
        public ICommand EmptySearchStringCommand { get; }
        public ICommand SaveTaxPayerCommand { get; }
        public ICommand BackupDatabaseCommand { get; }
        public ICommand ShowBackupListCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public bool IsSearching { get; private set; }
        public bool IsSearchNumeric { get; private set; }

        protected override void OnSearchStringChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                CollectionFiltered = Collection;
                IsSearchNumeric = false;
                IsSearching = false;
                return;
            }

            IsSearching = true;
            IsSearchNumeric = SearchString.IsNumeric();
            if (IsSearchNumeric)
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
            if (CopyTaxNumberCommand is AbstractCommand copyCommand)
            {
                copyCommand.RaiseCanExecuteChanged();
            }
            if (DeleteTaxPayerCommand is AbstractCommand deleteCommand)
            {
                deleteCommand.RaiseCanExecuteChanged();
            }
            if (SaveTaxPayerCommand is AbstractCommand saveCommand)
            {
                saveCommand.RaiseCanExecuteChanged();
            }
        }

        public async Task AddNewAsync(TaxPayerVM taxPayer)
        {
            if (taxPayer == null)
                return;

            Collection.Add(taxPayer);
            SelectedItem = taxPayer;
            await SaveCurrentAsync();

            if (string.IsNullOrWhiteSpace(SearchString))
            {
                CollectionFiltered = Collection;
            }
            else
            {
                var search = SearchString;
                SearchString = string.Empty;
                SearchString = search;
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var taxPayers = await _repository.GetAllAsync();
                Collection.Clear();
                foreach (var taxPayer in taxPayers)
                {
                    Collection.Add(new TaxPayerVM(taxPayer));
                }

                if (Collection.Count > 0)
                {
                    Status = string.Format(Strings.TaxPayersLoadedFormat, Collection.Count);
                }
                else
                {
                    Status = Strings.NoRecordsInDatabase;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.DataLoadErrorFormat, ex.Message), Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnSearchStringChanged();
        }
        private void PlaySuccessSound()
        {
            var settings = AppSettings.GetAppSettings();
            if (settings.MuteAudio)
                return;

            try
            {
                SystemSounds.Beep.Play();
            }
            catch
            {
                // Silently fail; don't crash the app
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
                Status = Strings.TaxPayerSavedSuccess;
                PlaySuccessSound();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.SaveErrorFormat, ex.Message), Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DeleteSelectedAsync()
        {
            if (SelectedItem == null)
                return;

            var result = MessageBox.Show(Strings.DeleteConfirmation, Strings.ConfirmationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var deleted = await _repository.DeleteAsync(SelectedItem.TaxNumber);
                if (deleted)
                {
                    Collection.Remove(SelectedItem);
                    SelectedItem = null;
                    Status = Strings.TaxPayerDeletedSuccess;
                    OnSearchStringChanged();
                }
                else
                {
                    MessageBox.Show(Strings.RecordNotFound, Strings.WarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.DeleteErrorFormat, ex.Message), Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateBackupAsync()
        {
            try
            {
                var settings = AppSettings.GetAppSettings();
                var connectionString = $"Data Source={settings.DatabasePath}";
                var backupPath = await _backupService.CreateBackupAsync(connectionString);

                if (backupPath != null)
                {
                    Status = string.Format(Strings.BackupCreatedFormat, Path.GetFileName(backupPath));
                    PlaySuccessSound();

                    await _backupService.CleanupOldBackupsAsync();
                }
                else
                {
                    Status = Strings.BackupFailed;
                    MessageBox.Show(Strings.BackupCreateFailed, Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.BackupErrorFormat, ex.Message), Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
