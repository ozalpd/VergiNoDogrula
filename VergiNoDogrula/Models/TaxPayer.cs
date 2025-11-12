namespace VergiNoDogrula.Models
{
    /// <summary>
    /// Represents an entity that is subject to taxation, identified by a unique tax number and a title.
    /// </summary>
    /// <remarks>A TaxPayer is uniquely identified by its tax number. Instances of this class can be compared
    /// for equality based on their tax numbers. The class enforces validation on both the title and tax number to
    /// ensure they are not null or empty, and that the tax number is in a valid format.</remarks>
    public class TaxPayer : ITaxPayer, IEquatable<TaxPayer>
    {
        public TaxPayer() { }
        public TaxPayer(string title, string taxNumber)
        {
            Title = title;
            TaxNumber = taxNumber;
        }

        /// <summary>
        /// Gets or sets the title associated with the object.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the tax identification number associated with the entity.
        /// </summary>
        /// <remarks>The value is automatically trimmed of leading and trailing whitespace. The property
        /// validates that the assigned value is a valid tax number format. Assigning an invalid or null value will
        /// result in an exception.</remarks>
        public string TaxNumber
        {
            get { return _taxNumber; }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                string trimmedValue = value.Trim();
                if (!trimmedValue.IsValidTaxNumber())
                {
                    throw new ArgumentException("The provided value is not a valid tax number.", nameof(value));
                }

                _taxNumber = trimmedValue;
            }
        }
        private string _taxNumber = string.Empty;

        /// <summary>
        /// Determines whether the current TaxPayer is equal to another TaxPayer based on their tax numbers.
        /// </summary>
        /// <remarks>Equality is determined solely by the TaxNumber property, which uniquely identifies
        /// each TaxPayer.</remarks>
        /// <param name="other">The TaxPayer to compare with the current TaxPayer, or <see langword="null"/> to compare with no object.</param>
        /// <returns>true if the tax numbers of both TaxPayer instances are equal; otherwise, false.</returns>
        public bool Equals(TaxPayer? other)
        {
            //Only need to check TaxNumber for equality, as it is unique for each TaxPayer
            return TaxNumber.IsSameTaxNumbers(other);
        }
    }
}
