namespace VergiNoDogrula.WPF.Services;

/// <summary>
/// Service for managing database backups.
/// </summary>
internal interface IBackupService
{
    /// <summary>
    /// Creates a backup of the database.
    /// </summary>
    /// <param name="connectionString">The connection string to the source database.</param>
    /// <returns>The full path to the created backup file, or null if backup failed.</returns>
    Task<string?> CreateBackupAsync(string connectionString);

    /// <summary>
    /// Deletes old backup files based on retention policy.
    /// </summary>
    /// <param name="keepCount">Number of most recent backups to keep.</param>
    /// <returns>Number of files deleted.</returns>
    Task<int> CleanupOldBackupsAsync(int keepCount = 10);

    /// <summary>
    /// Checks if auto-backup is due based on settings.
    /// </summary>
    /// <returns>True if backup should be performed, false otherwise.</returns>
    bool IsBackupDue();
}
