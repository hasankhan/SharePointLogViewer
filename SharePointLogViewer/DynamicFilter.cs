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
            Predicate<object> predicate;

            if (String.IsNullOrEmpty(searchText))
                predicate = _ => true;
            else if (propertyName == "*")
            {
                var predicates = from property in typeof(T).GetProperties()
                                 select CreatePredicate(property, searchText);
                predicate = entry => predicates.Any(p => p(entry));
            }
            else
                predicate = CreatePredicate(typeof(T).GetProperty(propertyName), searchText);
                                        
            return predicate;
        }

        public bool IsMatch(object item)
        {
            return predicate(item);
        }

        static Predicate<object> CreatePredicate(PropertyInfo property, string searchText)
        {
            var comparer = new PropertyValueComparer(property, searchText);
            return new Predicate<object>(comparer.IsMatch);
        }

        #region Predicate

        class PropertyValueComparer
        {
            PropertyInfo property;
            string searchText;

            public PropertyValueComparer(PropertyInfo property, string searchText)
            {
                this.property = property;
                this.searchText = searchText;
            }

            public PropertyValueComparer(Type type, string propertyName, string searchText)
                : this(type.GetProperty(propertyName), searchText) { }

            public bool IsMatch(object entry)
            {
                object value = property.GetValue(entry, null);
                if (value == null)
                    return false;
                return value.ToString().ToLower().Contains(searchText);
            }
        } 

        #endregion
    }
}
