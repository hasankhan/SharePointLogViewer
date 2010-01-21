using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;


namespace SharePointLogViewer.Controls
{
    public class SortableListView : ListView
    {
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public SortableListView()
        {
            this.AddHandler(
                GridViewColumnHeader.ClickEvent,
                new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(this.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null &&
                headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                // see if we have an attached SortPropertyName value
                string sortBy = GetSortPropertyName(headerClicked.Column);
                if (string.IsNullOrEmpty(sortBy))
                {
                    // otherwise use the column header name
                    sortBy = headerClicked.Column.Header as string;
                }
                Sort(sortBy, direction);

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;
            }
        }

        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(SortableListView));

        public static string GetSortPropertyName(GridViewColumn obj)
        {
            return (string)obj.GetValue(SortPropertyNameProperty);
        }

        public static void SetSortPropertyName(GridViewColumn obj, string value)
        {
            obj.SetValue(SortPropertyNameProperty, value);
        }

    }

}
