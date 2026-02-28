using Microsoft.Data.Sqlite;
using VergiNoDogrula.Models;

namespace VergiNoDogrula.Data
{
    /// <summary>
    /// SQLite-based implementation of ITaxPayerRepository.
    /// </summary>
    public class SqliteTaxPayerRepository : ITaxPayerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Gets the last data update time in local time.
        /// </summary>
        public DateTime? LastUpdateTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of SqliteTaxPayerRepository with the specified database path.
        /// </summary>
        /// <param name="databasePath">The full path to the SQLite database file.</param>
        public SqliteTaxPayerRepository(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TaxPayers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    TaxNumber TEXT NOT NULL UNIQUE
                )";
            command.ExecuteNonQuery();

            var metadataCommand = connection.CreateCommand();
            metadataCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS DatabaseMetadata (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1),
                    LastUpdateUtc TEXT NOT NULL
                );

                INSERT INTO DatabaseMetadata (Id, LastUpdateUtc)
                VALUES (1, @lastUpdateUtc)
                ON CONFLICT(Id) DO NOTHING;";
            metadataCommand.Parameters.AddWithValue("@lastUpdateUtc", string.Empty);
            metadataCommand.ExecuteNonQuery();

            var readMetadataCommand = connection.CreateCommand();
            readMetadataCommand.CommandText = "SELECT LastUpdateUtc FROM DatabaseMetadata WHERE Id = 1";
            var lastUpdateUtc = readMetadataCommand.ExecuteScalar() as string;
            LastUpdateTime = ToLocalDateTime(lastUpdateUtc);
        }

        public async Task<IEnumerable<TaxPayer>> GetAllAsync()
        {
            var taxPayers = new List<TaxPayer>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Title, TaxNumber FROM TaxPayers ORDER BY Id";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var title = reader.GetString(0);
                var taxNumber = reader.GetString(1);
                taxPayers.Add(new TaxPayer(title, taxNumber));
            }

            return taxPayers;
        }

        public async Task SaveAsync(TaxPayer taxPayer)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TaxPayers (Title, TaxNumber) VALUES (@title, @taxNumber)
                ON CONFLICT(TaxNumber) DO UPDATE SET Title = @title";
            command.Parameters.AddWithValue("@title", taxPayer.Title);
            command.Parameters.AddWithValue("@taxNumber", taxPayer.TaxNumber);

            await command.ExecuteNonQueryAsync();
            await LastUpdateUtcAsync(connection);
        }

        public async Task<bool> DeleteAsync(string taxNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TaxPayers WHERE TaxNumber = @taxNumber";
            command.Parameters.AddWithValue("@taxNumber", taxNumber);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            await LastUpdateUtcAsync(connection);
            return rowsAffected > 0;
        }

        public async Task<TaxPayer?> GetByTaxNumberAsync(string taxNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Title, TaxNumber FROM TaxPayers WHERE TaxNumber = @taxNumber";
            command.Parameters.AddWithValue("@taxNumber", taxNumber);

            using var reader = await command.ExecuteReaderAsync();
            TaxPayer? result = null;
            if (await reader.ReadAsync())
            {
                var title = reader.GetString(0);
                var taxNumberValue = reader.GetString(1);
                result = new TaxPayer(title, taxNumberValue);
            }

            return result;
        }

        private async Task LastUpdateUtcAsync(SqliteConnection connection)
        {
            var nowUtc = DateTimeOffset.UtcNow;

            var metadataCommand = connection.CreateCommand();
            metadataCommand.CommandText = @"
                UPDATE DatabaseMetadata
                SET LastUpdateUtc = @lastUpdateUtc
                WHERE Id = 1";
            metadataCommand.Parameters.AddWithValue("@lastUpdateUtc", nowUtc.ToString("O"));

            await metadataCommand.ExecuteNonQueryAsync();
            LastUpdateTime = nowUtc.ToLocalTime().DateTime;
        }

        private static DateTime? ToLocalDateTime(string? utcValue)
        {
            DateTime? dateTime = null;
            if (DateTimeOffset.TryParse(utcValue, out var parsedUtc))
                dateTime = parsedUtc.ToLocalTime().DateTime;

            return dateTime;
        }
    }
}
