using System.Windows;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Dialogs
{
    public partial class AddTaxPayerDialog : Window
    {
        private readonly TaxPayerVM _viewModel;

        internal TaxPayerVM? Result { get; private set; }

        public AddTaxPayerDialog()
        {
            InitializeComponent();
            _viewModel = new TaxPayerVM();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TaxNumber = TaxNumberTextBox.Text;
            _viewModel.Title = TitleTextBox.Text;
            _viewModel.Validate();

            if (_viewModel.HasErrors)
            {
                ErrorMessage.Text = string.Join("\n", GetAllErrors(_viewModel));
                return;
            }

            Result = _viewModel;
            DialogResult = true;
            Close();
        }

        private void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OkButton.IsEnabled = !string.IsNullOrWhiteSpace(TaxNumberTextBox.Text) &&
                                 !string.IsNullOrWhiteSpace(TitleTextBox.Text);
        }

        private IEnumerable<string> GetAllErrors(TaxPayerVM viewModel)
        {
            var errors = new List<string>();
            foreach (var error in viewModel.GetErrors(nameof(TaxPayerVM.Title)))
            {
                errors.Add(error.ToString() ?? "");
            }
            foreach (var error in viewModel.GetErrors(nameof(TaxPayerVM.TaxNumber)))
            {
                errors.Add(error.ToString() ?? "");
            }
            return errors;
        }
    }
}
