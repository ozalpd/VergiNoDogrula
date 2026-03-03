using System.Windows;
using VergiNoDogrula.WPF.Dialogs;
using VergiNoDogrula.WPF.Services;

namespace VergiNoDogrula.WPF.Commands;

/// <summary>
/// Command to show the backup files list dialog.
/// </summary>
internal class ShowBackupListCommand : AbstractCommand
{
    public override bool CanExecute(object? parameter)
    {
        return parameter is IBackupService;
    }

    public override void Execute(object? parameter)
    {
        if (parameter is not IBackupService backupService)
            return;

        var dialog = new BackupListDialog(backupService)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }
}
