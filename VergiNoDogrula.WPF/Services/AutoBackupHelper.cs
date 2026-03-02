using VergiNoDogrula.Data;
using VergiNoDogrula.WPF.Models;

namespace VergiNoDogrula.WPF.Services
{
    internal static class AutoBackupHelper
    {
        internal static async Task RunAsync(bool byPassIsBackupDue = false)
        {
            var settings = AppSettings.GetAppSettings();
            if (!settings.AutoBackupEnabled)
                return;

            var backupService = new DatabaseBackupService(settings);
            var metadataRepository = new SqliteDatabaseMetadataRepository(settings.DatabasePath);

            if (byPassIsBackupDue || backupService.IsBackupDue())
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
    }
}
