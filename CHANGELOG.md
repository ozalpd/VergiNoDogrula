# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.3.1] - 2026-03-13

### Fixed
- **DataGrid Scrolling in Main Window** - Fixed the taxpayer list not scrolling when the record count grows
  - Changed the DataGrid host row in `MainWindow.xaml` from `Auto` to star-sized (`*`) to provide a constrained viewport
  - Updated DataGrid placement to use the dedicated content row, enabling built-in vertical scrolling behavior

## [1.3.0] - 2026-03-13

### Added
- **Shared Localization Library** - Added `VergiNoDogrula.i18n` as a framework-agnostic localization project
  - Uses `.resx` resource files with generated public `Strings` class
  - Includes default English resources in `Strings.resx` and Turkish translations in `Strings.tr.resx`
  - Designed to be reusable from WPF, MAUI, and ASP.NET projects

### Changed
- **WPF UI Localization** - Localized WPF windows, dialogs, tooltips, labels, grid headers, status messages, and dialog messages
  - `MainWindow.xaml` and `MainWindow.xaml.cs` now read visible text from `Strings`
  - `TaxPayerCollectionVM` now uses localized status, confirmation, warning, and error strings
  - `AddTaxPayerDialog.xaml` and `AddTaxPayerDialog.xaml.cs` now use localized resources
  - `BackupListDialog.xaml` and `BackupListDialog.xaml.cs` now use localized resources
- **BackupListDialog Cleanup** - Renamed helper method from `OpenFolde` to `OpenFolder`

### Localization
- Migrated user-facing WPF text to shared resource keys in `Strings`
  - `MainWindow` now uses localized window title, search placeholder, labels, tooltips, grid headers, and empty-state text
  - `AddTaxPayerDialog` now uses localized dialog title, labels, and action buttons
  - `BackupListDialog` now uses localized dialog title, headers, tooltips, close button, status text, and error messages
  - `TaxPayerCollectionVM` now formats localized status, confirmation, warning, and backup messages

## [1.2.2] - 2026-03-04

### Changed
- **BackupListDialog UI** - Optimized layout by hiding empty status elements
  - `StatusContainer` is now collapsed when no new backups are detected
  - Prevents wasted vertical space when backup status notification is not needed
  - Improves dialog visual hierarchy and focus on backup file list

## [1.2.1] - 2026-03-04

### Fixed
- **Search Placeholder Visibility** - Fixed search box placeholder ("Ara..") logic to properly hide when focused OR contains text
  - Changed from conflicting triggers to `MultiDataTrigger` with proper conditions
  - Placeholder now only visible when SearchTextBox is NOT focused AND has no text
  - Prevents placeholder from reappearing when typing after losing focus

## [1.2.0] - 2026-03-03

### Added
- **Backup File List Dialog** - View all existing backup files with detailed information
  - `BackupListDialog` - Modal window displaying backup files in a DataGrid with three columns: File Name, Creation Date, and Size
  - `BackupFileInfo` model with auto-formatted file size (KB/MB)
  - `ShowBackupListCommand` to open the backup list dialog
  - New toolbar button with `file-earmark` icon positioned between Backup and About buttons
  - `GetBackupFilesAsync()` method in `IBackupService` and `DatabaseBackupService`
  - Files are ordered by creation time (newest first)
  - Empty state message displayed when no backup files exist
  - `BackupService` property exposed in `TaxPayerCollectionVM` for command binding

### Changed
- **IBackupService** - Added `GetBackupFilesAsync()` method to retrieve backup file list
- **TaxPayerCollectionVM** - Added `ShowBackupListCommand` and exposed `BackupService` for UI binding

## [1.1.5] - 2026-03-02

### Added
- **Window Position Persistence** - `WindowPosition` model for saving and restoring main window location, size, and multi-monitor awareness
  - Restored position is applied before window is displayed (via `SourceInitialized` event) to prevent visual jump
  - DPI-aware handling for multi-monitor setups with proper scaling
  - Detects primary screen and validates window bounds within available working areas

### Changed
- **Removed System.Windows.Forms Dependency** - Replaced `Screen.AllScreens` with native Windows API (`EnumDisplayMonitors`)
  - `WindowPosition.SetWindowPositions()` now uses P/Invoke for monitor enumeration
  - Eliminates external framework dependency while maintaining multi-monitor and DPI-aware functionality
  - Uses native `GetDpiForMonitor()` API for accurate DPI scaling across monitors
- **Window Initialization Flow** - Moved window position restoration from `Loaded` to `SourceInitialized` event
  - Prevents window visual jump when opening
  - Window now appears directly at the correct position

### Technical Details
- Added `WindowPosition` class with native Windows API interop (`user32.dll`, `shcore.dll`)
- Implemented `EnumerateMonitors()` using `EnumDisplayMonitors` and `GetMonitorInfo`
- Added `MonitorInfo` helper class encapsulating monitor metadata
- Enhanced `NativeMethods` with `MONITORINFOEX` struct and monitor enumeration delegates

## [1.1.4] - 2026-03-02

### Added
- **Configurable Backup Retention** - `MaxBackupFiles` setting in `AppSettings`
  - Allows users to configure the number of recent backups to keep (default: 10, minimum: 10)
  - Exposed in `appsettings.json` for end-user customization
  - Replaces hardcoded `keepCount` parameter in `CleanupOldBackupsAsync`

### Changed
- **Auto-Backup Cleanup** - `AutoBackupHelper.RunAsync()` now calls `CleanupOldBackupsAsync()`
  - Old backup files are automatically cleaned up when auto-backup runs
  - Uses `MaxBackupFiles` setting from `AppSettings` to determine retention count
- **IBackupService Interface** - `CleanupOldBackupsAsync()` parameter changed from `keepCount` to use settings
  - Parameter removed; method now reads retention count from `AppSettings.MaxBackupFiles`
  - Simplifies API and ensures consistent retention policy across manual and auto backups

### Technical Details
- Updated `DatabaseBackupService.CleanupOldBackupsAsync()` to read `_settings.MaxBackupFiles`
- `AutoBackupHelper.RunAsync()` now performs cleanup after successful backup creation

## [1.1.3] - 2026-03-02

### Fixed
- Improved shutdown backup flow by triggering final backup from `MainWindow.Closing` with deferred close to avoid window-closing re-entrancy errors

### Changed
- Updated application version metadata to `1.1.3`

## [1.1.2] - 2026-03-02

### Fixed
- Fixed visibility issue of Backup (Yedekle) and Delete (Sil) buttons in the toolbar

## [1.1.1] - 2026-03-02

### Added
- Periodic auto-backup timer
  - Automatically triggers backup at configured intervals (default: 10 minutes)
  - Runs in background using `System.Timers.Timer`
  - Only executes when `AutoBackupEnabled` is true and backup is due
  - Respects smart deduplication (only backs up if database was modified)

## [1.1.0] - 2026-03-02

### Added
- **Database Backup Service** - Automated and manual database backup functionality
  - `IBackupService` interface and `DatabaseBackupService` implementation
  - Uses SQLite's `BackupDatabase()` API for consistent snapshots while database is in use
  - ZIP compression with `CompressionLevel.Optimal` for efficient storage
  - Timestamped backup files: `taxpayers_backup_yyyyMMdd_HHmms.zip`
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

## [1.0.0] - 2026-03-01

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
