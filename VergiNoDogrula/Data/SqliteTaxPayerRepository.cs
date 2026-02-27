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
        }

        public async Task<bool> DeleteAsync(string taxNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TaxPayers WHERE TaxNumber = @taxNumber";
            command.Parameters.AddWithValue("@taxNumber", taxNumber);

            var rowsAffected = await command.ExecuteNonQueryAsync();
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
            if (await reader.ReadAsync())
            {
                var title = reader.GetString(0);
                var taxNumberValue = reader.GetString(1);
                return new TaxPayer(title, taxNumberValue);
            }

            return null;
        }
    }
}
