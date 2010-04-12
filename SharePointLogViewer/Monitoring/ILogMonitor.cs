using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer.Monitoring
{
    class LogEntryDiscoveredEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; set; }
    }

    interface ILogMonitor: IDisposable
    {
        event EventHandler<LogEntryDiscoveredEventArgs> LogEntryDiscovered;
        void Start();
        void Stop();
    }
}
