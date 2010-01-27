using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharePointLogViewer
{
    class LogEntryDiscoveredEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; set; }
    }

    class LogDirectoryWatcher : IDisposable
    {
        const string Filter = "*.log";

        FileSystemWatcher watcher;
        string logsLocation;
        FileTail fileTail;

        public event EventHandler<LogEntryDiscoveredEventArgs> LogEntryDiscovered = delegate { };

        public LogDirectoryWatcher(string folderPath)
        {
            fileTail = new FileTail();
            watcher = new FileSystemWatcher();
            logsLocation = folderPath;
            watcher.Path = logsLocation;
            watcher.Filter = Filter;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Created += new FileSystemEventHandler(watcher_Created);
        }

        public void Start()
        {
            string filePath = GetLastAccessedFile(logsLocation);
            if (filePath != null)
            {
                fileTail.Start(filePath);
                fileTail.LineDiscovered += new EventHandler<LineDiscoveredEventArgs>(fileTail_LineDiscovered);
            }

            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            fileTail.Stop();
            watcher.EnableRaisingEvents = false;
        }

        void fileTail_LineDiscovered(object sender, LineDiscoveredEventArgs e)
        {
            var entry = LogEntry.Parse(e.Line);
            LogEntryDiscovered(this, new LogEntryDiscoveredEventArgs() { LogEntry = entry });
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            fileTail.Stop();
            fileTail.Start(e.FullPath);
        }

        string GetLastAccessedFile(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            var file = dirInfo.GetFiles().OrderByDescending( f => f.LastWriteTime).FirstOrDefault();

            if (file != null)
                return file.FullName;
            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            fileTail.Stop();            
            watcher.Dispose();
        }

        #endregion
    }
}
