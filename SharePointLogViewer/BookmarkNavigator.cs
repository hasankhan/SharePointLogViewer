using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SharePointLogViewer
{
    class BookmarkNavigator
    {
        ListView lstLog;
        IEnumerable<LogEntryViewModel> logEntries;

        public BookmarkNavigator(ListView lstLog, IEnumerable<LogEntryViewModel> logEntries)
        {
            this.lstLog = lstLog;
            this.logEntries = logEntries;
        }

        public void Previous()
        {
            if (lstLog.Items.Count == 0)
                return;
            var startFrom = lstLog.SelectedItem ?? lstLog.Items[0];
            var prev = FindPreviousBookMark(startFrom);

            if (prev == null)
                prev = FindPreviousBookMark(logEntries.Last());

            if (prev != null)
            {
                lstLog.SelectedItem = prev;
                lstLog.ScrollIntoView(prev);
            }
        }

        public void Next()
        {
            if (lstLog.Items.Count == 0)
                return;
            var startFrom = lstLog.SelectedItem ?? lstLog.Items[0];
            var next = FindNextBookMark(startFrom);

            if (next == null)
                next = FindNextBookMark(logEntries.First());

            if (next != null)
            {
                lstLog.SelectedItem = next;
                lstLog.ScrollIntoView(next);
            }
        }


        LogEntryViewModel FindPreviousBookMark(object startFrom)
        {
            var prev = logEntries.Reverse()
                        .SkipWhile(le => le != startFrom)
                        .Skip(1)
                        .FirstOrDefault(le => le.Bookmarked);
            return prev;
        }

        LogEntryViewModel FindNextBookMark(object startFrom)
        {
            var next = logEntries.SkipWhile(le => le != startFrom)
                       .Skip(1)
                       .FirstOrDefault(le => le.Bookmarked);
            return next;
        }
    }
}
