using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Commands;

internal class EmptySearchStringCommand : AbstractCommand
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;
        if (parameter is TaxPayerCollectionVM collectionVM)
        {
            collectionVM.SearchString = string.Empty;
        }
    }
}
