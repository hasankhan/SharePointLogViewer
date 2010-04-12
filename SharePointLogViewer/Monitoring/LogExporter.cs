using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharePointLogViewer.Monitoring
{
    class LogExporter
    {
        public void Save(Stream stream, IEnumerable<LogEntry> entries)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (LogEntry logEntry in entries)
                    streamWriter.WriteLine(Format(logEntry));
            }
        }

        public static string Format(LogEntry entry)
        {            
            string stringRep = entry.Timestamp + "\t" +
                               entry.Process + "\t" +
                               entry.TID + "\t" +
                               entry.Area + "\t" +
                               entry.Category + "\t" +
                               entry.EventID + "\t" +
                               entry.Level + "\t" +
                               entry.Message + "\t" +
                               entry.Correlation + "\t";
            return stringRep;
        }
    }
}
