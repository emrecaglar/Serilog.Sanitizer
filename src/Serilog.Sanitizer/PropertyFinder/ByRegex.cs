﻿using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Sanitizer.PropertyValueBuilder;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByRegex : IPropertyFinder
    {
        private readonly object _value;
        private readonly Regex _regex;
        private readonly PropertyFinderConfigration _configration;

        public ByRegex(object value, Regex regex, PropertyFinderConfigration configration)
        {
            _value = value;
            _regex = regex;
            _configration = configration;
        }

        public bool Equal(object property)
        {
            if (property is JsonProperty jsonprop && (!_configration.OnlyPrimitive || jsonprop.Value.ValueKind != JsonValueKind.Object))
            {
                return _regex.IsMatch(jsonprop.Name);
            }
            if (property is JProperty jprop)
            {
                if (!_configration.OnlyPrimitive || jprop.First.Type != JTokenType.Object)
                {
                    return _regex.IsMatch(jprop.Name);
                }
            }
            else if (property is PropertyInfo prop && (_value == null || (prop.DeclaringType == _value.GetType())))
            {
                if (!_configration.OnlyPrimitive || TypeUtil.IsBuiltinType(prop.PropertyType))
                {
                    return _regex.IsMatch(prop.Name);
                }
            }
            else if (property is string)
            {
                return _regex.IsMatch(property.ToString());
            }
            else if (property is KeyValuePair<string, LogEventPropertyValue> p)
            {
                return _regex.IsMatch(p.Key);
            }

            return false;
        }
    }
}
