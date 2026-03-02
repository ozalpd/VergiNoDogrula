using System.Globalization;
using Microsoft.Data.Sqlite;

namespace VergiNoDogrula.Data;

/// <summary>
/// SQLite-based implementation of IDatabaseMetadataRepository.
/// </summary>
public class SqliteDatabaseMetadataRepository : IDatabaseMetadataRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance with the specified database path.
    /// </summary>
    /// <param name="databasePath">The full path to the SQLite database file.</param>
    public SqliteDatabaseMetadataRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    /// <summary>
    /// Gets the last database update time in UTC.
    /// </summary>
    /// <returns>The last update time, or null if never updated.</returns>
    public async Task<DateTime?> GetLastUpdateTimeUtcAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT LastUpdateUtc FROM DatabaseMetadata WHERE Id = 1";
        
        var result = await command.ExecuteScalarAsync() as string;
        
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        if (DateTime.TryParse(result, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utcTime))
        {
            return utcTime;
        }

        return null;
    }

    /// <summary>
    /// Gets the last database update time in local time.
    /// </summary>
    /// <returns>The last update time in local time, or null if never updated.</returns>
    public async Task<DateTime?> GetLastUpdateTimeAsync()
    {
        var utcTime = await GetLastUpdateTimeUtcAsync();
        return utcTime?.ToLocalTime();
    }
}
