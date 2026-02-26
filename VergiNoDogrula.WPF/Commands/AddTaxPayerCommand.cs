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

            var newTaxPayer = new TaxPayerVM();
            collection.TaxPayers.Add(newTaxPayer);
            collection.SelectedItem = newTaxPayer;
        }
    }
}
