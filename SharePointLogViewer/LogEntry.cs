using System;
using System.Collections.Generic;
using System.Text;

namespace SharePointLogViewer
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

        public override string ToString()
        {
            string stringRep = Timestamp + "\t";
            stringRep += Process + "\t";
            stringRep += TID + "\t";
            stringRep += Area + "\t";
            stringRep += Category + "\t";
            stringRep += EventID + "\t";
            stringRep += Level + "\t";
            stringRep += Message + "\t";
            stringRep += Correlation + "\t";
            
            return stringRep;
        }

        public static LogEntry Parse(string line)
        {
            string[] fields = line.Split(new char[] { '\t' });
            var entry = new LogEntry()
            {
                Timestamp = fields[0],
                Process = fields[1],
                TID = fields[2],
                Area = fields[3],
                Category = fields[4],
                EventID = fields[5],
                Level = fields[6],
                Message = fields[7],
                Correlation = fields[8]
            };
            return entry;
        }
    }
}
