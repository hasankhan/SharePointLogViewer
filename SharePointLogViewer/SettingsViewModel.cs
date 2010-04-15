using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer
{
    class SPColumn
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }

    class SettingsViewModel
    {
        public GeneralSettingsViewModel GeneralSettings { get; set; }
        public NotificationSettingsViewModel NotificationSettings { get; set; }

        public SettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsViewModel();
            NotificationSettings = new NotificationSettingsViewModel();
        }
    }

    class GeneralSettingsViewModel
    {
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public int LiveLimit { get; set; }
        public List<SPColumn> Columns { get; private set; }

        public GeneralSettingsViewModel()
        {
            Columns = new List<SPColumn>();
        }
    }

    class NotificationSettingsViewModel
    {
        public NotificationSettingsViewModel()
        {
        }
        public bool EnableSystemTrayNotification { get; set; }
        public bool EnableEmailNotification { get; set; }
        public bool EnableEventLogNotification { get; set; }
        public bool HonourFilters { get; set; }
        public TraceSeverity MinimumSeverity { get; set; }
        public string EmailSender { get; set; }
        public string EmailRecepients { get; set; }
        public string SmtpServer { get; set; }
    }
}
