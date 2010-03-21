using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SharePointLogViewer
{
    class OverflowCollection<T>:ObservableCollection<T>
    {
        Func<T, bool> evictionCriterea;
        bool deferNotification;

        public int MaxItems { get; set; }

        public OverflowCollection(Func<T, bool> evictionCriterea) : base()
        {
            this.evictionCriterea = evictionCriterea;
        }

        public OverflowCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> collection)
        {
            deferNotification = true;
            foreach (T itm in collection)
                this.Add(itm);
            deferNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        } 

        protected override void InsertItem(int index, T item)
        {
            if (MaxItems > 0 && Count > MaxItems)
            {
                var target = base.Items.FirstOrDefault<T>(evictionCriterea);
                base.Remove(target);
            }
            base.InsertItem(index, item);            
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!deferNotification)
                base.OnCollectionChanged(e);
        }
    }
}
