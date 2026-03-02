namespace VergiNoDogrula.Data;

/// <summary>
/// Repository interface for accessing database metadata.
/// </summary>
public interface IDatabaseMetadataRepository
{
    /// <summary>
    /// Gets the last database update time in UTC.
    /// </summary>
    /// <returns>The last update time, or null if never updated.</returns>
    Task<DateTime?> GetLastUpdateTimeUtcAsync();

    /// <summary>
    /// Gets the last database update time in local time.
    /// </summary>
    /// <returns>The last update time in local time, or null if never updated.</returns>
    Task<DateTime?> GetLastUpdateTimeAsync();
}
