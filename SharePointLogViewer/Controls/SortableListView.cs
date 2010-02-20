using System;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using BaseWPFHelpers;

// SortableListView, from the following blog post:
//
// http://blogs.interknowlogy.com/joelrumerman/archive/2007/04/03/12497.aspx

namespace SharePointLogViewer.Controls
{
    /// <summary>
    /// Extends ListView to provide sortable columns
    /// </summary>
    public class SortableListView : ListView
    {
        SortableGridViewColumn lastSortedOnColumn = null;

        GridViewColumnHeader lastSortedOnColumnHeader = null;

        ListSortDirection? lastDirection = ListSortDirection.Ascending;

        protected ResourceDictionary dictionary;


        #region New Dependency Properties


        public string ColumnHeaderSortedAscendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedAscendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedAscendingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderSortedAscendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedAscendingTemplateProperty =
                       DependencyProperty.Register("ColumnHeaderSortedAscendingTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));


        public string ColumnHeaderSortedDescendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedDescendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedDescendingTemplateProperty, value); }
        }


        // Using a DependencyProperty as the backing store for ColumnHeaderSortedDescendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedDescendingTemplateProperty =
            DependencyProperty.Register("ColumnHeaderSortedDescendingTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));


        public string ColumnHeaderNotSortedTemplate
        {
            get { return (string)GetValue(ColumnHeaderNotSortedTemplateProperty); }
            set { SetValue(ColumnHeaderNotSortedTemplateProperty, value); }
        }


        // Using a DependencyProperty as the backing store for ColumnHeaderNotSortedTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderNotSortedTemplateProperty =
            DependencyProperty.Register("ColumnHeaderNotSortedTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));

        #endregion

       

        ///
        /// Executes when the control is initialized completely the first time through. Runs only once.
        ///
        ///
        protected override void OnInitialized(EventArgs e)
        {
            Uri uri = new Uri("/Controls/FiterListViewDictionary.xaml", UriKind.Relative);
            dictionary = Application.LoadComponent(uri) as ResourceDictionary;


            // add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            // cast the ListView's View to a GridView
            GridView gridView = this.View as GridView;
            if (gridView != null)
            {
                // determine which column is marked as IsDefaultSortColumn. Stops on the first column marked this way.
                SortableGridViewColumn sortableGridViewColumn = null;
                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    gridViewColumn.HeaderTemplate = (DataTemplate)dictionary["SortableGridHeaderTemplate"];

                    sortableGridViewColumn = gridViewColumn as SortableGridViewColumn;
                    
                    if (sortableGridViewColumn != null)
                    {
                        if (sortableGridViewColumn.IsDefaultSortColumn)
                        {
                            break;
                        }
                        sortableGridViewColumn = null;
                    }
                                     
                }

                // if the default sort column is defined, sort the data and then update the templates as necessary.
                if (sortableGridViewColumn != null)
                {
                    lastSortedOnColumn = sortableGridViewColumn;
                    Sort(sortableGridViewColumn.SortPropertyName, ListSortDirection.Ascending);

                    if (!String.IsNullOrEmpty(this.ColumnHeaderSortedAscendingTemplate))
                    {
                        sortableGridViewColumn.HeaderTemplate = this.TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                    }

                    this.SelectedIndex = 0;
                }
            }

            base.OnInitialized(e);

        }





        ///
        /// Event Handler for the ColumnHeader Click Event.
        ///
        ///
        ///
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/Controls/FiterListViewDictionary.xaml", UriKind.Relative);
            ResourceDictionary dictionary = Application.LoadComponent(uri) as ResourceDictionary;
            
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;


            // ensure that we clicked on the column header and not the padding that's added to fill the space.
            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                // attempt to cast to the sortableGridViewColumn object.
                SortableGridViewColumn sortableGridViewColumn = (headerClicked.Column) as SortableGridViewColumn;

                // ensure that the column header is the correct type and a sort property has been set.
                if (sortableGridViewColumn != null && !String.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
                {
                    ListSortDirection? direction;

                    // determine if this is a new sort, or a switch in sort direction.
                    if (lastSortedOnColumn == null
                        || String.IsNullOrEmpty(lastSortedOnColumn.SortPropertyName)
                        || !String.Equals(sortableGridViewColumn.SortPropertyName, lastSortedOnColumn.SortPropertyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else if (lastDirection == ListSortDirection.Descending)
                        {
                            direction = null; 
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }


                    // get the sort property name from the column's information.
                    string sortPropertyName = sortableGridViewColumn.SortPropertyName;

                    // Sort the data.
                    if (direction == null)
                        RemoveSort();
                    else
                        Sort(sortPropertyName, direction.Value);    

                    Label sortIndicator = (Label)Helpers.SingleFindDownInTree(headerClicked,
                        new Helpers.FinderMatchName("sortIndicator"));

                    if (direction == null)
                    {
                        sortIndicator.Style = (Style)dictionary["HeaderTemplateTransparent"];    
                    }
                    if (direction == ListSortDirection.Ascending)
                    {
                       sortIndicator.Style = (Style)dictionary["HeaderTemplateArrowUp"];                       
                    }
                    else if(direction == ListSortDirection.Descending)
                    {
                        sortIndicator.Style = (Style)dictionary["HeaderTemplateArrowDown"];                        
                    }

                    // Remove arrow from previously sorted header
                    if (lastSortedOnColumnHeader != null && lastSortedOnColumnHeader!=headerClicked)
                    {
                        sortIndicator = (Label)Helpers.SingleFindDownInTree(lastSortedOnColumnHeader,
                            new Helpers.FinderMatchName("sortIndicator"));

                        sortIndicator.Style = (Style)dictionary["HeaderTemplateTransparent"];    
                    }

                    if (direction == null)
                    {
                        lastSortedOnColumn = null;
                        lastSortedOnColumnHeader = null;
                    }
                    else
                    {
                        lastSortedOnColumn = sortableGridViewColumn;
                        lastSortedOnColumnHeader = headerClicked;
                    }
                }
            }
        }


        ///
        /// Helper method that sorts the data.
        ///
        ///
        ///
        private void Sort(string sortBy, ListSortDirection direction)
        {
            lastDirection = direction;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);

                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        private void RemoveSort()
        { 
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                dataView.Refresh();
            }
        }

    }
}
