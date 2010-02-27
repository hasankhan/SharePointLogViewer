using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharePointLogViewer
{
    class LogEntryViewModel : INotifyPropertyChanged
    {
        LogEntry entry;
        bool bookmarked;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        public LogEntryViewModel(LogEntry entry)
        {
            this.entry = entry;
        }

        public bool Bookmarked
        {
            get { return bookmarked; }
            set
            {
                bookmarked = value;
                OnPropertyChanged("Bookmarked");
            }
        }

        public string Timestamp
        {
            get { return entry.Timestamp; }
            set 
            { 
                entry.Timestamp = value;
                OnPropertyChanged("Timestamp"); 
            }
        }
        public string Process
        {
            get { return entry.Process; }
            set 
            { 
                entry.Process = value; 
                OnPropertyChanged("Process"); 
            }
        }
        public string TID
        {
            get { return entry.TID; }
            set 
            { 
                entry.TID = value; 
                OnPropertyChanged("TID"); 
            }
        }
        public string Area
        {
            get { return entry.Area; }
            set 
            { 
                entry.Area = value; 
                OnPropertyChanged("Area");
            }
        }
        public string Category
        {
            get { return entry.Category; }
            set 
            { 
                entry.Category = value; 
                OnPropertyChanged("Category"); 
            }
        }
        public string EventID
        {
            get { return entry.EventID; }
            set 
            { 
                entry.EventID = value; 
                OnPropertyChanged("EventID"); 
            }
        }
        public string Level
        {
            get { return entry.Level; }
            set 
            { 
                entry.Level = value; 
                OnPropertyChanged("Level"); 
            }
        }
        public string Message
        {
            get { return entry.Message; }
            set 
            { 
                entry.Message = value; 
                OnPropertyChanged("Message"); 
            }
        }
        public string Correlation
        {
            get { return entry.Correlation; }
            set 
            { 
                entry.Correlation = value; 
                OnPropertyChanged("Correlation"); 
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
