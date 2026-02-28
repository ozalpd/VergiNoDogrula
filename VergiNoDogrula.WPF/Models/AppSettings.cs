using System.IO;
using System.Text.Json;

namespace VergiNoDogrula.WPF.Models
{
    internal class AppSettings
    {
        public AppSettings() { }

        private static AppSettings? _instance;
        private static readonly object _syncRoot = new();

        public bool AutoBackupEnabled { get; set; }
        public uint AutoBackupIntervalMinutes
        {
            get
            {
                if (backupInterval == null || backupInterval == 0)
                {
                    backupInterval = 60; // Default to 60 minutes if not set
                }
                return backupInterval.Value;
            }

            set => backupInterval = value;
        }
        uint? backupInterval = null;

        public string BackupFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_backupDir))
                {
                    _backupDir = GetBackupFolderPath();
                }
                return _backupDir;
            }
            set => _backupDir = value;
        }
        string _backupDir = string.Empty;

        public DateTime? LastBackupTime { get; set; }


        public string DatabasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dbPath))
                {
                    var appDataPath = GetDatabaseFolderPath();
                    Directory.CreateDirectory(appDataPath);
                    _dbPath = Path.Combine(appDataPath, "taxpayers.db");
                }
                return _dbPath;
            }

            set => _dbPath = value;
        }
        string _dbPath = string.Empty;

        private string GetBackupFolderPath()
        {
            if (string.IsNullOrWhiteSpace(_backupDir))
                return Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                       "VergiNoDogrula\\BackUp");
            return _backupDir;
        }

        public string GetDatabaseFolderPath()
        {
            if (string.IsNullOrWhiteSpace(_dbPath))
                return Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                       "VergiNoDogrula");

            return Path.GetDirectoryName(_dbPath) ?? string.Empty;
        }

        public static AppSettings GetAppSettings()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            lock (_syncRoot)
            {
                if (_instance is not null)
                {
                    return _instance;
                }

                var settingsFilePath = GetSettingsFilePath();
                if (File.Exists(settingsFilePath))
                {
                    var settingsJson = File.ReadAllText(settingsFilePath);
                    if (!string.IsNullOrWhiteSpace(settingsJson))
                    {
                        try
                        {
                            _instance = JsonSerializer.Deserialize<AppSettings>(settingsJson);
                        }
                        catch (JsonException)
                        {
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }

                _instance ??= new AppSettings();
                return _instance;
            }
        }

        private static string GetSettingsFilePath()
        {
            var settingsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VergiNoDogrula");
            Directory.CreateDirectory(settingsFolder);

            return Path.Combine(settingsFolder, "appsettings.json");
        }

        public void Save()
        {
            var settingsFilePath = GetSettingsFilePath();
            var settingsJson = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, settingsJson);
        }
    }
}
