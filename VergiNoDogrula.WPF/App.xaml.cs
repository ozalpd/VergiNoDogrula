using System.Windows;
using VergiNoDogrula.Data;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.Services;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await AutoBackup();
        }

        private static async Task AutoBackup()
        {
            var settings = AppSettings.GetAppSettings();
            if (!settings.AutoBackupEnabled)
                return;

            var backupService = new DatabaseBackupService(settings);
            var metadataRepository = new SqliteDatabaseMetadataRepository(settings.DatabasePath);

            if (backupService.IsBackupDue())
            {
                var lastDbUpdateUtc = await metadataRepository.GetLastUpdateTimeUtcAsync();
                var lastBackupUtc = settings.LastBackupTimeUtc;

                bool needsBackup = lastDbUpdateUtc.HasValue && (!lastBackupUtc.HasValue || lastDbUpdateUtc.Value > lastBackupUtc.Value);

                if (needsBackup)
                {
                    var connectionString = $"Data Source={settings.DatabasePath}";
                    await backupService.CreateBackupAsync(connectionString);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppSettings.GetAppSettings().Save();
            base.OnExit(e);
        }
    }
}
