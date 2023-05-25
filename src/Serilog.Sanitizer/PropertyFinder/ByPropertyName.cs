using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Sanitizer.PropertyValueBuilder;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByPropertyName : IPropertyFinder
    {
        private readonly object _value;
        private readonly string _propertyName;
        private readonly PropertyFinderConfigration _configration;

        public ByPropertyName(object value, string propertyName, PropertyFinderConfigration configration)
        {
            _value = value;
            _propertyName = propertyName;
            _configration = configration;
        }

        public bool Equal(object property)
        {
            if (property is JsonProperty jsonprop && (!_configration.OnlyPrimitive || jsonprop.Value.ValueKind != JsonValueKind.Object))
            {
                return jsonprop.Name.Equals(_propertyName, _configration.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
            }
            if (property is JProperty jprop)
            {
                if (!_configration.OnlyPrimitive || jprop.First.Type != JTokenType.Object)
                {
                    return jprop.Name.Equals(_propertyName, _configration.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
                }
            }
            else if (property is PropertyInfo prop && (_value == null || (prop.DeclaringType == _value.GetType())))
            {
                if (!_configration.OnlyPrimitive || TypeUtil.IsBuiltinType(prop.PropertyType))
                {
                    return prop.Name.Equals(_propertyName, _configration.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
                }
            }
            else if (property is string)
            {
                return property.ToString().Equals(_propertyName, _configration.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
            }
            else if (property is KeyValuePair<string, LogEventPropertyValue> p)
            {
                return p.Key.Equals(_propertyName, _configration.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
            }

            return false;
        }
    }
}
