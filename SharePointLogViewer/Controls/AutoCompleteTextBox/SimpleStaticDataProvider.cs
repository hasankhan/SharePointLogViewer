using System;
using System.Collections.Generic;

namespace SharePointLogViewer.Controls.AutoCompleteTextBox
{
    public class SimpleStaticDataProvider : IAutoCompleteDataProvider
    {
        private IEnumerable<string> _source;

        public SimpleStaticDataProvider(IEnumerable<string> source)
        {
            _source = source;
        }

        public IEnumerable<string> GetItems(string textPattern)
        {
            foreach (var item in _source)
                if (item.StartsWith(textPattern, StringComparison.OrdinalIgnoreCase))
                    yield return item;
        }
    }
}