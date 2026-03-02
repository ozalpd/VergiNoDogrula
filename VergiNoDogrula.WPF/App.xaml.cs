using System.Windows;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.Services;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Timers.Timer? _backupTimer;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var settings = AppSettings.GetAppSettings();
            if (settings.AutoBackupEnabled)
            {
                await AutoBackupHelper.RunAsync();
                int backupInterval = (int)settings.AutoBackupIntervalMinutes;
                _backupTimer = new System.Timers.Timer(backupInterval * 60 * 1000);
                _backupTimer.Elapsed += async (sender, args) => await AutoBackupHelper.RunAsync();
                _backupTimer.Start();
            }
        }

        protected override async void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            var settings = AppSettings.GetAppSettings();
            if (settings.AutoBackupEnabled)
                await AutoBackupHelper.RunAsync(byPassIsBackupDue: true);

            settings.Save();
            base.OnSessionEnding(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _backupTimer?.Stop();
            _backupTimer?.Dispose();

            AppSettings.GetAppSettings().Save();
            base.OnExit(e);
        }
    }
}
