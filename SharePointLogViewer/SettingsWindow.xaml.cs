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
            LoadGeneralSettings();
            LoadNotificationSettings();
            this.DataContext = settingsVm;
        }

        private void LoadNotificationSettings()
        {
            settingsVm.NotificationSettings.EnableSystemTrayNotification = Properties.Settings.Default.EnableSystemTrayNotifications;
            settingsVm.NotificationSettings.EnableEmailNotification = Properties.Settings.Default.EnableEmailNotifications;
            settingsVm.NotificationSettings.EnableEventLogNotification = Properties.Settings.Default.EnableEventLogNotifications;
            settingsVm.NotificationSettings.HonourFilters = Properties.Settings.Default.HonourFilters;
            settingsVm.NotificationSettings.MinimumSeverity = Properties.Settings.Default.MinimumSeverity;
            settingsVm.NotificationSettings.EmailSender = Properties.Settings.Default.EmailSenders;
            settingsVm.NotificationSettings.EmailRecepients = Properties.Settings.Default.EmailRecepients;
            settingsVm.NotificationSettings.SmtpServer = Properties.Settings.Default.SmtpServer;
        }

        private void LoadGeneralSettings()
        {
            settingsVm.GeneralSettings.Columns.AddRange(from prop in typeof(LogEntry).GetProperties()
                                                        select new SPColumn()
                                                        {
                                                            IsSelected = Properties.Settings.Default.Columns.Contains(prop.Name),
                                                            Name = prop.Name
                                                        });

            settingsVm.GeneralSettings.LiveLimit = Properties.Settings.Default.LiveLimit;
            settingsVm.GeneralSettings.HideToSystemTray = Properties.Settings.Default.HideToSystemTray;
            settingsVm.GeneralSettings.RunAtStartup = GetRunAtStartup();
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
            SaveGeneralSettings();
            SaveNotificationSettings();
            this.DialogResult = true;
            Close();
        }

        private void SaveNotificationSettings()
        {
            Properties.Settings.Default.EnableSystemTrayNotifications = settingsVm.NotificationSettings.EnableSystemTrayNotification;
            Properties.Settings.Default.EnableEmailNotifications = settingsVm.NotificationSettings.EnableEmailNotification;
            Properties.Settings.Default.EnableEventLogNotifications = settingsVm.NotificationSettings.EnableEventLogNotification;
            Properties.Settings.Default.HonourFilters = settingsVm.NotificationSettings.HonourFilters;
            Properties.Settings.Default.MinimumSeverity = settingsVm.NotificationSettings.MinimumSeverity;
            Properties.Settings.Default.EmailSenders = settingsVm.NotificationSettings.EmailSender;
            Properties.Settings.Default.EmailRecepients = settingsVm.NotificationSettings.EmailRecepients;
            Properties.Settings.Default.SmtpServer = settingsVm.NotificationSettings.SmtpServer;
        }

        private void SaveGeneralSettings()
        {
            Properties.Settings.Default.LiveLimit = settingsVm.GeneralSettings.LiveLimit;
            Properties.Settings.Default.HideToSystemTray = settingsVm.GeneralSettings.HideToSystemTray;
            SetRunAtStartup(settingsVm.GeneralSettings.RunAtStartup);
            var columns = new StringCollection();
            columns.AddRange((from col in settingsVm.GeneralSettings.Columns
                              where col.IsSelected
                              select col.Name).ToArray());
            Properties.Settings.Default.Columns = columns;
        }

        private bool GetRunAtStartup()
        {
            try
            {
                var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                bool run = runKey.GetValue("splv") != null;

                return run;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetRunAtStartup(bool run)
        {
            try
            {
                var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (run)
                    runKey.SetValue("splv", Assembly.GetExecutingAssembly().Location + " /background");
                else
                    runKey.DeleteValue("splv", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set SPLV to run at startup due to exception: " + ex.Message, "SharePoint LogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
