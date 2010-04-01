using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer
{
    class MultiLogMonitor: IDisposable
    {
        List<LogMonitor> logMonitors;

        public event EventHandler<LogEntryDiscoveredEventArgs> LogEntryDiscovered = delegate { };

        public MultiLogMonitor(IEnumerable<string> folderPaths)
        {
            logMonitors = new List<LogMonitor>(from folderPath in folderPaths
                                               select new LogMonitor(folderPath));
        }

        public void Start()
        {
            logMonitors.ForEach(monitor =>
            {
                monitor.Start();
                monitor.LogEntryDiscovered += new EventHandler<LogEntryDiscoveredEventArgs>(monitor_LogEntryDiscovered);
            });
        }
        
        public void Stop()
        {
            logMonitors.ForEach(monitor =>
            { 
                monitor.Stop();
                monitor.LogEntryDiscovered -= new EventHandler<LogEntryDiscoveredEventArgs>(monitor_LogEntryDiscovered);
            });
        }

        void monitor_LogEntryDiscovered(object sender, LogEntryDiscoveredEventArgs e)
        {
            LogEntryDiscovered(this, e);
        }
    
        #region IDisposable Members

        public void  Dispose()
        {
            Stop();
            logMonitors.ForEach(monitor => monitor.Dispose());
        }

        #endregion
    }
}
