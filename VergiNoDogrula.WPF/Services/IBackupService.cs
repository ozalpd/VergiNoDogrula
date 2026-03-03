using VergiNoDogrula.WPF.Models;

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
    /// <returns>Number of files deleted.</returns>
    Task<int> CleanupOldBackupsAsync();

    /// <summary>
    /// Checks if auto-backup is due based on settings.
    /// </summary>
    /// <returns>True if backup should be performed, false otherwise.</returns>
    bool IsBackupDue();

    /// <summary>
    /// Gets the list of backup files in the backup folder.
    /// </summary>
    /// <returns>List of backup file information ordered by creation time (newest first).</returns>
    Task<List<BackupFileInfo>> GetBackupFilesAsync();

    /// <summary>
    /// Checks if any backup files exist that are newer than the specified UTC time.
    /// </summary>
    /// <param name="lastBackupTimeUtc">The UTC timestamp to compare against backup file creation times.</param>
    /// <returns>True if one or more backup files were created after the specified time; otherwise, false.</returns>
    Task<bool> HasNewerBackupThanAsync(DateTime lastBackupTimeUtc);
}
