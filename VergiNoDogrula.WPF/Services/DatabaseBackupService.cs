using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
using VergiNoDogrula.WPF.Models;

namespace VergiNoDogrula.WPF.Services;

/// <summary>
/// Service for creating and managing database backups.
/// </summary>
internal class DatabaseBackupService : IBackupService
{
    private readonly AppSettings _settings;

    public DatabaseBackupService(AppSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Creates a timestamped backup of the database using SQLite's BACKUP API.
    /// </summary>
    /// <param name="connectionString">The connection string to the source database.</param>
    /// <returns>The full path to the created backup file, or null if backup failed.</returns>
    public async Task<string?> CreateBackupAsync(string connectionString)
    {
        string? tempDbPath = null;
        try
        {
            var sourcePath = _settings.DatabasePath;
            if (!File.Exists(sourcePath))
            {
                return null;
            }

            var backupFolder = _settings.BackupFolder;
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"taxpayers_backup_{timestamp}.zip";
            var backupPath = Path.Combine(backupFolder, backupFileName);

            tempDbPath = Path.Combine(Path.GetTempPath(), $"taxpayers_temp_{timestamp}.db");

            await Task.Run(() =>
            {
                using (var sourceConnection = new SqliteConnection(connectionString))
                {
                    sourceConnection.Open();

                    using (var backupConnection = new SqliteConnection($"Data Source={tempDbPath}"))
                    {
                        backupConnection.Open();
                        sourceConnection.BackupDatabase(backupConnection);
                    }
                }

                SqliteConnection.ClearAllPools();
            });

            await Task.Run(() =>
            {
                using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(tempDbPath, Path.GetFileName(sourcePath), CompressionLevel.Optimal);
            });

            if (File.Exists(tempDbPath))
            {
                File.Delete(tempDbPath);
            }

            _settings.LastBackupTimeUtc = DateTime.UtcNow;
            _settings.Save();

            return backupPath;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (SqliteException)
        {
            return null;
        }
        finally
        {
            if (tempDbPath != null && File.Exists(tempDbPath))
            {
                try
                {
                    SqliteConnection.ClearAllPools();
                    File.Delete(tempDbPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    /// <summary>
    /// Deletes old backup files, keeping only the most recent ones.
    /// </summary>
    /// <param name="keepCount">Number of most recent backups to keep.</param>
    /// <returns>Number of files deleted.</returns>
    public async Task<int> CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backupFolder = _settings.BackupFolder;
            if (!Directory.Exists(backupFolder))
            {
                return 0;
            }

            var backupFiles = Directory.GetFiles(backupFolder, "taxpayers_backup_*.zip")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            if (backupFiles.Count <= keepCount)
            {
                return 0;
            }

            var filesToDelete = backupFiles.Skip(keepCount).ToList();
            var deletedCount = 0;

            await Task.Run(() =>
            {
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        deletedCount++;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            });

            return deletedCount;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Checks if auto-backup is due based on settings.
    /// </summary>
    /// <returns>True if backup should be performed, false otherwise.</returns>
    public bool IsBackupDue()
    {
        if (!_settings.AutoBackupEnabled)
        {
            return false;
        }

        if (_settings.LastBackupTimeUtc is null)
        {
            return true;
        }

        var intervalMinutes = _settings.AutoBackupIntervalMinutes;
        var nextBackupTime = _settings.LastBackupTimeUtc.Value.AddMinutes(intervalMinutes);

        return DateTime.UtcNow >= nextBackupTime;
    }
}
