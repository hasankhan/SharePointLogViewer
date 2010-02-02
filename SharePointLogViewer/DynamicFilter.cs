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
                                 select CreatePredicate<T>(property.Name, searchText);
                predicate = entry => predicates.Any(p => p(entry));
            }
            else
                predicate = delegate(object entry)
                {
                    PropertyInfo property = typeof(T).GetProperty(propertyName);
                    if (property == null)
                        return false;
                    string value = property.GetValue(entry, null) as String;
                    if (value == null)
                        return false;
                    return value.ToLower().Contains(searchText);
                }; ;
            return predicate;
        }

        public bool IsMatch(object item)
        {
            return predicate(item);
        }        
    }
}
