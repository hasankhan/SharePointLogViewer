using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Linq;

namespace SharePointLogViewer
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
                    reader.ReadLine().Split(new char[] { '\t' });

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var entry = LogEntry.Parse(line);
                        entries.Add(entry);
                        //TODO:
                        //if (row["Timestamp"].ToString().EndsWith("*"))
                        //{
                        //    entries.Rows[entries.Rows.Count - 2]["Message"] = entries.Rows[entries.Rows.Count - 2]["Message"].ToString().TrimEnd('.') + row["Message"].ToString().Substring(3);
                        //    entries.Rows.Remove(row);
                        //}
                    }
                }                
            }
            catch { }

            return entries;
        }

        
    }
}
