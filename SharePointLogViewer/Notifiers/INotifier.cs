using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer.Notifiers
{
    interface INotifier
    {
        void Notify(LogEntryViewModel logEntry);
    }
}
