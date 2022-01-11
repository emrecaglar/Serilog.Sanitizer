using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByPredicate : IPropertyFinder
    {
        private readonly object _value;
        private readonly Func<PropertyInfo, bool> _predicate;
        private readonly PropertyFinderConfigration _configration;

        public ByPredicate(object value, Func<PropertyInfo, bool> predicate, PropertyFinderConfigration configration)
        {
            _value = value;
            _predicate = predicate;
            _configration = configration;
        }

        public bool Equal(object property)
        {
            if (_value == null && property is PropertyInfo pi)
            {
                return _predicate(pi);
            }
            else if (property is PropertyInfo p)
            {
                var founded = _value.GetType().GetRuntimeProperties().FirstOrDefault(_predicate);

                return founded == p;
            }

            return false;
        }
    }
}
