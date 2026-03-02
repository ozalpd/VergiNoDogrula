using System.Windows;

namespace VergiNoDogrula.WPF.Commands;

internal class ShowAboutCommand : AbstractCommand
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter)
    {
        var aboutDialog = new Dialogs.AboutDialog
        {
            Owner = Application.Current.MainWindow
        };
        aboutDialog.ShowDialog();
    }
}
