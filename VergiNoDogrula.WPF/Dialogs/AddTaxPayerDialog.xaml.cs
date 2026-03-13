using System.ComponentModel;
using System.Windows;
using VergiNoDogrula.i18n;
using VergiNoDogrula.WPF.ViewModels;

namespace VergiNoDogrula.WPF.Dialogs;

public partial class AddTaxPayerDialog : Window
{
    private readonly TaxPayerVM _viewModel;

    internal TaxPayerVM? Result { get; private set; }
    internal TaxPayerCollectionVM? TaxPayerCollection { get; set; }


    public AddTaxPayerDialog()
    {
        InitializeComponent();
        _viewModel = new TaxPayerVM();
        _viewModel.ErrorsChanged += OnViewModelErrorsChanged;
         DataContext = _viewModel;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (TaxPayerCollection != null && TaxPayerCollection.IsSearchNumeric)
        {
            TaxNumberTextBox.Text = TaxPayerCollection.SearchString;
        }

        TaxNumberTextBox.Focus();
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.ErrorsChanged -= OnViewModelErrorsChanged;
    }


    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.TaxNumber = TaxNumberTextBox.Text;
        _viewModel.Title = TitleTextBox.Text.Trim();

        if (_viewModel.HasErrors)
            return;

        Result = _viewModel;
        DialogResult = true;
        Close();
    }

    private void TaxNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _viewModel.TaxNumber = TaxNumberTextBox.Text;
    }

    private void TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _viewModel.Title = TitleTextBox.Text.Trim();
    }


    private void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        OkButton.IsEnabled = !string.IsNullOrWhiteSpace(TaxNumberTextBox.Text) &&
                             IsTaxNumberOK() &&
                             !string.IsNullOrWhiteSpace(TitleTextBox.Text);
    }

    private void OnViewModelErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (_viewModel.HasErrors)
        {
            ErrorMessage.Text = string.Join("\n", GetAllErrors(_viewModel));
        }
        else
        {
            IsTaxNumberOK();
        }
    }

    private void TaxNumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !char.IsDigit(e.Text, 0);
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

    private bool IsTaxNumberOK()
    {
        if (!_viewModel.HasErrors)
        {
            ErrorMessage.Text = string.Empty;
        }
        string taxNumber = TaxNumberTextBox.Text;
        if (string.IsNullOrWhiteSpace(taxNumber) || !ValidateExtensions.IsValidTaxNumber(taxNumber))
            return false;


        if (TaxPayerCollection != null)
        {
            if (TaxPayerCollection.Collection.Any(t => t.TaxNumber.Equals(taxNumber)))
            {
                ErrorMessage.Text = Strings.DuplicateTaxNumber;
                return false;
            }
        }

        return true;
    }
}
