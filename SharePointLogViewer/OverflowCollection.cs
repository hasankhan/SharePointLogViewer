using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharePointLogViewer
{
    class OverflowCollection<T>:ObservableCollection<T>
    {
        public int MaxItems { get; set; }

        public OverflowCollection() { }

        public OverflowCollection(IEnumerable<T> collection) : base(collection) { }

        protected override void InsertItem(int index, T item)
        {                
            base.InsertItem(index, item);
            if (MaxItems > 0 && Count > MaxItems)
                RemoveAt(0);
        }
    }
}
