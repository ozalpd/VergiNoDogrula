using System.Diagnostics;
using System.IO;
using System.Windows;
using VergiNoDogrula.WPF.Models;
using VergiNoDogrula.WPF.Services;

namespace VergiNoDogrula.WPF.Dialogs;

/// <summary>
/// Dialog window for displaying the list of backup files.
/// </summary>
internal partial class BackupListDialog : Window
{
    private readonly IBackupService _backupService;
    private AppSettings _settings = AppSettings.GetAppSettings();

    public BackupListDialog(IBackupService backupService)
    {
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        InitializeComponent();
        Loaded += BackupListDialog_Loaded;
    }

    private async void BackupListDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadBackupFilesAsync();
    }

    private async Task LoadBackupFilesAsync()
    {
        try
        {
            var backupFiles = await _backupService.GetBackupFilesAsync();
            BackupFilesDataGrid.ItemsSource = backupFiles;
            var recentBackup = backupFiles.FirstOrDefault();
            if (recentBackup != null && recentBackup.CreatedDateUtc > _settings.LastBackupTimeUtc)
            {
                StatusTextBlock.Text = "Son yedek işleminden daha yeni bir yedekleme dosyası bulundu!";
                StatusTextBlock2.Text = $"Yedekleme Dosyası: {recentBackup.FileName}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Yedek dosyaları yüklenirken hata oluştu: {ex.Message}",
                "Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenFolde(string folderPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Klasör açılamadı: {ex.Message}",
                "Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenBackupFolderButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFolde(_settings.BackupFolder);
    }

    private void OpenDatabaseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var folderPath = Directory.GetParent(_settings.DatabasePath)?.FullName;
        if (!string.IsNullOrEmpty(folderPath))
            OpenFolde(folderPath);
    }
}
