using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class SerilogPropertyValueBuilder : IPropertyValueBuilder
    {
        private SanitizeContext _context;
        
        public SerilogPropertyValueBuilder(SanitizeContext context)
        {
            _context = context;
        }

        public LogEventPropertyValue CreateValue(object value)
        {
            try
            {
                if (value == null)
                {
                    return new ScalarValue(null);
                }

                var property = (KeyValuePair<string, LogEventPropertyValue>)value;

                if(property.Value is ScalarValue)
                {
                    if (_context.TryGetValue(property, property.Key, ((ScalarValue)property.Value).Value, out object sanitized))
                    {
                        return new ScalarValue(sanitized);
                    }

                    return (ScalarValue)property.Value;
                }

                return property.Value;
            }
            catch
            {
                return new ScalarValue(null);
            }
        }
    }
}
