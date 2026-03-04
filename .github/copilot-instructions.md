# AI Coding Agent Instructions for VergiNoDogrula

This document provides guidance for AI coding agents working on the `VergiNoDogrula` codebase.

## Technology Stack

| Component | Technology |
|---|---|
| Target Framework | .NET 10 |
| UI Framework | WPF (`net10.0-windows`) |
| Language | C# 14.0 |
| Build System | SDK-style projects, `dotnet` CLI |

All projects enable `ImplicitUsings` and `Nullable`. All new code **must** be nullable-aware and avoid adding explicit `using` directives that are already covered by implicit usings.

## Solution Structure

```
VergiNoDogrula/                       # Root / solution directory
├── .github/
│   └── copilot-instructions.md       # This file
├── LICENSE                           # MIT License
├── VergiNoDogrula/                   # Business-logic class library (net10.0)
│   ├── Models/
│   │   ├── ITaxPayer.cs              # Interface: Title, TaxNumber
│   │   └── TaxPayer.cs              # Concrete model with property-level validation
│   └── ValidateExtensions.cs        # Extension methods: IsValidVKN, IsValidTCKN, IsValidTaxNumber, IsSameTaxNumbers
├── VergiNoDogrula.Data/              # Data-access class library (net10.0)
│   ├── ITaxPayerRepository.cs        # Repository interface: GetAll, Save, Delete, GetByTaxNumber
│   ├── SqliteTaxPayerRepository.cs   # SQLite implementation of ITaxPayerRepository + DatabaseMetadata tracking
│   ├── IDatabaseMetadataRepository.cs # Repository interface for accessing `DatabaseMetadata` table (last update timestamp).
│   └── SqliteDatabaseMetadataRepository.cs # UTC-aware metadata reader; reads `LastUpdateUtc` from database; provides `GetLastUpdateTimeUtcAsync()` and `GetLastUpdateTimeAsync()` (local time conversion).
├── VergiNoDogrula.WPF/              # WPF presentation layer (net10.0-windows)
│   ├── Commands/
│   │   ├── AbstractCommand.cs       # Base ICommand implementation with public RaiseCanExecuteChanged
│   │   ├── AddTaxPayerCommand.cs    # Opens `AddTaxPayerDialog` to create a new `TaxPayerVM`. Passes `TaxPayerCollection` to the dialog for duplicate tax number validation. On success, adds the new taxpayer to the collection and selects it.
│   │   ├── CopyTaxNumberCommand.cs  # Copies the selected taxpayer's tax number to clipboard. Enabled only when `SelectedItem` is non-null.
│   │   ├── SaveTaxPayerCommand.cs   # Validates then persists the selected TaxPayerVM to SQLite
│   │   ├── DeleteTaxPayerCommand.cs # Deletes the selected TaxPayerVM from SQLite with confirmation
│   │   └── BackupDatabaseCommand.cs # Manual backup command; creates timestamped ZIP backup
│   ├── Dialogs/
│   │   ├── AddTaxPayerDialog.xaml   # Dialog window for creating a new taxpayer
│   │   └── AddTaxPayerDialog.xaml.cs # Code-behind with validation and OK button state management
│   ├── Models/
│   │   ├── AppSettings.cs           # App-level settings (db path, backup folder/interval, LastBackupTimeUtc [UTC], singleton load/save)
│   │   └── WindowPosition.cs        # Window position persistence; uses native Windows API for multi-monitor support + DPI scaling
│   ├── Services/
│   │   ├── IBackupService.cs         # Backup service interface: CreateBackupAsync, CleanupOldBackupsAsync, IsBackupDue
│   │   └── DatabaseBackupService.cs  # SQLite backup implementation using BackupDatabase() API + ZIP compression
│   ├── ViewModels/
│   │   ├── AbstractViewModel.cs     # INotifyPropertyChanged base
│   │   ├── AbstractCollectionVM.cs  # Generic base for collection ViewModels; provides `Collection`, `CollectionFiltered`, `SelectedItem`, `SearchString` properties and abstract hooks for search/selection lifecycle.
│   │   ├── AbstractDataErrorInfoVM.cs # INotifyDataErrorInfo base (error dictionary)
│   │   ├── TaxPayerVM.cs            # ViewModel wrapping TaxPayer; validates via exception catching
│   │   └── TaxPayerCollectionVM.cs  # ObservableCollection<TaxPayerVM> + commands + repository integration + search logic + backup service
│   ├── Resources/
│   │   ├── BootstrapIcons.xaml      # Bootstrap Icons geometry resources
│   │   └── Styles.xaml              # Shared styles (DisabledWhenNullTextBoxStyle)
│   ├── MainWindow.xaml / .xaml.cs   # Main UI; search box with focus-hiding placeholder, TextBoxes for selected item, icon buttons (Ekle/Save/Delete/Copy/Backup), DataGrid with empty-state template, status bar
│   └── App.xaml / .xaml.cs          # Application entry; merges Styles.xaml, auto-backup check on startup, persists app settings on exit
└── VergiNoDogrula.sln
```

## Architecture Overview

The solution follows an N-tier architecture with strict separation between presentation, business-logic, and data-access layers.

### Business-Logic Layer — `VergiNoDogrula`

A .NET class library containing validation rules and domain models. **No WPF or data-access dependencies.**

| File | Purpose |
|---|---|
| `Models/ITaxPayer.cs` | Contract for a tax-paying entity (`Title`, `TaxNumber`). |
| `Models/TaxPayer.cs` | Concrete model. Property setters throw `ArgumentNullException` / `ArgumentException` on invalid input. Implements `IEquatable<TaxPayer>` (equality by `TaxNumber`). |
| `ValidateExtensions.cs` | Static extension methods for Turkish tax-number validation: 10-digit VKN and 11-digit TCKN algorithms. All methods are pure and thread-safe. |


### Data-Access Layer — `VergiNoDogrula.Data`

A .NET class library dedicated to persistence. Depends on `Microsoft.Data.Sqlite`.

| File | Purpose |
|---|---|
| `ITaxPayerRepository.cs` | Repository interface defining async CRUD operations: `GetAllAsync`, `SaveAsync`, `DeleteAsync`, `GetByTaxNumberAsync`. |
| `SqliteTaxPayerRepository.cs` | SQLite-backed implementation. Auto-creates the database and `TaxPayers` table on first use. Uses `UPSERT` (INSERT … ON CONFLICT … UPDATE) for save operations. Tracks CUD metadata in `DatabaseMetadata` and exposes `LastUpdateTime` (local time). |
| `IDatabaseMetadataRepository.cs` | Repository interface for accessing `DatabaseMetadata` table (last update timestamp). |
| `SqliteDatabaseMetadataRepository.cs` | UTC-aware metadata reader; reads `LastUpdateUtc` from database; provides `GetLastUpdateTimeUtcAsync()` and `GetLastUpdateTimeAsync()` (local time conversion). |

### Presentation Layer — `VergiNoDogrula.WPF`

A WPF desktop application. References `VergiNoDogrula` and `VergiNoDogrula.Data`.

| Folder / File | Purpose |
|---|---|
| `ViewModels/AbstractViewModel.cs` | Base class providing `INotifyPropertyChanged` via `RaisePropertyChanged`. |
| `ViewModels/AbstractCollectionVM.cs` | Generic base class for collection ViewModels. Provides `Collection` (ObservableCollection<T>), `CollectionFiltered` (filtered view), `SelectedItem`, and `SearchString` properties. Defines abstract hooks: `OnSearchStringChanged`, `OnSelectedItemChanging`, `OnSelectedItemChanged` for derived classes to implement filtering and selection lifecycle logic. |
| `ViewModels/AbstractDataErrorInfoVM.cs` | Base class adding `INotifyDataErrorInfo` with an `_errors` dictionary, `AddError`, `ClearErrors`, and `GetErrors`. |
| `ViewModels/TaxPayerVM.cs` | Wraps a `TaxPayer` model; validates by calling model setters in a `try/catch` and forwarding exception messages to the error dictionary. Exposes `Validate()` for on-demand (pre-save) validation. |
| `ViewModels/TaxPayerCollectionVM.cs` | Owns `ObservableCollection<TaxPayerVM>`, `SelectedItem`, commands, and `ITaxPayerRepository`. Provides `LoadDataAsync`, `SaveCurrentAsync`, `DeleteSelectedAsync`. Subscribes to `SelectedItem.ErrorsChanged` to refresh command states. Implements smart search: numeric input filters by `TaxNumber` (contains), text input filters by `Title` (case-insensitive contains). |
| `Commands/AbstractCommand.cs` | Base `ICommand` with public `RaiseCanExecuteChanged`. |
| `Commands/AddTaxPayerCommand.cs` | Opens `AddTaxPayerDialog` to create a new `TaxPayerVM`. Passes `TaxPayerCollection` to the dialog for duplicate tax number validation. On success, adds the new taxpayer to the collection and selects it.
| `Commands/CopyTaxNumberCommand.cs` | Copies the selected taxpayer's tax number to the system clipboard. Enabled only when `SelectedItem` is non-null. |
| `Commands/SaveTaxPayerCommand.cs` | Persists the selected `TaxPayerVM` to SQLite via the repository. Enabled only when `SelectedItem` is non-null and has no validation errors. |
| `Commands/DeleteTaxPayerCommand.cs` | Deletes the selected `TaxPayerVM` from SQLite with a confirmation dialog. Enabled only when `SelectedItem` is non-null. |
| `Commands/BackupDatabaseCommand.cs` | Manual backup command; creates timestamped ZIP backup. |
| `Commands/ShowBackupListCommand.cs` | Opens the backup list dialog. Takes `IBackupService` as parameter to load backup files. |
| `Dialogs/AddTaxPayerDialog.xaml` | Modal dialog for creating a new taxpayer. Features: numeric-only input for tax number, real-time validation (format & duplicate check), auto-focus on tax number field, disabled OK button until both fields are valid. Requires `TaxPayerCollection` property to be set for duplicate validation. |
| `Dialogs/BackupListDialog.xaml` | Modal dialog for viewing backup files. Displays DataGrid with file name, creation date (dd.MM.yyyy HH:mm), and formatted size. Shows empty state message when no backups exist. Hides status notification when no new backups are detected. |
| `Dialogs/BackupListDialog.xaml.cs` | Code-behind for backup list dialog. Loads backup files asynchronously, displays status notification when a newer backup exists, provides buttons to open backup and database folders. |
| `Resources/Styles.xaml` | `DisabledWhenNullTextBoxStyle` — disables TextBoxes when `SelectedItem` is null, shows validation errors with red border and message. |
| `MainWindow.xaml` | Grid layout with search box (with smart placeholder "Ara.." that hides when focused OR contains text), labelled TextBoxes bound to `SelectedItem.TaxNumber` / `SelectedItem.Title`, icon buttons (Ekle/Save/Delete/Copy/Backup/BackupList/About), and a `DataGrid` with empty-state template ("Henüz kayıt yok"), and a status bar. |
| `Models/AppSettings.cs` | Singleton app settings persisted under `%APPDATA%\VergiNoDogrula\appsettings.json` (`DatabasePath`, backup settings: `AutoBackupEnabled`, `AutoBackupIntervalMinutes`, `BackupFolder`, `LastBackupTimeUtc`, `MaxBackupFiles`). Also holds `MainWindowPosition` (window location/size persistence). |
| `Models/WindowPosition.cs` | Encapsulates main window position, size, and DPI-aware multi-monitor restoration. Uses native Windows API (`EnumDisplayMonitors`, `GetMonitorInfo`, `GetDpiForMonitor`) to enumerate monitors, detect the screen containing the window, and apply proper DPI scaling. `GetWindowPositions()` saves state; `SetWindowPositions()` restores state before window is displayed. |
| `Services/IBackupService.cs` | Backup service interface: `CreateBackupAsync`, `CleanupOldBackupsAsync`, `IsBackupDue`. |
| `Services/DatabaseBackupService.cs` | SQLite backup implementation using `SqliteConnection.BackupDatabase()` API. Creates timestamped ZIP files. Checks if backup is due based on interval. Auto-cleanup uses `MaxBackupFiles` from settings. |
| `Services/AutoBackupHelper.cs` | Static helper for auto-backup workflow. Checks if backup is due, compares database modification time with last backup time, creates backup if needed, and performs cleanup of old backups. |

## Developer Workflow

| Task | Command / Action |
|---|---|
| Build | `dotnet build` from the repository root |
| Run | Launch `VergiNoDogrula.WPF` (the startup project) from Visual Studio or `dotnet run --project VergiNoDogrula.WPF` |
| Validation-logic changes | Edit files inside `VergiNoDogrula/` |
| Data-access changes | Edit files inside `VergiNoDogrula.Data/` |
| UI changes | Edit files inside `VergiNoDogrula.WPF/` |
| Database location | `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db` (auto-created on first run) |
| App settings location | `%APPDATA%\VergiNoDogrula\appsettings.json` |
| Backup location | `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\` (default) |

## Key Conventions

### Third-Party Asset Attribution
- Bootstrap icon geometry data in `VergiNoDogrula.WPF/Resources/BootstrapIcons.xaml` is derived from Bootstrap Icons.
- Source URL must be documented in project docs and relevant resource files: `https://icons.getbootstrap.com`.
- Keep attribution references to the Bootstrap Icons repository and license:
  - `https://github.com/twbs/icons`
  - `MIT`
- When adding or regenerating icon geometries, do not remove existing attribution notes from `README.md` or `BootstrapIcons.xaml`.

### Native Windows API Usage
- **P/Invoke declarations** live in `NativeMethods` static classes nested within appropriate model/service files.
- All P/Invoke calls should be wrapped in `try/catch` with sensible fallbacks to ensure compatibility across Windows versions.
- Use `[SupportedOSPlatform("windows")]` to mark APIs that are Windows-only.
- **Multi-monitor and DPI support** — `WindowPosition.SetWindowPositions()` uses `EnumDisplayMonitors`, `GetMonitorInfo`, and `GetDpiForMonitor` to detect monitors and apply correct scaling. Window position is restored via `SourceInitialized` event (before window is displayed) to prevent visual jump.

### Localization and Sorting
- The application is designed for Turkish users and uses Turkish culture (`tr-TR`) for text operations.
- `SqliteTaxPayerRepository.GetAllAsync()` sorts results using Turkish culture to ensure correct alphabetical ordering of Turkish characters (ç, ğ, ı, ö, ş, ü).
- When implementing new sorting or string comparison features for user-facing text, always use `StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: true)`.
- **SQLite queries should NOT use `COLLATE` for Turkish text**; sort in C# instead using Turkish culture to avoid incorrect ordering.
- Numeric comparisons and non-user-facing data (e.g., tax numbers, IDs) use invariant culture.

### Backup Strategy
- **Auto-backup on startup** - `App.OnStartup()` checks if backup is due and database was modified since last backup.
- **Manual backup** - Blue archive button in toolbar triggers on-demand backup.
- **Smart deduplication** - Compares `LastUpdateUtc` from database vs `LastBackupTimeUtc` to avoid redundant backups.
- **SQLite BACKUP API** - Uses `SqliteConnection.BackupDatabase()` for consistent snapshots while database is in use.
- **ZIP compression** - Backups compressed with `CompressionLevel.Optimal`.
- **Retention policy** - Keeps last N backups (configurable via `MaxBackupFiles`, default: 10, minimum: 10), auto-deletes older ones.
- **Auto-cleanup** - `AutoBackupHelper.RunAsync()` calls `CleanupOldBackupsAsync()` after successful backup creation.
- **UTC timestamps** - All backup/update times stored in UTC (`LastBackupTimeUtc`, `LastUpdateUtc`) to avoid timezone/DST issues.
- **Default location** - `%USERPROFILE%\Documents\VergiNoDogrula\BackUp\`
- **Naming convention** - `dbbackup-yyyy-MM-dd-HHmm.zip`

### Services Pattern
- Services live in `VergiNoDogrula.WPF/Services/` and encapsulate application-level operations (not business logic).
- Define an interface (`IBackupService`) for testability.
- Services are instantiated in ViewModels or `App.xaml.cs` (no DI container currently).

### Separation of Concerns
- **Never** place validation or business logic in the WPF project.
- **Never** place data-access logic in the WPF project. The repository interface and implementation live in `VergiNoDogrula.Data`.
- ViewModels delegate to the model layer for validation; they only translate exceptions into `INotifyDataErrorInfo` entries.
- ViewModels delegate to the repository for persistence; they call repository methods and handle exceptions with user-facing messages.

### Coding Style
- Use expression-bodied members and modern C# syntax where it improves readability.
- Keep XML doc comments on all public/protected members in the library project.
- Turkish-language comments are acceptable in `ValidateExtensions.cs` (existing convention). English is preferred elsewhere.
- Access modifiers: ViewModels and Commands in the WPF project are `internal`. Models and extensions in the library are `public`.

### MVVM Pattern
- ViewModels inherit from `AbstractViewModel` (or `AbstractDataErrorInfoVM` when validation is needed).
- Commands inherit from `AbstractCommand`. Keep command logic small; heavy work belongs in services or the model layer.
- `DataContext` is assigned in code-behind (`MainWindow.InitializeDataContext`). No DI container is currently used.
- Commands that depend on `SelectedItem` state must have their `CanExecute` refreshed. `TaxPayerCollectionVM` subscribes to `SelectedItem` property changes and `SelectedItem.ErrorsChanged`, then calls `RaiseCanExecuteChanged()` on relevant commands.

### Data Models
- Use `ITaxPayer` for contracts and `TaxPayer` for concrete instances.
- `TaxPayer` properties are **not** immutable (public setters) but **are** guarded by validation in the setters.

## Validation Approach

### TaxPayer Model Validation

The `TaxPayer` class validates properties by throwing exceptions from setters:

- **`Title`**: Explicitly checks `null` → `ArgumentNullException`, then empty → `ArgumentException`.
- **`TaxNumber`**: Explicitly checks `null` → `ArgumentNullException`, trims whitespace, then delegates to `IsValidTaxNumber()` → `ArgumentException` on failure.

> **Consistency note:** `Title` uses explicit null/empty checks while `TaxNumber` relies partly on the extension method. When modifying validation, prefer a consistent pattern across both properties.

### ViewModel Validation (`TaxPayerVM`)

1. Property setters call private `ValidateTaxNumber` / `ValidateTitle` methods.
2. These methods clear previous errors, assign the value to the underlying `TaxPayer` model inside a `try/catch`, and call `AddError` with the exception message on failure.
3. `Validate()` re-runs both checks and is called by `AddTaxPayerCommand` before allowing a new row.
4. `HasErrors` (from `AbstractDataErrorInfoVM`) blocks operations when true.

### UI Binding Guidelines

- Use `UpdateSourceTrigger=LostFocus` for TextBox validation to avoid premature error display.
- Always set `ValidatesOnNotifyDataErrors=True` on validated bindings.
- Validation errors are surfaced via `DisabledWhenNullTextBoxStyle` which shows a red border and an error message below the TextBox.
- TextBoxes are automatically disabled when `SelectedItem` is `null`.

### Data Persistence
- The application uses SQLite via `Microsoft.Data.Sqlite` for local data storage.
- The database file is stored at `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db`.
- `SqliteTaxPayerRepository` auto-creates the database and schema on first instantiation.
- `SqliteTaxPayerRepository` also maintains a single-row `DatabaseMetadata` table with `LastUpdateUtc` (updated on CUD operations).
- `LastUpdateTime` is exposed in local time by converting stored UTC metadata.
- All repository operations are async (`Task`-based). Commands use `async void Execute` to bridge `ICommand` with async repository calls.
- Save uses UPSERT semantics: existing records (matched by `TaxNumber`) are updated; new records are inserted.

## Search Functionality
- `SearchString` property in `AbstractCollectionVM<T>` triggers filtering via the abstract `OnSearchStringChanged()` method.
- `TaxPayerCollectionVM` implements smart search:
  - **Numeric input** (detected via `IsNumeric()` extension) → filters by `TaxNumber` using `Contains()`.
  - **Text input** → filters by `Title` using case-insensitive `Contains()`.
- `CollectionFiltered` is an `ObservableCollection<TaxPayerVM>` bound to the DataGrid. When search is empty, it references the full `Collection`; otherwise it's a new filtered collection.
- Search TextBox in `MainWindow.xaml` uses `UpdateSourceTrigger=PropertyChanged` for real-time filtering.
- A placeholder TextBlock ("Ara..") overlays the search box and is hidden when the TextBox is focused OR contains text. Uses `MultiDataTrigger` with default `Visibility=Collapsed` and shows only when NOT focused AND text is empty (`IsHitTestVisible=False` allows click-through to the TextBox).

## Adding New Features — Checklist

1. **New model property** → Add to `ITaxPayer`, implement with setter validation in `TaxPayer`, expose in `TaxPayerVM` with `try/catch` validation, update repository schema/queries in `VergiNoDogrula.Data`, bind in XAML.
2. **New command** → Inherit from `AbstractCommand`, register in the appropriate collection ViewModel, add `RaiseCanExecuteChanged` calls in the ViewModel where needed, bind via `Command` / `CommandParameter` in XAML.
3. **New style / resource** → Add to `Resources/Styles.xaml`; it is already merged in `App.xaml`.
4. **New validation rule** → Implement as an extension method in `ValidateExtensions.cs`; call from the model setter.
5. **New repository method** → Add to `ITaxPayerRepository`, implement in `SqliteTaxPayerRepository` under `VergiNoDogrula.Data`, call from the appropriate ViewModel method`.
