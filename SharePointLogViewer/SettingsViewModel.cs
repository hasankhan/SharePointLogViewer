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
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public int LiveLimit { get; set; }
        public List<SPColumn> Columns { get; private set; }

        public SettingsViewModel()
        {
            Columns = new List<SPColumn>();
        }
    }
}
