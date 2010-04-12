using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SharePointLogViewer.Notifiers
{
    class EventLogNotifier : INotifier
    {
        string source;

        public EventLogNotifier(string source, string logName)
        {
            this.source = source;

            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, logName);
        }
        #region INotifier Members

        public void Notify(LogEntryViewModel logEntry)
        {
            EventLog.WriteEntry(source, logEntry.Message, EventLogEntryType.Error);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
