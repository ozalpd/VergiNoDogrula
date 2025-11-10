# AI Coding Agent Instructions for VergiNoDogrula

This document provides guidance for AI coding agents working on the `VergiNoDogrula` codebase.

## Architecture Overview

This solution follows a standard N-tier architecture, separating the user interface from the core business logic.

- **`VergiNoDogrula.WPF`**: This is the presentation layer, a WPF application built on .NET for Windows. It is responsible for all user-facing elements and interactions.
  - Key file: `MainWindow.xaml` and its code-behind `MainWindow.xaml.cs`.
  - This project references `VergiNoDogrula` to access the business logic.

- **`VergiNoDogrula`**: This is the business logic layer, a .NET class library. It contains the core functionality for validating tax numbers.
  - **`Validate.cs`**: This class should contain the primary logic for tax number validation.
  - **`Models/TaxPayer.cs`**: This is the data model representing a taxpayer. Note that its properties have private setters, promoting immutability.

## Developer Workflow

- **Building**: The project can be built by running `dotnet build` from the root of the repository.
- **Running**: The `VergiNoDogrula.WPF` project is the startup project.
- **Modifications**:
  - For changes to the validation logic, edit files within the `VergiNoDogrula` project.
  - For UI changes, edit files within the `VergiNoDogrula.WPF` project.

## Key Conventions

- **Separation of Concerns**: Strictly maintain the separation between the UI (`VergiNoDogrula.WPF`) and business logic (`VergiNoDogrula`). The UI should not contain any validation logic directly.
- **Data Models**: Use the models defined in `VergiNoDogrula/Models` for data transfer between the layers.
- **Modern .NET**: The projects are configured with modern .NET features like `ImplicitUsings` and `Nullable` enabled. Please adhere to these conventions.
