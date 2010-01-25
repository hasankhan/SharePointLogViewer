using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace SharePointLogViewer
{
    class ListSearchCompleteEventArgs<T> : EventArgs
    {
        public IEnumerable<T> Result { get; set; }
    }

    class ListSearcher<T>
    {
        BackgroundWorker worker = new BackgroundWorker();
        IEnumerable<T> list;
        string criterea;
        string searchText;
        IEnumerable<T> result;

        public event EventHandler<ListSearchCompleteEventArgs<T>> SearchComplete = delegate { };

        public ListSearcher()
        {
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Predicate<T> predicate;
            if (criterea == "*")
            {
                var predicates = from property in typeof(T).GetProperties()
                                 select CreatePredicate(property.Name, searchText);
                predicate = entry => predicates.Any(p => p(entry));
            }
            else
                predicate = CreatePredicate(criterea, searchText);
            result = from item in list
                     where predicate(item)
                     select item;
        }        

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchComplete(this, new ListSearchCompleteEventArgs<T>(){Result = result});
        }

        public void Start(IEnumerable<T> list, string criterea, string searchText)
        {
            this.list = list;
            this.criterea = criterea;
            this.searchText = searchText;
            worker.RunWorkerAsync();            
        }

        static Predicate<T> CreatePredicate(string propertyName, string text)
        {
            Predicate<T> predicate = delegate(T entry)
                                            {
                                                PropertyInfo property = typeof(T).GetProperty(propertyName);
                                                if (property == null)
                                                    return false;
                                                string value = property.GetValue(entry, null) as String;
                                                if (value == null)
                                                    return false;
                                                return value.ToLower().Contains(text);
                                            };
            return predicate;
        }
    }
}
