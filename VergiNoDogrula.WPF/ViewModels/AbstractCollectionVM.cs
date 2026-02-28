using System.Collections.ObjectModel;

namespace VergiNoDogrula.WPF.ViewModels;

internal abstract class AbstractCollectionVM<T> : AbstractViewModel
{
    protected AbstractCollectionVM()
    {
        Collection = new ObservableCollection<T>();
        CollectionFiltered = Collection;
    }

    public ObservableCollection<T> Collection { get; }
    public ObservableCollection<T> CollectionFiltered { get; protected set; }

    protected abstract void OnSearchStringChanged();
    protected abstract void OnSelectedItemChanging(T? newSelectedItem);
    protected abstract void OnSelectedItemChanged(T? oldSelectedItem);



    public T? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem is IEquatable<T>)
            {
                if (Equals(_selectedItem, value))
                    return;
            }
            OnSelectedItemChanging(value);
            var oldItem = _selectedItem;
            _selectedItem = value;
            RaisePropertyChanged(nameof(SelectedItem));
            OnSelectedItemChanged(oldItem);
        }
    }
    protected T? _selectedItem;


    public string SearchString
    {
        get => _searchString ?? string.Empty;
        set
        {
            _searchString = value;
            RaisePropertyChanged(nameof(SearchString));
            OnSearchStringChanged();
            RaisePropertyChanged(nameof(CollectionFiltered));
            if (SelectedItem == null)
            {
                if (CollectionFiltered.Count > 0)
                    SelectedItem = CollectionFiltered[0];
            }
        }
    }
    private string? _searchString;
}

