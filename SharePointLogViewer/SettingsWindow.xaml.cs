using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Reflection;

namespace SharePointLogViewer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        SettingsViewModel settingsVm = new SettingsViewModel();

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settingsVm.Columns.AddRange(from prop in typeof(LogEntry).GetProperties()
                                        select new SPColumn() { IsSelected = Properties.Settings.Default.Columns.Contains(prop.Name), Name = prop.Name });

            settingsVm.LiveLimit = Properties.Settings.Default.LiveLimit;
            settingsVm.HideToSystemTray = Properties.Settings.Default.HideToSystemTray;
            settingsVm.RunAtStartup = GetRunAtStartup();
            this.DataContext = settingsVm;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LiveLimit = settingsVm.LiveLimit;
            Properties.Settings.Default.HideToSystemTray = settingsVm.HideToSystemTray;
            SetRunAtStartup(settingsVm.RunAtStartup);
            var columns = new StringCollection();
            columns.AddRange((from col in settingsVm.Columns
                             where col.IsSelected
                             select col.Name).ToArray());
            Properties.Settings.Default.Columns = columns;
            this.DialogResult = true;
            Close();
        }

        private bool GetRunAtStartup()
        {
            RegistryKey localMachine = Registry.LocalMachine;
            RegistryKey runKey;
            //access the regitry key where you want to add your registry key            
            try
            {
                runKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                bool run = runKey.GetValue("splv") != null;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetRunAtStartup(bool run)
        {
            RegistryKey localMachine = Registry.LocalMachine; 
            RegistryKey runKey;
            //access the regitry key where you want to add your registry key   
            try
            {
                runKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (run)
                {
                    if (runKey.GetValue("splv") == null)
                        runKey.SetValue("splv", Assembly.GetExecutingAssembly().Location, RegistryValueKind.ExpandString);
                }
                else
                    runKey.DeleteSubKey("splv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set SPLV to run at startup due to lack of administrative priviliges. "+ex.Message, "SharePoint LogViewer");
            }
        }
    }
}
