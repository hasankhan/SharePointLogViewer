using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;

namespace SharePointLogViewer.Searching
{
    class BookmarkNavigator
    {
        Func<ICollectionView> getCollectionView;
        ListView lstLog;

        public BookmarkNavigator(ListView lstLog, Func<ICollectionView> getCollectionView)
        {
            this.lstLog = lstLog;
            this.getCollectionView = getCollectionView;
        }

        IEnumerable<LogEntryViewModel> LogEntries
        {
            get
            {
                IEnumerable<LogEntryViewModel> logEntries;
                ICollectionView collectionView = getCollectionView();
                if (collectionView == null)
                    logEntries = Enumerable.Empty<LogEntryViewModel>();
                else
                    logEntries = collectionView.Cast<LogEntryViewModel>();
                return logEntries;
            }
        }

        object SelectedItem
        {
            get
            {
                return lstLog.SelectedItem;
            }
            set
            {
                lstLog.SelectedItem = value;
                lstLog.ScrollIntoView(value);
            }
        }

        public void Previous()
        {
            if (LogEntries.Count() == 0)
                return;
            var startFrom = SelectedItem ?? LogEntries.FirstOrDefault();
            var prev = FindPreviousBookMark(startFrom, SelectedItem != null);

            if (prev == null)
                prev = FindPreviousBookMark(LogEntries.LastOrDefault(), false);

            if (prev != null)
                SelectedItem = prev;
        }

        public void Next()
        {
            if (LogEntries.Count() == 0)
                return;
            var startFrom = SelectedItem ?? LogEntries.FirstOrDefault();
            var next = FindNextBookMark(startFrom, SelectedItem != null);

            if (next == null)
                next = FindNextBookMark(LogEntries.FirstOrDefault(), false);

            if (next != null)
                SelectedItem = next;
        }


        LogEntryViewModel FindPreviousBookMark(object startFrom, bool skipStartItem)
        {
            int skip = skipStartItem ? 1 : 0;
            var prev = LogEntries.Reverse()
                        .SkipWhile(le => le != startFrom)
                        .Skip(skip)
                        .FirstOrDefault(le => le.Bookmarked);
            return prev;
        }

        LogEntryViewModel FindNextBookMark(object startFrom, bool skipStartItem)
        {
            int skip = skipStartItem ? 1 : 0;
            var next = LogEntries.SkipWhile(le => le != startFrom)
                       .Skip(skip)
                       .FirstOrDefault(le => le.Bookmarked);
            return next;
        }
    }
}
