using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer.Filters
{
    class SeverityFilter: IFilter
    {
        int minSeverity;

        public SeverityFilter(int minSeverity)
        {
            this.minSeverity = minSeverity;
        }

        #region ILogEntryFilter Members

        public bool Accept(LogEntryViewModel logEntry)
        {
            int severity = GetSeverity(logEntry.Level);
            bool accept = (severity >= minSeverity);
            return accept;
        }

        private int GetSeverity(string level)
        {
            return 1;
        }

        #endregion
    }
}
