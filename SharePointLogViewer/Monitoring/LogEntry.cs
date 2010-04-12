using System;
using System.Collections.Generic;
using System.Text;

namespace SharePointLogViewer.Monitoring
{
    class LogEntry
    {
        public string Timestamp {get; set; }
        public string Process {get; set; }
        public string TID {get; set; }
        public string Area {get; set; }
        public string Category {get; set; }
        public string EventID {get; set; }
        public string Level {get; set; }
        public string Message {get; set; }
        public string Correlation {get; set; }

        public static LogEntry Parse(string line)
        {
            string[] fields = line.Split('\t');
            if (fields.Length > 8)
            {
                var entry = new LogEntry()
                {
                    Timestamp = fields[0].Trim(),
                    Process = fields[1].Trim(),
                    TID = fields[2].Trim(),
                    Area = fields[3].Trim(),
                    Category = fields[4].Trim(),
                    EventID = fields[5].Trim(),
                    Level = fields[6].Trim(),
                    Message = fields[7].Trim(),
                    Correlation = fields[8].Trim()
                };
                return entry;
            }
            else
                return null;
        }
    }
}
