using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SharePointLogViewer
{
    class HiglightBookmarkedValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bookmarkedAndNotSelected = false;

            if (values.Length == 2)
                bookmarkedAndNotSelected = (bool)values[0] == true && (bool)values[1] == false;
            
            return bookmarkedAndNotSelected;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}
