using System.Configuration;
using System.Data;
using System.Windows;
using VergiNoDogrula.WPF.Models;

namespace VergiNoDogrula.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            AppSettings.GetAppSettings().Save();
            base.OnExit(e);
        }
    }

}
