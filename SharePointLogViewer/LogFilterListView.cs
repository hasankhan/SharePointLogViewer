using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SharePointLogViewer.Controls;

namespace SharePointLogViewer
{
    class LogFilterListView : FilterableListView
    {
        protected override Type ListItemType
        {
            get { return typeof(LogEntryViewModel); }
        }
    }
}
