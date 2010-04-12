using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SharePointLogViewer.Filters
{
    class ListViewFilter: IFilter
    {
        ListView lstLog;

        public ListViewFilter(System.Windows.Controls.ListView lstLog)
        {
            this.lstLog = lstLog;
        }

        #region ILogEntryFilter Members

        public bool Accept(LogEntryViewModel logEntry)
        {
            bool accept = (lstLog.Items.Filter == null || lstLog.Items.Filter(logEntry));
            return accept;
        }

        #endregion
    }
}
