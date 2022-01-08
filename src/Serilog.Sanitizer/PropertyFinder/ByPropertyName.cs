using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByPropertyName : IPropertyFinder
    {
        private readonly object _value;
        private readonly string _propertyName;

        public ByPropertyName(object value, string propertyName)
        {
            _value = value;
            _propertyName = propertyName;
        }

        public bool Equal(object property)
        {
            if (property is JProperty jprop)
            {
                return jprop.Name == _propertyName;
            }
            else if (property is PropertyInfo prop && (_value == null || (prop.DeclaringType == _value.GetType())))
            {
                return prop.Name == _propertyName;
            }
            else if (property is string)
            {
                return property.ToString() == _propertyName;
            }

            return false;
        }
    }
}
