using System.Windows.Input;

namespace VergiNoDogrula.WPF.Commands
{
    internal class DeleteTaxPayerCommand : AbstractCommand
    {
        public override bool CanExecute(object? parameter)
        {
            if (parameter is not ViewModels.TaxPayerCollectionVM collectionVM)
                return false;

            return collectionVM.SelectedItem != null;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is ViewModels.TaxPayerCollectionVM collectionVM)
            {
                await collectionVM.DeleteSelectedAsync();
            }
        }
    }
}
