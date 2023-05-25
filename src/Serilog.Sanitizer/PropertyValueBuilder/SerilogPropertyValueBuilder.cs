using Serilog.Events;
using System.Collections.Generic;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class SerilogPropertyValueBuilder : IPropertyValueBuilder
    {
        private readonly SanitizeContext _context;
        
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

                if(property.Value is ScalarValue sv)
                {
                    if (_context.TryGetValue(property, property.Key, sv.Value, out object sanitized))
                    {
                        return new ScalarValue(sanitized);
                    }

                    return sv;
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
