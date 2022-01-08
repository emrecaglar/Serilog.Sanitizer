using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByRegex : IPropertyFinder
    {
        private readonly object _value;
        private readonly Regex _regex;

        public ByRegex(object value, Regex regex)
        {
            _value = value;
            _regex = regex;
        }

        public bool Equal(object property)
        {
            if (property is JProperty jprop)
            {
                return _regex.IsMatch(jprop.Name);
            }
            else if (property is PropertyInfo prop && (_value == null || (prop.DeclaringType == _value.GetType())))
            {
                return _regex.IsMatch(prop.Name);
            }
            else if (property is string)
            {
                return _regex.IsMatch(property.ToString());
            }

            return false;
        }
    }
}
