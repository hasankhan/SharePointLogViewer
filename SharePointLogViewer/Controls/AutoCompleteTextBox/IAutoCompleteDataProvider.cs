using System.Collections.Generic;

namespace SharePointLogViewer.Controls.AutoCompleteTextBox
{
    public interface IAutoCompleteDataProvider
    {
        IEnumerable<string> GetItems(string textPattern);
    }
}
