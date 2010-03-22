using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// SortableListView, from the following blog post:
//
// http://blogs.interknowlogy.com/joelrumerman/archive/2007/04/03/12497.aspx

namespace SharePointLogViewer.Controls
{
    public class SortableGridViewColumn : GridViewColumn
    {
        static Setter hideSetter = new Setter(GridViewColumnHeader.VisibilityProperty, Visibility.Collapsed);

        public string SortPropertyName
        {
            get { return (string)GetValue(SortPropertyNameProperty); }
            set { SetValue(SortPropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortPropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.Register("SortPropertyName", typeof(string), typeof(SortableGridViewColumn), new UIPropertyMetadata(""));
        
        public bool IsDefaultSortColumn
        {
            get { return (bool)GetValue(IsDefaultSortColumnProperty); }
            set { SetValue(IsDefaultSortColumnProperty, value); }
        }

        public static readonly DependencyProperty IsDefaultSortColumnProperty =
            DependencyProperty.Register("IsDefaultSortColumn", typeof(bool), typeof(SortableGridViewColumn), new UIPropertyMetadata(false));

        public bool CanBeFiltered
        {
            get { return (bool)GetValue(CanBeFilteredColumnProperty); }
            set { SetValue(CanBeFilteredColumnProperty, value); }
        }

        public static readonly DependencyProperty CanBeFilteredColumnProperty =
            DependencyProperty.Register("CanBeFiltered", typeof(bool), typeof(SortableGridViewColumn), new UIPropertyMetadata(false));

        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set 
            {
                if (Visible ^ value)
                {
                    SetValue(VisibleProperty, value);                        
                    if (value && HeaderContainerStyle != null)
                        HeaderContainerStyle.Setters.Remove(hideSetter);
                    else if (!value)
                    {
                        if (HeaderContainerStyle == null)
                            HeaderContainerStyle = new Style();
                        HeaderContainerStyle.Setters.Add(hideSetter);
                    }
                }
            }
        }

        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(SortableGridViewColumn), new UIPropertyMetadata(true));
    }
}
