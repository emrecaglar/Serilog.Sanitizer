using Serilog.Sanitizer.PropertyValueBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public class ByPropertyInfo : IPropertyFinder
    {
        private readonly PropertyInfo _property;
        private readonly PropertyFinderConfigration _configration;

        public ByPropertyInfo(PropertyInfo property, PropertyFinderConfigration configration)
        {
            _property = property;
            _configration = configration;
        }

        public bool Equal(object property)
        {
            if (property is PropertyInfo p && (!_configration.OnlyPrimitive || TypeUtil.IsBuiltinType(p.PropertyType)))
            {
                return _property == p;
            }

            return false;
        }
    }
}
