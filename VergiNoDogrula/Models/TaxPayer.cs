using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VergiNoDogrula.Models
{
    public class TaxPayer : ITaxPayer, IEquatable<TaxPayer>
    {
        public TaxPayer() { }
        public TaxPayer(string title, string taxNumber)
        {
            Title = title;
            TaxNumber = taxNumber;
        }


        public string Title
        {
            get { return _title; }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Title cannot be empty.", nameof(value));
                }
                _title = value;
            }
        }
        private string _title = string.Empty;

        public string TaxNumber
        {
            get { return _taxNumber; }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (!value.IsValidTaxNumber())
                {
                    throw new ArgumentException("The provided value is not a valid tax number.", nameof(value));
                }

                _taxNumber = value;
            }
        }
        private string _taxNumber = string.Empty;

        public bool Equals(TaxPayer? other)
        {
            //Only need to check TaxNumber for equality, as it is unique for each TaxPayer
            return TaxNumber.IsSameTaxNumbers(other);
        }
    }
}
