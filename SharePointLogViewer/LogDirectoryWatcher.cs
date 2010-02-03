using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharePointLogViewer
{
    class FileCreatedEventArgs : EventArgs
    {
        public string Filename { get; set; }
    }

    class LogDirectoryWatcher : IDisposable
    {
        const string Filter = "*.log";

        FileSystemWatcher watcher;

        public event EventHandler<FileCreatedEventArgs> FileCreated = delegate { };

        public LogDirectoryWatcher(string folderPath)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = folderPath;
            watcher.Filter = Filter;
            watcher.NotifyFilter = NotifyFilters.FileName;
            ;
            watcher.Created += new FileSystemEventHandler(watcher_Created);
        }

        public void Start()
        {            
            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {            
            watcher.EnableRaisingEvents = false;
        }       

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            FileCreated(this, new FileCreatedEventArgs() { Filename = e.FullPath });
        }        

        #region IDisposable Members

        public void Dispose()
        {
            watcher.Dispose();
        }

        #endregion
    }
}
