using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class PropertyFinderFactory
    {
        private readonly object _value;

        public PropertyFinderFactory(object value)
        {
            _value = value;
        }

        public IPropertyFinder CreateFinder(object finder)
        {
            switch (finder)
            {
                case PropertyInfo propertyInfo:
                    return new ByPropertyInfo(propertyInfo);
                case Func<PropertyInfo, bool> predicate:
                    return new ByPredicate(_value, predicate);
                case string propertyName:
                    return new ByPropertyName(_value, propertyName);
                case Regex regex:
                    return new ByRegex(_value, regex);
            }

            return null;
        }
    }
}
