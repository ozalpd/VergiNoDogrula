# AI Coding Agent Instructions for VergiNoDogrula

This document provides guidance for AI coding agents working on the `VergiNoDogrula` codebase.

## Technology Stack

| Component | Technology |
|---|---|
| Target Framework | .NET 10 |
| UI Framework | WPF (`net10.0-windows`) |
| Language | C# 14.0 |
| Build System | SDK-style projects, `dotnet` CLI |

Both projects enable `ImplicitUsings` and `Nullable`. All new code **must** be nullable-aware and avoid adding explicit `using` directives that are already covered by implicit usings.

## Solution Structure

```
VergiNoDogrula/                       # Root / solution directory
├── .github/
│   └── copilot-instructions.md       # This file
├── VergiNoDogrula/                   # Business-logic class library (net10.0)
│   ├── Data/
│   │   ├── ITaxPayerRepository.cs    # Repository interface: GetAll, Save, Delete, GetByTaxNumber
│   │   └── SqliteTaxPayerRepository.cs # SQLite implementation of ITaxPayerRepository
│   ├── Models/
│   │   ├── ITaxPayer.cs              # Interface: Title, TaxNumber
│   │   └── TaxPayer.cs              # Concrete model with property-level validation
│   └── ValidateExtensions.cs        # Extension methods: IsValidVKN, IsValidTCKN, IsValidTaxNumber, IsSameTaxNumbers
├── VergiNoDogrula.WPF/              # WPF presentation layer (net10.0-windows)
│   ├── Commands/
│   │   ├── AbstractCommand.cs       # Base ICommand implementation with public RaiseCanExecuteChanged
│   │   ├── AddTaxPayerCommand.cs    # Opens AddTaxPayerDialog to create a new TaxPayerVM
│   │   ├── SaveTaxPayerCommand.cs   # Validates then persists the selected TaxPayerVM to SQLite
│   │   └── DeleteTaxPayerCommand.cs # Deletes the selected TaxPayerVM from SQLite with confirmation
│   ├── Dialogs/
│   │   ├── AddTaxPayerDialog.xaml   # Dialog window for creating a new taxpayer
│   │   └── AddTaxPayerDialog.xaml.cs # Code-behind with validation and OK button state management
│   ├── ViewModels/
│   │   ├── AbstractViewModel.cs     # INotifyPropertyChanged base
│   │   ├── AbstractDataErrorInfoVM.cs # INotifyDataErrorInfo base (error dictionary)
│   │   ├── TaxPayerVM.cs            # ViewModel wrapping TaxPayer; validates via exception catching
│   │   └── TaxPayerCollectionVM.cs  # ObservableCollection<TaxPayerVM> + commands + repository integration
│   ├── Resources/
│   │   └── Styles.xaml              # Shared styles (DisabledWhenNullTextBoxStyle)
│   ├── MainWindow.xaml / .xaml.cs   # Main UI; DataContext = TaxPayerCollectionVM
│   └── App.xaml / .xaml.cs          # Application entry; merges Styles.xaml
└── VergiNoDogrula.sln
```

## Architecture Overview

The solution follows an N-tier architecture with strict separation between the presentation and business-logic layers.

### Business-Logic Layer — `VergiNoDogrula`

A .NET class library containing all validation rules, data models, and data access. **No WPF or UI dependencies.** Depends on `Microsoft.Data.Sqlite` for persistence.

| File | Purpose |
|---|---|
| `Models/ITaxPayer.cs` | Contract for a tax-paying entity (`Title`, `TaxNumber`). |
| `Models/TaxPayer.cs` | Concrete model. Property setters throw `ArgumentNullException` / `ArgumentException` on invalid input. Implements `IEquatable<TaxPayer>` (equality by `TaxNumber`). |
| `ValidateExtensions.cs` | Static extension methods for Turkish tax-number validation: 10-digit VKN and 11-digit TCKN algorithms. All methods are pure and thread-safe. |
| `Data/ITaxPayerRepository.cs` | Repository interface defining async CRUD operations: `GetAllAsync`, `SaveAsync`, `DeleteAsync`, `GetByTaxNumberAsync`. |
| `Data/SqliteTaxPayerRepository.cs` | SQLite-backed implementation. Auto-creates the database and `TaxPayers` table on first use. Uses `UPSERT` (INSERT … ON CONFLICT … UPDATE) for save operations. |

### Presentation Layer — `VergiNoDogrula.WPF`

A WPF desktop application. References the `VergiNoDogrula` library.

| Folder / File | Purpose |
|---|---|
| `ViewModels/AbstractViewModel.cs` | Base class providing `INotifyPropertyChanged` via `RaisePropertyChanged`. |
| `ViewModels/AbstractDataErrorInfoVM.cs` | Base class adding `INotifyDataErrorInfo` with an `_errors` dictionary, `AddError`, `ClearErrors`, and `GetErrors`. |
| `ViewModels/TaxPayerVM.cs` | Wraps a `TaxPayer` model; validates by calling model setters in a `try/catch` and forwarding exception messages to the error dictionary. Exposes `Validate()` for on-demand (pre-save) validation. |
| `ViewModels/TaxPayerCollectionVM.cs` | Owns `ObservableCollection<TaxPayerVM>`, `SelectedItem`, commands, and `ITaxPayerRepository`. Provides `LoadDataAsync`, `SaveCurrentAsync`, `DeleteSelectedAsync`. Subscribes to `SelectedItem.ErrorsChanged` to refresh command states. |
| `Commands/AbstractCommand.cs` | Base `ICommand` with public `RaiseCanExecuteChanged`. |
| `Commands/AddTaxPayerCommand.cs` | Opens AddTaxPayerDialog to create a new TaxPayerVM |
| `Commands/SaveTaxPayerCommand.cs` | Persists the selected `TaxPayerVM` to SQLite via the repository. Enabled only when `SelectedItem` is non-null and has no validation errors. |
| `Commands/DeleteTaxPayerCommand.cs` | Deletes the selected `TaxPayerVM` from SQLite with a confirmation dialog. Enabled only when `SelectedItem` is non-null. |
| `Resources/Styles.xaml` | `DisabledWhenNullTextBoxStyle` — disables TextBoxes when `SelectedItem` is null, shows validation errors with red border and message. |
| `MainWindow.xaml` | Grid layout with labelled TextBoxes bound to `SelectedItem.TaxNumber` / `SelectedItem.Title`, "Ekle" (Add), "Kaydet" (Save), and "Sil" (Delete) buttons, and a `DataGrid`. |

## Developer Workflow

| Task | Command / Action |
|---|---|
| Build | `dotnet build` from the repository root |
| Run | Launch `VergiNoDogrula.WPF` (the startup project) from Visual Studio or `dotnet run --project VergiNoDogrula.WPF` |
| Validation-logic changes | Edit files inside `VergiNoDogrula/` |
| Data-access changes | Edit files inside `VergiNoDogrula/Data/` |
| UI changes | Edit files inside `VergiNoDogrula.WPF/` |
| Database location | `%LOCALAPPDATA%\VergiNoDogrula\taxpayers.db` (auto-created on first run) |

## Key Conventions

### Separation of Concerns
- **Never** place validation or business logic in the WPF project.
- **Never** place data-access logic in the WPF project. The repository interface and implementation live in the business-logic layer.
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
- All repository operations are async (`Task`-based). Commands use `async void Execute` to bridge `ICommand` with async repository calls.
- Save uses UPSERT semantics: existing records (matched by `TaxNumber`) are updated; new records are inserted.

## Adding New Features — Checklist

1. **New model property** → Add to `ITaxPayer`, implement with setter validation in `TaxPayer`, expose in `TaxPayerVM` with `try/catch` validation, update `SqliteTaxPayerRepository` schema and queries, bind in XAML.
2. **New command** → Inherit from `AbstractCommand`, register in the appropriate collection ViewModel, add `RaiseCanExecuteChanged` calls in the ViewModel where needed, bind via `Command` / `CommandParameter` in XAML.
3. **New style / resource** → Add to `Resources/Styles.xaml`; it is already merged in `App.xaml`.
4. **New validation rule** → Implement as an extension method in `ValidateExtensions.cs`; call from the model setter.
5. **New repository method** → Add to `ITaxPayerRepository`, implement in `SqliteTaxPayerRepository`, call from the appropriate ViewModel method.
