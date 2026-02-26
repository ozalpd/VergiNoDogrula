using System.Windows.Input;

namespace VergiNoDogrula.WPF.Commands
{
    internal abstract class AbstractCommand : ICommand
    {
        public abstract void Execute(object? parameter);

        public virtual bool CanExecute(object? parameter)
        {
            return true;
        }

        protected virtual void RaiseCanExecuteChanged(object? sender = null, EventArgs? e = null)
        {
            if (sender == null)
                sender = this;
            if (e == null)
                e = new EventArgs();

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(sender, e);
            }
        }
        public event EventHandler? CanExecuteChanged;
    }
}
