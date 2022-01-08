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

        public ByPropertyInfo(PropertyInfo property)
        {
            _property = property;
        }

        public bool Equal(object property)
        {
            if (property is PropertyInfo p)
            {
                return _property == p;
            }

            return false;
        }
    }
}
