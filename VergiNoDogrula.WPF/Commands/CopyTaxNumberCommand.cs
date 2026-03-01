using System.Windows;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Commands
{
    internal class CopyTaxNumberCommand : AbstractCommand
    {
        public override bool CanExecute(object? parameter)
        {
            if (parameter is not ViewModels.TaxPayerCollectionVM collectionVM)
                return false;

            return collectionVM.SelectedItem != null;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is TaxPayerCollectionVM collectionVM)
            {
                var taxNumber = collectionVM.SelectedItem?.TaxNumber;
                if (!string.IsNullOrEmpty(taxNumber))
                {
                    Clipboard.SetText(taxNumber);
                }
            }
        }
    }
}
