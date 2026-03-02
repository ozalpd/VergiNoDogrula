using System.IO;
using System.Text.Json;

namespace VergiNoDogrula.WPF.Models
{
    internal class AppSettings
    {
        public AppSettings() { }

        private static AppSettings? _instance;
        private static readonly object _syncRoot = new();

        /// <summary>
        /// Gets or sets a value indicating whether automatic backups are enabled.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, the system performs backups automatically at
        /// scheduled intervals. Ensure that backup settings are properly configured to prevent data loss.</remarks>
        public bool AutoBackupEnabled { get; set; }

        /// <summary>
        /// Gets or sets the interval, in minutes, at which automatic backups are performed. The minimum allowed value
        /// is 10 minutes.
        /// </summary>
        /// <remarks>If a value less than 10 is specified, the interval is automatically adjusted to 10
        /// minutes to ensure backups occur at a reasonable frequency and to help prevent data loss.</remarks>
        public uint AutoBackupIntervalMinutes
        {
            get
            {
                if (backupInterval < 10)
                {
                    backupInterval = 10;
                }
                return backupInterval;
            }

            set => backupInterval = value;
        }
        uint backupInterval = 10;

        /// <summary>
        /// Gets or sets the path to the folder where backups are stored. If the folder does not exist, it is created
        /// automatically.
        /// </summary>
        /// <remarks>The backup folder path can be set to a custom location. If not set, the default
        /// backup folder path is used. Ensure that the specified path has the necessary permissions for creating
        /// directories.</remarks>
        public string BackupFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_backupDir))
                {
                    _backupDir = GetDefaultBackupFolderPath();
                }

                if (!string.IsNullOrWhiteSpace(_backupDir) && !Directory.Exists(_backupDir))
                {
                    Directory.CreateDirectory(_backupDir);
                }
                return _backupDir;
            }
            set => _backupDir = value;
        }
        string _backupDir = string.Empty;

        /// <summary>
        /// Gets or sets the time of the last backup in Coordinated Universal Time (UTC).
        /// </summary>
        /// <remarks>This property is null if no backup has been performed.</remarks>
        public DateTime? LastBackupTimeUtc { get; set; }

        /// <summary>
        /// Number of most recent backups to keep
        /// </summary>
        public uint MaxBackupFiles
        {
            get
            {
                if (backupFilesToKeep < 10)
                {
                    backupFilesToKeep = 10;
                }
                return backupFilesToKeep;
            }

            set => backupFilesToKeep = value;
        }
        uint backupFilesToKeep = 10;

        /// <summary>
        /// Gets or sets a value indicating whether audio playback is muted.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, audio output is silenced. Use this property to
        /// control audio playback in scenarios where muting is required, such as user preferences or application
        /// settings.</remarks>
        public bool MuteAudio { get; set; } = false;

        /// <summary>
        /// Gets or sets the file path to the database used by the application. If the path is not set, a default path
        /// is generated.
        /// </summary>
        /// <remarks>The database path is automatically created if it does not exist when accessed. The
        /// default path is located in the application's default database folder and is named 'taxpayers.db'.</remarks>
        public string DatabasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dbPath))
                {
                    _dbPath = Path.Combine(GetDefaultDatabaseFolderPath(), "taxpayers.db");
                }

                var dbDirPath = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrWhiteSpace(dbDirPath) && !Directory.Exists(dbDirPath))
                {
                    Directory.CreateDirectory(dbDirPath);
                }

                return _dbPath;
            }

            set => _dbPath = value;
        }
        string _dbPath = string.Empty;

        private static string GetDefaultBackupFolderPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "VergiNoDogrula",
            "BackUp");

        private static string GetDefaultDatabaseFolderPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VergiNoDogrula");

        public string GetDatabaseFolderPath() => Path.GetDirectoryName(DatabasePath) ?? string.Empty;

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
