using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SharePointLogViewer
{
    class LoadCompletedEventArgs : EventArgs
    {
        public IEnumerable<LogEntry> LogEntries { get; set; }
    }

    class LogsLoader
    {
        public event EventHandler<LoadCompletedEventArgs> LoadCompleted = delegate { };
        List<LogEntry> logEntries = new List<LogEntry>();
        BackgroundWorker worker;

        public LogsLoader()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadCompleted(this, new LoadCompletedEventArgs() { LogEntries = logEntries });            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            logEntries.Clear();
            string[] files = e.Argument as string[];
            foreach (string file in files)
                logEntries.AddRange(LogParser.PraseLog(file));     
        }

        public void Start(string[] files)
        {
            worker.RunWorkerAsync(files);
        }
    }
}
