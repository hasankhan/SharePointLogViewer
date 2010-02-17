using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SharePointLogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SharePointLogViewer.Properties.Settings.Default.Save();
        }
    }
}
