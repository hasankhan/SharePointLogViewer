using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharePointLogViewer
{    
    class LogMonitor: ILogMonitor
    {
        FileTail fileTail;
        string folderPath;
        LogDirectoryWatcher watcher;

        public event EventHandler<LogEntryDiscoveredEventArgs> LogEntryDiscovered = delegate { };

        public LogMonitor(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new ArgumentException("Directory does not exist.", folderPath);
            this.folderPath = folderPath;
            fileTail = new FileTail();
            fileTail.LineDiscovered += new EventHandler<LineDiscoveredEventArgs>(fileTail_LineDiscovered);
            watcher = new LogDirectoryWatcher(folderPath);
            watcher.FileCreated += new EventHandler<FileCreatedEventArgs>(watcher_FileCreated);            
        }

        public void Start()
        {
            watcher.Start();
            string filePath = SPUtility.GetLastAccessedFile(folderPath);
            if (filePath != null)
                fileTail.Start(filePath);
        }

        public void Stop()
        {
            watcher.Stop();
            fileTail.Stop();
        }

        void watcher_FileCreated(object sender, FileCreatedEventArgs e)
        {
            fileTail.Stop();
            fileTail.Start(e.Filename);
        }

        void fileTail_LineDiscovered(object sender, LineDiscoveredEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Line.Trim()))
            {
                var entry = LogEntry.Parse(e.Line);
                if (entry != null)
                    LogEntryDiscovered(this, new LogEntryDiscoveredEventArgs() { LogEntry = entry });
            }
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
