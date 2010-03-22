using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharePointLogViewer
{
    class DynamicFilter
    {        
        Predicate<object> predicate;

        private DynamicFilter(Predicate<object> predicate)
        {
            this.predicate = predicate;
        }

        public static DynamicFilter Create<T>(string criterea, string searchText)
        {
            var predicate = CreatePredicate<T>(criterea, searchText.ToLower());
            return new DynamicFilter(predicate);
        }

        static Predicate<object> CreatePredicate<T>(string propertyName, string searchText)
        {
            if (searchText == null)
                searchText = String.Empty;

            Predicate<object> predicate;
            if (searchText == String.Empty)
                predicate = _ => true;
            else
            {
                var keywords = searchText.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
                if (propertyName == "*")
                {
                    var predicates = (from property in typeof(T).GetProperties()
                                     select CreatePredicate(property, keywords)).ToList();
                    predicate = entry => predicates.Any(p => p(entry));
                }
                else
                    predicate = CreatePredicate(typeof(T).GetProperty(propertyName), keywords);
            }
                                        
            return predicate;
        }

        public bool IsMatch(object item)
        {
            return predicate(item);
        }

        static Predicate<object> CreatePredicate(PropertyInfo property, params string[] keywords)
        {
            var comparer = new PropertyValueComparer(property, keywords);
            return new Predicate<object>(comparer.IsMatch);
        }

        #region Predicate

        class PropertyValueComparer
        {
            FastInvokeHandler fastInvoker;
            string[] keywords;

            public PropertyValueComparer(PropertyInfo property, params string[] keywords)
            {
                this.keywords = keywords;
                this.fastInvoker = FastInvoke.GetMethodInvoker(property.GetGetMethod());
            }

            public PropertyValueComparer(Type type, string propertyName, params string[] keywords)
                : this(type.GetProperty(propertyName), keywords) { }

            public bool IsMatch(object entry)
            {
                object value = fastInvoker(entry, null);
                if (value == null)
                    return false;
                string text = value.ToString().ToLower();
                bool result = keywords.Any(word => text.Contains(word));
                return result;
            }
        } 

        #endregion
    }
}
