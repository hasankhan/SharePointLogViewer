using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace SharePointLogViewer
{
    class LineDiscoveredEventArgs : EventArgs
    {
        public string Line { get; set; }
    }

    class FileTail: IDisposable
    {
        static char[] seperators = { '\r', '\n' };
        BackgroundWorker worker;
        string filePath;
        ManualResetEvent stopSync;

        public event EventHandler<LineDiscoveredEventArgs> LineDiscovered = delegate { };

        public FileTail()
        {
            stopSync = new ManualResetEvent(true);
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        }

        public bool IsBusy
        {
            get { return worker.IsBusy; }
        }

        public void Start(string path)
        {
            if (IsBusy)
                throw new InvalidOperationException("Can not start while in Busy state.");
            filePath = path;
            stopSync.Reset();
            worker.RunWorkerAsync();
        }

        public void Stop()
        {
            worker.CancelAsync();
            stopSync.WaitOne();
            while (worker.IsBusy) ;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream))
            {
                reader.ReadToEnd();
                string data = String.Empty;
                while (!worker.CancellationPending)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (reader.EndOfStream)
                        continue;
                    data += reader.ReadToEnd();
                    string[] lines = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    data = data.EndsWith("\n") ? String.Empty : lines.Last();
                    int validLines = data == String.Empty ? lines.Length : lines.Length - 1;
                    foreach (string line in lines.Take(validLines))
                        worker.ReportProgress(0, line);                        
                }
            }
            stopSync.Set();
        }        

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LineDiscovered(this, new LineDiscoveredEventArgs() { Line = (String)e.UserState });
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
            worker.Dispose();
        }

        #endregion
    }
}
