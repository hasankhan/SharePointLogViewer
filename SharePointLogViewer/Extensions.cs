using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharePointLogViewer
{
    static class Extensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> list) 
        { 
            foreach (var item in list)
                collection.Add(item); 
        }
    }
}
