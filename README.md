# VergiNoDogrula

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
- **SQLite Persistence** — Taxpayer records are stored locally in a SQLite database at `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db`.
- **Metadata Tracking** — CUD operations update `DatabaseMetadata.LastUpdateUtc`; repository exposes `LastUpdateTime` in local time.
- **App Settings Persistence** — Settings are stored in `%APPDATA%\VergiNoDogrula\appsettings.json` and persisted on application exit.
- **Real-time Error Feedback** — Inline validation errors displayed via red borders and descriptive messages.

## Screenshots

The main window provides text fields for entering a tax number and title, **Ekle** (Add), **Kaydet** (Save), and **Sil** (Delete) buttons, and a DataGrid listing all entered taxpayer records.

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

## Solution Structure

```
VergiNoDogrula/
├── VergiNoDogrula/                   # Business-logic class library (net10.0)
│   ├── Data/
│   │   ├── ITaxPayerRepository.cs    # Repository interface for CRUD operations
│   │   └── SqliteTaxPayerRepository.cs # SQLite-backed implementation + DatabaseMetadata tracking
│   ├── Models/
│   │   ├── ITaxPayer.cs              # Interface: Title, TaxNumber
│   │   └── TaxPayer.cs              # Concrete model with setter validation
│   └── ValidateExtensions.cs        # Extension methods for VKN / TCKN validation
├── VergiNoDogrula.WPF/              # WPF presentation layer (net10.0-windows)
│   ├── Commands/
│   │   ├── AbstractCommand.cs       # Base ICommand implementation
│   │   ├── AddTaxPayerCommand.cs    # Opens AddTaxPayerDialog to create a new taxpayer
│   │   ├── SaveTaxPayerCommand.cs   # Persists the selected taxpayer to SQLite
│   │   └── DeleteTaxPayerCommand.cs # Deletes the selected taxpayer from SQLite
│   ├── Dialogs/
│   │   ├── AddTaxPayerDialog.xaml   # Dialog window for creating a new taxpayer
│   │   └── AddTaxPayerDialog.xaml.cs # Modal dialog: numeric-only tax number input, real-time validation with duplicate check, auto-focus on tax number, disabled OK button until valid
│   ├── Models/
│   │   └── AppSettings.cs           # App settings singleton (db path, backup settings, save/load)
│   ├── ViewModels/
│   │   ├── AbstractViewModel.cs     # INotifyPropertyChanged base
│   │   ├── AbstractDataErrorInfoVM.cs # INotifyDataErrorInfo base
│   │   ├── TaxPayerVM.cs            # ViewModel wrapping TaxPayer model
│   │   └── TaxPayerCollectionVM.cs  # ObservableCollection + commands + repository
│   ├── Resources/
│   │   ├── BootstrapIcons.xaml      # Bootstrap Icons geometry resources
│   │   └── Styles.xaml              # Shared WPF styles
│   ├── MainWindow.xaml / .xaml.cs   # Main application window
│   └── App.xaml / .xaml.cs          # Application entry point, resource merge, settings save on exit
└── VergiNoDogrula.sln
```

## Architecture

The solution follows an **N-tier** architecture with a strict separation between business logic and presentation.

### Business-Logic Layer (`VergiNoDogrula`)

A plain .NET class library with **no UI dependencies**. Contains:

- **`ITaxPayer`** — Contract defining `Title` and `TaxNumber` properties.
- **`TaxPayer`** — Concrete model. Setters guard against invalid input by throwing `ArgumentNullException` / `ArgumentException`. Implements `IEquatable<TaxPayer>` based on `TaxNumber`.
- **`ValidateExtensions`** — Pure, thread-safe extension methods implementing the official Turkish VKN (10-digit) and TCKN (11-digit) checksum algorithms.
- **`ITaxPayerRepository`** — Repository interface defining async CRUD operations (`GetAllAsync`, `SaveAsync`, `DeleteAsync`, `GetByTaxNumberAsync`).
- **`SqliteTaxPayerRepository`** — SQLite-backed implementation. Auto-creates the database and `TaxPayers` table on first use. Uses UPSERT semantics for save operations and tracks CUD timestamps in `DatabaseMetadata`.

### Presentation Layer (`VergiNoDogrula.WPF`)

A WPF application following the **MVVM** pattern:

- **ViewModels** delegate validation to the model layer, translating exceptions into `INotifyDataErrorInfo` entries for UI binding.
- **Commands** inherit from `AbstractCommand` and contain minimal logic. `AddTaxPayerCommand` opens `AddTaxPayerDialog` for new taxpayer entry. `SaveTaxPayerCommand` and `DeleteTaxPayerCommand` bridge async repository calls via `async void Execute`.
- **`TaxPayerCollectionVM`** integrates the repository for data loading, saving, and deletion. It subscribes to `SelectedItem` changes and `ErrorsChanged` to refresh command states.
- **Styles** are defined in `Resources/Styles.xaml` and merged via `App.xaml`.
- **`AppSettings`** is loaded as a singleton and persisted at application shutdown.
- **Data** is loaded asynchronously on startup via `MainWindow` initialization.

## Validation Rules

| Type | Length | Algorithm |
|---|---|---|
| **VKN** (Corporate Tax ID) | 10 digits | Weighted modular arithmetic checksum |
| **TCKN** (National ID) | 11 digits | Two-stage modular arithmetic checksum; first digit must not be zero |

Both algorithms are implemented in `ValidateExtensions.cs`. The `IsValidTaxNumber` method automatically dispatches to the correct algorithm based on the length of the input string.

## Data Locations

- **SQLite database**: `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db`
- **App settings**: `%APPDATA%\VergiNoDogrula\appsettings.json`

## Technology Stack

| Component | Technology |
|---|---|
| Target Framework | .NET 10 |
| UI Framework | WPF |
| Language | C# 14.0 |
| Local Database | SQLite via `Microsoft.Data.Sqlite` |
| Build System | SDK-style projects, `dotnet` CLI |

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Commit your changes (`git commit -m "Add my feature"`).
4. Push to the branch (`git push origin feature/my-feature`).
5. Open a Pull Request.

Please maintain the existing separation of concerns — validation logic belongs in the `VergiNoDogrula` library, not in the WPF project.

## License

This project is provided as-is. See the repository for license details.
