using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointLogViewer.Filters
{
    interface IFilter
    {
        bool Accept(LogEntryViewModel logEntry);
    }
}
