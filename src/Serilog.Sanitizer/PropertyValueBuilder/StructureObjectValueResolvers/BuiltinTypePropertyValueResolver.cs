using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public class BuiltinTypePropertyValueResolver : IStructureObjectPropertyValueResolver
    {
        private readonly object _value;
        private readonly DestructorLimits _destructorLimits;

        public BuiltinTypePropertyValueResolver(object value, DestructorLimits destructorLimits)
        {
            _value = value;
            _destructorLimits = destructorLimits;
        }

        public LogEventPropertyValue GetValue(object parentObject)
        {
            if (_value is string s && s.Length > _destructorLimits.MaximumStringLength)
            {
                return new ScalarValue(s.Substring(_destructorLimits.MaximumStringLength));
            }

            return new ScalarValue(_value);
        }
    }
}
