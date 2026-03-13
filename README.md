# VergiNoDogrula

**Version 1.3.0**

A WPF desktop application for validating Turkish tax identification numbers — **Vergi Kimlik Numarası (VKN)** and **TC Kimlik Numarası (TCKN)**.

## Third-Party Assets

- This project uses icon paths derived from **Bootstrap Icons**.
- Source: `https://icons.getbootstrap.com`
- Project repository: `https://github.com/twbs/icons`
- Version: `v1.13.1`
- License: `MIT`
- SPDX License Identifier: `MIT`

## Features

- **VKN Validation** — Validates 10-digit Turkish corporate tax identification numbers using the official checksum algorithm.
- **TCKN Validation** — Validates 11-digit Turkish national identity numbers using the official checksum algorithm.
- **Taxpayer Management** — Add, save, and delete taxpayer records with title and tax number.
- **Localization Support** — Shared `VergiNoDogrula.i18n` resource library with reusable `.resx` resources for WPF, MAUI, and ASP.NET.
- **Localized WPF UI** — Main window, dialogs, labels, tooltips, status text, and user-facing messages are localized through the shared `Strings` resource class.
- **Real-time Search** — Filter taxpayers by tax number (numeric search) or title (text search) with instant results.
- **Window Position Persistence** — Automatically saves and restores the main window's position, size, and screen on subsequent launches; multi-monitor aware with DPI scaling.
- **Automatic Backup** — Smart backup on startup: only backs up when database has changed and backup interval has passed.
- **Manual Backup** — One-click backup via toolbar button; creates compressed ZIP files.
- **Backup Retention** — Auto-cleanup keeps last N backups (configurable via `MaxBackupFiles` in settings, default: 10), preventing disk space bloat.
- **Clipboard Integration** — Copy taxpayer tax numbers to clipboard with a single click.
- **SQLite Persistence** — Taxpayer records are stored locally in a SQLite database at `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db`.
- **Metadata Tracking** — CUD operations update `DatabaseMetadata.LastUpdateUtc`; repository exposes `LastUpdateTime` in local time.
- **App Settings Persistence** — Settings are stored in `%APPDATA%\VergiNoDogrula\appsettings.json` and persisted on application exit.
- **Real-time Error Feedback** — Inline validation errors displayed via red borders and descriptive messages.

## Screenshots

The main window provides a search box with placeholder text for filtering records, text fields for entering a tax number and title, **Ekle** (Add), **Kaydet** (Save), **Sil** (Delete), and copy buttons, and a DataGrid listing all entered taxpayer records with an empty-state message when no records exist.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Windows (WPF requires a Windows host)

## Getting Started

### Clone the repository

```bash
git clone https://github.com/ozalpd/VergiNoDogrula.git
cd VergiNoDogrula
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project VergiNoDogrula.WPF
```

Or open `VergiNoDogrula.sln` in Visual Studio and press **F5** with `VergiNoDogrula.WPF` set as the startup project.

## Localization

Localization is implemented with the shared `VergiNoDogrula.i18n` class library.

- Default resources live in `VergiNoDogrula.i18n/Strings.resx`
- Turkish translations live in `VergiNoDogrula.i18n/Strings.tr.resx`
- The generated public `Strings` class is consumed directly from WPF XAML and C#
- The localization library is framework-agnostic and can be reused from WPF, MAUI, and ASP.NET projects

### Example usage

```csharp
using VergiNoDogrula.i18n;

var title = Strings.MainWindowTitle;
var message = string.Format(Strings.TaxPayersLoadedFormat, 10);
```

## Usage

### Adding a Taxpayer
1. Click the **Ekle** (Add/Plus icon) button
2. Enter a valid 10-digit VKN or 11-digit TCKN
3. Enter the taxpayer title/name
4. Click OK (the button is disabled until both fields are valid and the tax number is unique)

### Searching
- Type numbers to search by tax number
- Type text to search by title
- The search filters in real-time as you type
- Clear the search box to show all records

### Editing
1. Select a taxpayer from the DataGrid
2. Modify the tax number or title in the text fields
3. Click **Kaydet** (Save/Floppy disk icon) to persist changes

### Deleting
1. Select a taxpayer from the DataGrid
2. Click **Sil** (Delete/Trash icon)
3. Confirm the deletion

### Copy Tax Number
1. Select a taxpayer from the DataGrid
2. The tax number is now available via the copy button or can be copied from the text field

### Backup Database
- **Manual backup**: Click the blue archive icon (Yedekle) in the toolbar
- **View backup list**: Click the file icon next to the backup button to view all backup files with creation dates and sizes
- **Auto-backup**: Runs on application startup if:
  - Auto-backup is enabled (`AutoBackupEnabled = true`)
  - Backup interval has passed (default: 10 minutes)
  - Database was modified since last backup
- **Backup location**: `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\` (default)
- **Format**: Timestamped ZIP files (`dbbackup-yyyy-MM-dd-HHmm.zip`)
- **Retention**: Configurable via `MaxBackupFiles` setting (default: 10, minimum: 10); older backups are auto-deleted

### Configure Backup Settings
Edit `%APPDATA%\VergiNoDogrula\appsettings.json`:
```json
{
  "AutoBackupEnabled": true,
  "AutoBackupIntervalMinutes": 60,
  "BackupFolder": "C:\\path\\to\\custom\\backup\\folder",
  "MaxBackupFiles": 10,
  "LastBackupTimeUtc": "2024-01-15T10:30:00.000Z"
}
```

### Restore from Backup
1. Close the application
2. Extract the `.db` file from a backup ZIP
3. Replace `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db` with the extracted file
4. Restart the application

## Solution Structure

```
VergiNoDogrula/
├── VergiNoDogrula/                   # Business-logic class library (net10.0)
│   ├── Models/
│   │   ├── ITaxPayer.cs              # Interface: Title, TaxNumber
│   │   └── TaxPayer.cs              # Concrete model with setter validation
│   └── ValidateExtensions.cs        # Extension methods for VKN / TCKN validation
├── VergiNoDogrula.Data/              # Data-access class library (net10.0)
│   ├── ITaxPayerRepository.cs        # Repository interface for CRUD operations
│   ├── SqliteTaxPayerRepository.cs   # SQLite-backed implementation + DatabaseMetadata tracking
│   ├── IDatabaseMetadataRepository.cs # Repository interface for database metadata access
│   └── SqliteDatabaseMetadataRepository.cs # UTC-aware metadata reader
├── VergiNoDogrula.i18n/              # Shared localization class library (net10.0)
│   ├── Strings.resx                  # Default resource file
│   ├── Strings.tr.resx               # Turkish translations
│   └── Strings.Designer.cs           # Generated public strongly-typed resource accessors
├── VergiNoDogrula.WPF/              # WPF presentation layer (net10.0-windows)
│   ├── Commands/
│   │   ├── AbstractCommand.cs       # Base ICommand implementation
│   │   ├── AddTaxPayerCommand.cs    # Opens AddTaxPayerDialog to create a new taxpayer
│   │   ├── CopyTaxNumberCommand.cs  # Copies selected taxpayer's tax number to clipboard
│   │   ├── SaveTaxPayerCommand.cs   # Persists the selected taxpayer to SQLite
│   │   ├── DeleteTaxPayerCommand.cs # Deletes the selected taxpayer from SQLite
│   │   ├── BackupDatabaseCommand.cs # Manual backup command
│   │   └── ShowBackupListCommand.cs # Opens backup list dialog
│   ├── Dialogs/
│   │   ├── AddTaxPayerDialog.xaml   # Dialog window for creating a new taxpayer
│   │   ├── AddTaxPayerDialog.xaml.cs # Modal dialog: numeric-only tax number input, real-time validation with duplicate check, auto-focus on tax number, disabled OK button until valid
│   │   ├── BackupListDialog.xaml    # Dialog window for viewing backup files
│   │   └── BackupListDialog.xaml.cs # Displays DataGrid with file name, creation date, and size
│   ├── Models/
│   │   ├── AppSettings.cs           # App settings singleton (db path, backup settings, save/load)
│   │   ├── BackupFileInfo.cs        # Backup file metadata (FileName, CreatedDate, SizeInBytes, FormattedSize)
│   │   └── WindowPosition.cs        # Window position persistence (multi-monitor aware, DPI-aware, restored before window display)
│   ├── Services/
│   │   ├── IBackupService.cs         # Backup service interface
│   │   └── DatabaseBackupService.cs  # SQLite backup using BackupDatabase() API + ZIP compression
│   ├── ViewModels/
│   │   ├── AbstractViewModel.cs     # INotifyPropertyChanged base
│   │   ├── AbstractCollectionVM.cs  # Generic collection ViewModel base with search support
│   │   ├── AbstractDataErrorInfoVM.cs # INotifyDataErrorInfo base
│   │   ├── TaxPayerVM.cs            # ViewModel wrapping TaxPayer model
│   │   └── TaxPayerCollectionVM.cs  # ObservableCollection + commands + repository + search logic + backup service
│   ├── Resources/
│   │   ├── BootstrapIcons.xaml      # Bootstrap Icons geometry resources
│   │   └── Styles.xaml              # Shared WPF styles
│   ├── MainWindow.xaml / .xaml.cs   # Main application window
│   └── App.xaml / .xaml.cs          # Application entry point, resource merge, auto-backup check, settings save on exit
└── VergiNoDogrula.sln
```

## Architecture

The solution follows an **N-tier** architecture with strict separation between business logic, data access, and presentation.

### Business-Logic Layer (`VergiNoDogrula`)

A plain .NET class library with **no UI or data-access dependencies**. Contains:

- **`ITaxPayer`** — Contract defining `Title` and `TaxNumber` properties.
- **`TaxPayer`** — Concrete model. Setters guard against invalid input by throwing `ArgumentNullException` / `ArgumentException`. Implements `IEquatable<TaxPayer>` based on `TaxNumber`.
- **`ValidateExtensions`** — Pure, thread-safe extension methods implementing the official Turkish VKN (10-digit) and TCKN (11-digit) checksum algorithms.

### Data-Access Layer (`VergiNoDogrula.Data`)

A dedicated .NET class library for persistence. Contains:

- **`ITaxPayerRepository`** — Repository interface defining async CRUD operations (`GetAllAsync`, `SaveAsync`, `DeleteAsync`, `GetByTaxNumberAsync`).
- **`SqliteTaxPayerRepository`** — SQLite-backed implementation. Auto-creates the database and `TaxPayers` table on first use. Uses UPSERT semantics for save operations and tracks CUD timestamps in `DatabaseMetadata`.
- **`IDatabaseMetadataRepository`** — Repository interface for accessing database metadata (last update timestamp).
- **`SqliteDatabaseMetadataRepository`** — UTC-aware metadata reader; provides `GetLastUpdateTimeUtcAsync()` and `GetLastUpdateTimeAsync()` (local time conversion).
- **`Microsoft.Data.Sqlite`** package reference — Kept in this project so the core library stays lean and persistence-agnostic.

### Localization Layer (`VergiNoDogrula.i18n`)

A framework-agnostic .NET class library containing shared localized resources.

- **`Strings.resx`** — Default language resource file.
- **`Strings.tr.resx`** — Turkish translation resource file.
- **`Strings.Designer.cs`** — Generated public strongly-typed resource class used from XAML and C#.
- The library is designed to be reusable from WPF, MAUI, and ASP.NET projects.

### Presentation Layer (`VergiNoDogrula.WPF`)

A WPF application following the **MVVM** pattern:

- **ViewModels** delegate validation to the model layer, translating exceptions into `INotifyDataErrorInfo` entries for UI binding.
- **Commands** inherit from `AbstractCommand` and contain minimal logic. `AddTaxPayerCommand` opens `AddTaxPayerDialog` for new taxpayer entry. `SaveTaxPayerCommand` and `DeleteTaxPayerCommand` bridge async repository calls via `async void Execute`. `BackupDatabaseCommand` triggers manual backup.
- **Services** encapsulate application-level operations. `DatabaseBackupService` uses SQLite's `BackupDatabase()` API for consistent snapshots while the database is in use, then compresses to ZIP.
- **`TaxPayerCollectionVM`** integrates the repository for data loading, saving, deletion, backup operations, and localized status/error messaging. It subscribes to `SelectedItem` changes and `ErrorsChanged` to refresh command states.
- **`WindowPosition`** model persists the main window's location and size. Detects the appropriate monitor using native Windows API, applies DPI scaling, and restores position before the window is displayed.
- **Styles** are defined in `Resources/Styles.xaml` and merged via `App.xaml`.
- **`AppSettings`** is loaded as a singleton and persisted at application shutdown. Contains backup configuration (`AutoBackupEnabled`, `AutoBackupIntervalMinutes`, `BackupFolder`, `LastBackupTimeUtc`).
- **Auto-backup** runs on application startup if enabled, interval has passed, and database was modified since last backup.
- **Data** is loaded asynchronously on startup via `MainWindow` initialization.
- **Localization** in XAML uses `x:Static i18n:Strings.ResourceKey`, while code-behind and ViewModels use `Strings.ResourceKey` directly.

#### UI Features

- **Smart Search** — Numeric input filters by tax number, text input filters by title (case-insensitive).
- **Localized UI Text** — Window titles, labels, tooltips, dialog buttons, grid headers, empty-state text, status text, and message box content are loaded from shared resources.
- **Placeholder Text** — Search box shows localized hint text that disappears on focus or when text is entered.
- **Empty State** — DataGrid shows a localized empty-state message when the collection is empty.
- **Status Bar** — Displays localized operation feedback (success/error messages, record counts).
- **Icon Buttons** — Uses Bootstrap Icons for Add (plus-circle), Save (floppy), Delete (trash3), Backup (archive-blue) operations.
- **Real-time Validation** — Input fields show validation errors with red borders and descriptive messages.
- **Clipboard Support** — Copy taxpayer tax numbers to clipboard with a dedicated copy command.
- **Backup Integration** — Manual backup button and auto-backup on startup with smart deduplication.
- **Window Persistence** — Main window position, size, and screen are saved on application exit and restored on next launch; multi-monitor aware.

## Validation Rules

| Type | Length | Algorithm |
|---|---|---|
| **VKN** (Corporate Tax ID) | 10 digits | Weighted modular arithmetic checksum |
| **TCKN** (National ID) | 11 digits | Two-stage modular arithmetic checksum; first digit must not be zero |

Both algorithms are implemented in `ValidateExtensions.cs`. The `IsValidTaxNumber` method automatically dispatches to the correct algorithm based on the length of the input string.

## Data Locations

| Item | Path |
|------|------|
| SQLite database | `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db` |
| App settings | `%APPDATA%\VergiNoDogrula\appsettings.json` |
| Backups | `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\` (default) |

> **Note:** The application uses Turkish culture (`tr-TR`) for sorting taxpayer titles to ensure correct alphabetical ordering of Turkish characters (ç, ğ, ı, ö, ş, ü).

## Technology Stack

| Component | Technology |
|---|---|
| Target Framework | .NET 10 |
| UI Framework | WPF |
| Language | C# 14.0 |
| Local Database | SQLite via `Microsoft.Data.Sqlite` |
| Localization | Shared `.resx` resources via `VergiNoDogrula.i18n` |
| Build System | SDK-style projects, `dotnet` CLI |

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Commit your changes (`git commit -m "Add my feature"`).
4. Push to the branch (`git push origin feature/my-feature`).
5. Open a Pull Request.

Please maintain the existing separation of concerns — validation logic belongs in `VergiNoDogrula`, data-access logic belongs in `VergiNoDogrula.Data`, localization resources belong in `VergiNoDogrula.i18n`, and UI logic belongs in `VergiNoDogrula.WPF`.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
