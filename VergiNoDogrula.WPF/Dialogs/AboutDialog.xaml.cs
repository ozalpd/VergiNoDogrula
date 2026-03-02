using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VergiNoDogrula.WPF.Models;

namespace VergiNoDogrula.WPF.Dialogs;

/// <summary>
/// Interaction logic for AboutDialog.xaml
/// </summary>
public partial class AboutDialog : Window
{
    public string Product => AppVersion.Product;
    public string Version => AppVersion.Version;
    public string Description => AppVersion.Description;
    public string Copyright => AppVersion.Copyright;

    public AboutDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }
}
