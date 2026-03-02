# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2025-01-16

### Added
- Periodic auto-backup timer
  - Automatically triggers backup at configured intervals (default: 10 minutes)
  - Runs in background using `System.Timers.Timer`
  - Only executes when `AutoBackupEnabled` is true and backup is due
  - Respects smart deduplication (only backs up if database was modified)

## [1.1.0] - 2025-01-16

### Added
- **Database Backup Service** - Automated and manual database backup functionality
  - `IBackupService` interface and `DatabaseBackupService` implementation
  - Uses SQLite's `BackupDatabase()` API for consistent snapshots while database is in use
  - ZIP compression with `CompressionLevel.Optimal` for efficient storage
  - Timestamped backup files: `taxpayers_backup_yyyyMMdd_HHmmss.zip`
- **Smart Backup Deduplication** - Only backs up when database has been modified
  - Compares `LastUpdateUtc` from database vs `LastBackupTimeUtc` from settings
  - Prevents unnecessary duplicate backups
- **Auto-Backup on Startup** - Configurable automatic backup
  - Checks if backup is due based on `AutoBackupIntervalMinutes` (default: 10 minutes)
  - Only creates backup if database was modified since last backup
  - Configurable via `appsettings.json`
- **Manual Backup Command** - Blue archive button in toolbar
  - `BackupDatabaseCommand` for on-demand backups
  - Bound to UI button with Bootstrap Icons `archive` icon
- **Backup Retention Policy** - Auto-cleanup of old backups
  - Keeps last 10 backups by default
  - `CleanupOldBackupsAsync` deletes older files automatically
- **Database Metadata Repository** - Separate repository for metadata access
  - `IDatabaseMetadataRepository` interface
  - `SqliteDatabaseMetadataRepository` implementation
  - UTC-aware timestamp handling with `GetLastUpdateTimeUtcAsync()` and `GetLastUpdateTimeAsync()`
- **UTC Timestamp Storage** - Consistent timezone handling
  - All timestamps stored in UTC (`LastBackupTimeUtc`, `LastUpdateUtc`)
  - Prevents timezone/DST issues
  - Proper parsing with `DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal`
- **About Dialog** - Application information dialog
  - Displays version, copyright, description
  - GitHub repository link
  - `ShowAboutCommand` and info-circle button in toolbar
- **Version Information** - Assembly versioning
  - Version 1.1.0 in all project files
  - `AppVersion` helper class for runtime access
  - Company, product, and copyright metadata

### Changed
- **AppSettings** - Added backup-related properties
  - `LastBackupTimeUtc` (renamed from `LastBackupTime`, now stores UTC)
  - `AutoBackupEnabled` (boolean flag)
  - `AutoBackupIntervalMinutes` (default: 60)
  - `BackupFolder` (default: `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\`)
- **App.OnStartup** - Added auto-backup check
  - Instantiates `DatabaseBackupService` and `SqliteDatabaseMetadataRepository`
  - Performs backup if conditions are met (enabled, interval passed, database modified)
- **TaxPayerCollectionVM** - Integrated backup service
  - Added `IBackupService` dependency
  - `BackupDatabaseCommand` property
  - `ShowAboutCommand` property
  - `CreateBackupAsync()` method with success/failure handling
- **MainWindow.xaml** - Added buttons to toolbar
  - Blue archive icon (`archive`) for backup
  - Blue info-circle icon for About dialog
  - Tooltips: "Veritabanını yedeklemek için tıklayın" and "Hakkında"
- **Documentation** - Updated README.md and copilot-instructions.md
  - Added backup usage instructions
  - Added backup configuration examples
  - Updated solution structure and architecture sections

### Technical Details
- **SQLite Backup API** - Uses `SqliteConnection.BackupDatabase()`
  - Creates consistent snapshot without locking the source database
  - Temporary database file created in system temp folder
  - `SqliteConnection.ClearAllPools()` called to release file locks before zipping
- **Backup Process Flow**:
  1. Source DB → SQLite BACKUP API → Temp DB
  2. Temp DB → ZIP Compression → Final Backup
  3. Cleanup temp file
  4. Update `LastBackupTimeUtc` and save settings
  5. Run `CleanupOldBackupsAsync` to enforce retention policy
- **Error Handling** - Graceful handling of backup failures
  - Catches `IOException`, `UnauthorizedAccessException`, `SqliteException`
  - Returns `null` on failure instead of throwing
  - `finally` block ensures temp file cleanup

### Default Locations
- Database: `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db`
- Settings: `%APPDATA%\VergiNoDogrula\appsettings.json`
- Backups: `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\`

## [1.0.0] - 2025-01-15

### Added
- Turkish tax number validation (VKN and TCKN)
- Taxpayer management (CRUD operations)
- Real-time search (numeric for tax number, text for title)
- SQLite persistence with automatic schema creation
- WPF UI with MVVM pattern
- Turkish culture support for proper alphabetical sorting
- Input validation with real-time error feedback
- Clipboard integration for copying tax numbers
- Bootstrap Icons integration
- Empty state UI for DataGrid
