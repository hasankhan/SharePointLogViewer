using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharePointLogViewer
{
    class OverflowCollection<T>:ObservableCollection<T>
    {
        Func<T, bool> evictionCriterea;

        public int MaxItems { get; set; }

        public OverflowCollection(Func<T, bool> evictionCriterea) : base()
        {
            this.evictionCriterea = evictionCriterea;
        }

        public OverflowCollection(IEnumerable<T> collection) : base(collection) { }

        protected override void InsertItem(int index, T item)
        {
            if (MaxItems > 0 && Count > MaxItems)
            {
                var target = base.Items.FirstOrDefault<T>(evictionCriterea);
                base.Remove(target);
            }
            base.InsertItem(index, item);            
        }
    }
}
