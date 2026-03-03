namespace VergiNoDogrula.WPF.Models;

/// <summary>
/// Represents information about a backup file.
/// </summary>
internal class BackupFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public long SizeInBytes { get; set; }

    public string FormattedSize
    {
        get
        {
            const int KB = 1024;
            const int MB = KB * 1024;

            if (SizeInBytes >= MB)
                return $"{SizeInBytes / (double)MB:F2} MB";
            if (SizeInBytes >= KB)
                return $"{SizeInBytes / (double)KB:F2} KB";
            return $"{SizeInBytes} bytes";
        }
    }
}
