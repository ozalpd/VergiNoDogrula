using VergiNoDogrula.Models;

namespace VergiNoDogrula.Data
{
    /// <summary>
    /// Defines operations for persisting and retrieving TaxPayer entities.
    /// </summary>
    public interface ITaxPayerRepository
    {
        /// <summary>
        /// Retrieves all TaxPayer entities from the data store.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of all TaxPayers.</returns>
        Task<IEnumerable<TaxPayer>> GetAllAsync();

        /// <summary>
        /// Saves a TaxPayer entity to the data store. If a TaxPayer with the same tax number already exists, it will be updated.
        /// </summary>
        /// <param name="taxPayer">The TaxPayer to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(TaxPayer taxPayer);

        /// <summary>
        /// Deletes a TaxPayer entity from the data store by tax number.
        /// </summary>
        /// <param name="taxNumber">The tax number of the TaxPayer to delete.</param>
        /// <returns>A task representing the asynchronous operation, returning true if deleted, false if not found.</returns>
        Task<bool> DeleteAsync(string taxNumber);

        /// <summary>
        /// Retrieves a TaxPayer by their unique tax number.
        /// </summary>
        /// <param name="taxNumber">The tax number to search for.</param>
        /// <returns>A task representing the asynchronous operation, containing the TaxPayer if found, or null otherwise.</returns>
        Task<TaxPayer?> GetByTaxNumberAsync(string taxNumber);
    }
}
