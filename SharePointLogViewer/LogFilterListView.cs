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
using BaseWPFHelpers;
using SharePointLogViewer.Controls;

namespace SharePointLogViewer
{
    class LogFilterListView : SharePointLogViewer.Controls.FilterableListView
    {
        protected override void ApplyCurrentFilters()
        {
            if (currentFilters.Count == 0)
            {
                Items.Filter = null;
                return;
            }

            // construct a filter and apply it               
            Items.Filter = delegate(object item)
            {
                // when applying the filter to each item, iterate over all of
                // the current filters
                bool match = true;
                foreach (FilterStruct filter in currentFilters.Values)
                {
                    // obtain the value for this property on the item under test
                    PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(LogEntry))[filter.property];
                    object itemValue = filterPropDesc.GetValue((LogEntry)item);

                    if (itemValue != null)
                    {
                        // check to see if it meets our filter criteria
                        if (!itemValue.Equals(filter.value.ItemView))
                            match = false;
                    }
                    else
                    {
                        if (filter.value.Item != null)
                            match = false;
                    }
                }
                return match;
            };
        }

        protected override void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            if (button != null)
            {
                // navigate up to the header
                GridViewColumnHeader header = (GridViewColumnHeader)Helpers.FindElementOfTypeUp(button, typeof(GridViewColumnHeader));

                // then down to the popup
                Popup popup = (Popup)Helpers.FindElementOfType(header, typeof(Popup));

                if (popup != null)
                {
                    // find the property name that we are filtering
                    SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
                    String propertyName = column.SortPropertyName;


                    // clear the previous filter
                    if (filterList == null)
                    {
                        filterList = new ArrayList();
                    }
                    filterList.Clear();

                    // if this property is currently being filtered, provide an option to clear the filter.
                    if (IsPropertyFiltered(propertyName))
                    {
                        filterList.Add(new FilterItem("[clear]"));
                    }
                    else
                    {
                        bool containsNull = false;
                        PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(LogEntry))[propertyName];

                        // iterate over all the objects in the list
                        foreach (Object item in Items)
                        {
                            object value = filterPropDesc.GetValue(item);
                            if (value != null)
                            {
                                FilterItem filterItem = new FilterItem(value as IComparable);
                                if (!filterList.Contains(filterItem))
                                {
                                    filterList.Add(filterItem);
                                }
                            }
                            else
                            {
                                containsNull = true;
                            }
                        }

                        filterList.Sort();

                        if (containsNull)
                        {
                            filterList.Add(new FilterItem(null));
                        }
                    }

                    // open the popup to display this list
                    popup.DataContext = filterList;
                    CollectionViewSource.GetDefaultView(filterList).Refresh();
                    popup.IsOpen = true;

                    // connect to the selection change event
                    ListView listView = (ListView)popup.Child;
                    listView.SelectionChanged += SelectionChangedHandler;
                }
            }
        }
    }
}
