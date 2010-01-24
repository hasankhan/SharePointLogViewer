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

namespace SharePointLogViewer.Controls
{
    /// <summary>
    /// Extends ListView to provide filterable columns
    /// </summary>
    public abstract class FilterableListView : SortableListView
    {
        #region dependency properties

        /// <summary>
        /// The style applied to the filter button when it is an active state
        /// </summary>
        public Style FilterButtonActiveStyle
        {
            get { return (Style)GetValue(FilterButtonActiveStyleProperty); }
            set { SetValue(FilterButtonActiveStyleProperty, value); }
        }

        public static readonly DependencyProperty FilterButtonActiveStyleProperty =
                       DependencyProperty.Register("FilterButtonActiveStyle", typeof(Style), typeof(FilterableListView), new UIPropertyMetadata(null));

        /// <summary>
        /// The style applied to the filter button when it is an inactive state
        /// </summary>
        public Style FilterButtonInactiveStyle
        {
            get { return (Style)GetValue(FilterButtonInactiveStyleProperty); }
            set { SetValue(FilterButtonInactiveStyleProperty, value); }
        }

        public static readonly DependencyProperty FilterButtonInactiveStyleProperty =
                       DependencyProperty.Register("FilterButtonInActiveStyle", typeof(Style), typeof(FilterableListView), new UIPropertyMetadata(null));

        #endregion

        public static readonly ICommand ShowFilter = new RoutedCommand();

        protected ArrayList filterList;
                
        #region inner classes

        /// <summary>
        /// A simple data holder for passing information regarding filter clicks
        /// </summary>
        protected struct FilterStruct
        {
            public Button button;
            public FilterItem value;
            public String property;

            public FilterStruct(String property, Button button, FilterItem value)
            {
                this.value = value;
                this.button = button;
                this.property = property;
            }
        }

        /// <summary>
        /// The items which are bound to the drop down filter list
        /// </summary>
        protected class FilterItem : IComparable
        {
            /// <summary>
            /// The filter item instance
            /// </summary>
            private string item;

            public string Item
            {
                get { return item; }
                set { item = value; }
            }

            /// <summary>
            /// The item viewed in the filter drop down list. Typically this is the same as the item
            /// property, however if item is null, this has the value of "[empty]"
            /// </summary>
            private Object itemView;

            public Object ItemView
            {
                get { return itemView; }
                set { itemView = value; }
            }

            public FilterItem(IComparable item)
            {
                this.itemView = item;
                this.item = item.ToString();
                if (item == null)
                {
                    itemView = "[empty]";
                }
            }

            public override int GetHashCode()
            {
                return item != null ? item.GetHashCode() : 0;
            }

            public override bool Equals(object obj)
            {
                FilterItem otherItem = obj as FilterItem;
                if (otherItem != null)
                {
                    if (otherItem.Item == this.Item)
                    {
                        return true;
                    }
                }
                return false;
            }

            public int CompareTo(object obj)
            {
                FilterItem otherFilterItem = (FilterItem)obj;

                if (this.Item == null && obj == null)
                {
                    return 0;
                }
                else if (otherFilterItem.Item != null && this.Item != null)
                {
                    return ((IComparable)item).CompareTo((IComparable)otherFilterItem.item);
                }
                else
                {
                    return -1;
                }
            }

        }

        #endregion

        protected Hashtable currentFilters = new Hashtable();

        private void AddFilter(String property, FilterItem value, Button button)
        {
            if (currentFilters.ContainsKey(property))
            {
                currentFilters.Remove(property);
            }
            currentFilters.Add(property, new FilterStruct(property, button, value));
        }

        protected bool IsPropertyFiltered(String property)
        {
            foreach (String filterProperty in currentFilters.Keys)
            {
                FilterStruct filter = (FilterStruct)currentFilters[filterProperty];
                if (filter.property == property)
                    return true;
            }

            return false;
        }
        
               
        

        public FilterableListView()
        {
            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand));            
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Uri uri = new Uri("/Controls/FiterListViewDictionary.xaml", UriKind.Relative);
            dictionary = Application.LoadComponent(uri) as ResourceDictionary;

            // cast the ListView's View to a GridView
            GridView gridView = this.View as GridView;
            if (gridView != null)
            {
                // apply the data template, that includes the popup, button etc ... to each column
                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    SortableGridViewColumn sc = gridViewColumn as SortableGridViewColumn;
                    if(sc.CanBeFiltered)
                        gridViewColumn.HeaderTemplate = (DataTemplate)dictionary["FilterGridHeaderTemplate"];                    
                    else
                        gridViewColumn.HeaderTemplate = (DataTemplate)dictionary["SortableGridHeaderTemplate"];                    
                }
            }

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // ensure that the custom inactive style is applied
            if (FilterButtonInactiveStyle != null)
            {
                List<FrameworkElement> columnHeaders = Helpers.FindElementsOfType(this, typeof(GridViewColumnHeader));

                foreach (FrameworkElement columnHeader in columnHeaders)
                {
                    Button button = (Button)Helpers.FindElementOfType(columnHeader, typeof(Button));
                    if (button != null)
                    {
                        button.Style = FilterButtonInactiveStyle;
                    }
                }
            }
            
        }


        /// <summary>
        /// Handles the ShowFilter command to populate the filter list and display the popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e);       

        /// <summary>
        /// Applies the current filter to the list which is being viewed
        /// </summary>
        protected abstract void ApplyCurrentFilters();
        
        /// <summary>
        /// Handles the selection change event from the filter popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            // obtain the term to filter for
            ListView filterListView = (ListView)sender;
            FilterItem filterItem = (FilterItem)filterListView.SelectedItem;

            // navigate up to the header to obtain the filter property name
            GridViewColumnHeader header = (GridViewColumnHeader)Helpers.FindElementOfTypeUp(filterListView, typeof(GridViewColumnHeader));

            SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
            String currentFilterProperty = column.SortPropertyName;

            if (!column.CanBeFiltered)
            {
                FilterStruct filter = (FilterStruct)currentFilters[currentFilterProperty];
                filter.button.Visibility = System.Windows.Visibility.Hidden;

                return;
            }
            if (filterItem == null)
                return;

            // determine whether to clear the filter for this column
            if (filterItem.ItemView.Equals("[clear]"))
            {
                if (currentFilters.ContainsKey(currentFilterProperty))
                {
                    FilterStruct filter = (FilterStruct)currentFilters[currentFilterProperty];
                    filter.button.ContentTemplate = (DataTemplate)dictionary["filterButtonInactiveTemplate"];
                    if (FilterButtonInactiveStyle != null)
                    {
                        filter.button.Style = FilterButtonInactiveStyle;
                    }
                    currentFilters.Remove(currentFilterProperty);
                }

                ApplyCurrentFilters();                
            }
            else
            {   
                // find the button and apply the active style
                Button button = (Button)Helpers.FindVisualElement(header, "filterButton");
                button.ContentTemplate = (DataTemplate)dictionary["filterButtonActiveTemplate"];

                if (FilterButtonActiveStyle != null)
                {
                    button.Style = FilterButtonActiveStyle;
                }

                AddFilter(currentFilterProperty, filterItem, button);
                ApplyCurrentFilters();
            }

            // navigate up to the popup and close it
            Popup popup = (Popup)Helpers.FindElementOfTypeUp(filterListView, typeof(Popup));
            popup.IsOpen = false;
        }
    }
}
