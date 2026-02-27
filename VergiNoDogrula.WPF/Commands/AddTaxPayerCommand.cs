using System.Windows;
using VergiNoDogrula.WPF.Dialogs;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Commands;

internal class AddTaxPayerCommand : AbstractCommand
{
    public override void Execute(object? parameter)
    {
        if (parameter == null || parameter is TaxPayerCollectionVM == false)
            return;

        TaxPayerCollectionVM collection = (TaxPayerCollectionVM)parameter;
        var dialog = new AddTaxPayerDialog();
        dialog.TaxPayerCollection = collection;
        var owner = Application.Current?.MainWindow;
        if (owner != null)
        {
            dialog.Owner = owner;
        }

        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            var newTaxPayer = dialog.Result;
            newTaxPayer.Validate();
            if (newTaxPayer.HasErrors) //This is not expected but its better safe than sorry :-)
            {
                var errors = newTaxPayer.GetErrors(nameof(newTaxPayer.TaxNumber));
                MessageBox.Show(string.Join("\n", errors), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            collection.TaxPayers.Add(newTaxPayer);
            collection.SelectedItem = newTaxPayer;
        }
    }
}

