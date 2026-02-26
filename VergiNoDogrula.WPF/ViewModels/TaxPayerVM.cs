using System.Collections;
using System.ComponentModel;
using VergiNoDogrula.Models;

namespace VergiNoDogrula.WPF.ViewModels
{
    internal class TaxPayerVM : AbstractDataErrorInfoVM, ITaxPayer
    {
        TaxPayer _taxPayer;
        string _taxNumber = string.Empty;
        string _title = string.Empty;

        public TaxPayerVM()
        {
            _taxPayer = new TaxPayer();
        }

        public TaxPayerVM(string title, string taxNumber)
        {
            _taxPayer = new TaxPayer();
            TaxNumber = taxNumber;
            Title = title;
        }

        public string TaxNumber
        {
            get => _taxNumber;
            set
            {
                if (_taxNumber != value)
                {
                    _taxNumber = value;
                    ValidateTaxNumber(value);
                    RaisePropertyChanged(nameof(TaxNumber));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    ValidateTitle(value);
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public void Validate()
        {
            ValidateTaxNumber(TaxNumber);
            ValidateTitle(Title);
        }

        private void ValidateTaxNumber(string value)
        {
            ClearErrors(nameof(TaxNumber));
            try
            {
                _taxPayer.TaxNumber = value;
            }
            catch (Exception ex)
            {
                AddError(nameof(TaxNumber), ex.Message);
            }
        }

        private void ValidateTitle(string value)
        {
            ClearErrors(nameof(Title));
            try
            {
                _taxPayer.Title = value;
            }
            catch (Exception ex)
            {
                AddError(nameof(Title), ex.Message);
            }
        }

        public bool Equals(TaxPayer? other)
        {
            return _taxPayer.Equals(other);
        }
    }
}
