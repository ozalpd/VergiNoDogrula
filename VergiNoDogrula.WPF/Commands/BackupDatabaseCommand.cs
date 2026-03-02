namespace VergiNoDogrula.WPF.Commands;

internal class BackupDatabaseCommand : AbstractCommand
{
    public override bool CanExecute(object? parameter) => true;

    public override async void Execute(object? parameter)
    {
        if (parameter is ViewModels.TaxPayerCollectionVM collectionVM)
        {
            await collectionVM.CreateBackupAsync();
        }
    }
}
