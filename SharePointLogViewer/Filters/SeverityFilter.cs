using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer.Filters
{
    class SeverityFilter: IFilter
    {
        int minSeverity;

        public SeverityFilter(TraceSeverity minSeverity)
        {
            this.minSeverity = (int)minSeverity;
        }

        #region ILogEntryFilter Members

        public bool Accept(LogEntryViewModel logEntry)
        {
            int severity = SPUtility.GetSeverity(logEntry.Level);
            bool accept = (severity >= minSeverity);
            return accept;
        }      

        #endregion
    }
}
