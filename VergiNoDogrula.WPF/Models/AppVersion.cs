using System.Reflection;

namespace VergiNoDogrula.WPF.Models;

/// <summary>
/// Provides application version information.
/// </summary>
internal static class AppVersion
{
    /// <summary>
    /// Gets the version number (e.g., "1.1.0").
    /// </summary>
    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    /// <summary>
    /// Gets the full informational version.
    /// </summary>
    public static string FullVersion =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? Version;

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public static string Product =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyProductAttribute>()?
            .Product ?? "VergiNo ve TCKN Doğrula";

    /// <summary>
    /// Gets the copyright information.
    /// </summary>
    public static string Copyright =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyCopyrightAttribute>()?
            .Copyright ?? "Copyright © 2025";

    /// <summary>
    /// Gets the description.
    /// </summary>
    public static string Description =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyDescriptionAttribute>()?
            .Description ?? string.Empty;
}
