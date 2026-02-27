using System.Windows;
using VergiNoDogrula.WPF.Dialogs;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Commands
{
    internal class AddTaxPayerCommand : AbstractCommand
    {
        public override void Execute(object? parameter)
        {
            if (parameter == null || parameter is TaxPayerCollectionVM == false)
                return;

            TaxPayerCollectionVM collection = (TaxPayerCollectionVM)parameter;
            var selectedItem = collection.SelectedItem;
            if (selectedItem != null)
            {
                selectedItem.Validate();
                if (selectedItem.HasErrors)
                    return;
            }

            var dialog = new AddTaxPayerDialog();
            var owner = Application.Current?.MainWindow;
            if (owner != null)
            {
                dialog.Owner = owner;
            }

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                collection.TaxPayers.Add(dialog.Result);
                collection.SelectedItem = dialog.Result;
            }
        }
    }
}
