using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Linq;

namespace SharePointLogViewer.Monitoring
{
    class LogParser
    {
        public static IEnumerable<LogEntry> PraseLog(string fileName)
        {
            List<LogEntry> entries = new List<LogEntry>();

            try
            {                
                FileStream logFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var reader = new StreamReader(logFile))
                {
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var entry = LogEntry.Parse(line);
                        if (entry.Timestamp.EndsWith("*") && entries.Count > 0)
                            entries[entries.Count - 1].Message = entries[entries.Count - 1].Message.TrimEnd('.') + entry.Message.Substring(3);
                        else
                            entries.Add(entry);
                    }
                }                
            }
            catch { }

            return entries;
        }

        
    }
}
