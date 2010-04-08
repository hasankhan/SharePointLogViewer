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
using System.Linq;


namespace SharePointLogViewer.Controls
{
    /// <summary>
    /// Extends ListView to provide filterable columns
    /// </summary>
    public abstract class FilterableListView : SortableListView
    {
        protected abstract Type ListItemType { get; }

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

        #region inner classes

        /// <summary>
        /// A simple data holder for passing information regarding filter clicks
        /// </summary>
        class FilterStruct
        {
            public Button Button {get; private set; }
            FilterItem value;
            public PropertyDescriptor PropertyDescriptor { get; set; }

            public FilterStruct(PropertyDescriptor propertyDescriptor, Button button, FilterItem value)
            {
                this.value = value;
                this.Button = button;
                PropertyDescriptor = propertyDescriptor;
            }

            public bool IsMatch(object item)
            {
                object itemValue = PropertyDescriptor.GetValue(item);
                if (itemValue == null)
                    return value.Item == null;
                else
                    return itemValue.Equals(value.ItemView);
            }
        }

        /// <summary>
        /// The items which are bound to the drop down filter list
        /// </summary>
        class FilterItem : IComparable
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
                if (item == null)
                {
                    itemView = "[empty]";
                    this.item = null;
                }
                else
                {
                    itemView = item;
                    this.item = item.ToString();
                }

            }

            public override int GetHashCode()
            {
                return item != null ? item.GetHashCode() : 0;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                
                FilterItem otherItem = obj as FilterItem;
                if (otherItem != null)
                    return this.item == otherItem.item;
                
                return base.Equals(obj);
            }

            public int CompareTo(object obj)
            {
                FilterItem otherFilterItem = (FilterItem)obj;

                if (this.Item == null && obj == null)
                    return 0;
                else if (otherFilterItem.Item != null && this.Item != null)
                    return ((IComparable)item).CompareTo((IComparable)otherFilterItem.item);
                else
                    return -1;
            }
        }

        #endregion

        Predicate<object> extraFilter;
        public Predicate<object> ExtraFilter
        {
            get { return extraFilter; }
            set
            {
                extraFilter = value;
                ApplyCurrentFilters();
            }
        }

        Dictionary<string, FilterStruct> currentFilters = new Dictionary<string, FilterStruct>();

        private void AddFilter(String property, FilterItem value, Button button)
        {
            var descriptor = TypeDescriptor.GetProperties(ListItemType)[property];
            var filter = new FilterStruct(descriptor, button, value);
            currentFilters[property] = filter;
        }

        protected bool IsPropertyFiltered(String property)
        {
            return currentFilters.ContainsKey(property);
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
                    if (sc != null && sc.CanBeFiltered)
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
        private void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
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
                    SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
                    String propertyName = column.SortPropertyName;

                    var filterList = new List<FilterItem>();

                    var uniqueValues = new HashSet<object>();

                    if (IsPropertyFiltered(propertyName))
                        filterList.Add(new FilterItem("[clear]"));
                    else
                    {
                        bool containsNull = false;
                        PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(ListItemType)[propertyName];

                        foreach (Object item in Items)
                        {
                            object value = filterPropDesc.GetValue(item);
                            if (value != null)
                            {
                                if (uniqueValues.Add(value))
                                    filterList.Add(new FilterItem(value as IComparable));
                            }
                            else
                                containsNull = true;
                        }

                        filterList.Sort();

                        if (containsNull)
                            filterList.Add(new FilterItem(null));
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

        /// <summary>
        /// Applies the current filter to the list which is being viewed
        /// </summary>
        void ApplyCurrentFilters()
        {
            if (currentFilters.Count == 0)
            {
                Items.Filter = ExtraFilter;
                return;
            }

            Items.Filter = item => 
            {
                bool accept = currentFilters.Values.All(f => f.IsMatch(item));
                if (accept && ExtraFilter != null)
                    accept = ExtraFilter(item);
                return accept;
            };
        }
        
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
                filter.Button.Visibility = System.Windows.Visibility.Hidden;

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
                    filter.Button.ContentTemplate = (DataTemplate)dictionary["filterButtonInactiveTemplate"];
                    if (FilterButtonInactiveStyle != null)
                        filter.Button.Style = FilterButtonInactiveStyle;
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
                    button.Style = FilterButtonActiveStyle;

                AddFilter(currentFilterProperty, filterItem, button);
                ApplyCurrentFilters();
            }

            // navigate up to the popup and close it
            Popup popup = (Popup)Helpers.FindElementOfTypeUp(filterListView, typeof(Popup));
            popup.IsOpen = false;
        }
    }
}
