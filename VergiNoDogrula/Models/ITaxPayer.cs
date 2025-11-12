namespace VergiNoDogrula.Models;

/// <summary>
/// Defines the contract for an entity that is subject to taxation and can be identified by a title and
/// a tax number.
/// </summary>
public interface ITaxPayer
{
    string Title { get; set; }
    string TaxNumber { get; set; }
}