using System.ComponentModel;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal abstract class AbstractViewModel : INotifyPropertyChanged
    {
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
