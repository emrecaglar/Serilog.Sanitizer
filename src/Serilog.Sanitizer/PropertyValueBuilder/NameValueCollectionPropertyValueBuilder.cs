using Serilog.Events;
using System.Collections.Specialized;
using System.Linq;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class NameValueCollectionPropertyValueBuilder : IPropertyValueBuilder
    {
        private readonly SanitizeContext _context;

        public NameValueCollectionPropertyValueBuilder(PropertyValueBuilderFactory propertyValueBuilderFactory, SanitizeContext context, DestructorLimits destructorLimits)
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

                var form = (NameValueCollection)value;

                var formvalues = form.AllKeys.Select(key =>
                {
                    if (_context.TryGetValue(form, key, form[key], out object sanitized))
                    {
                        return new StructureValue(
                            new[] { new LogEventProperty(key, new ScalarValue(sanitized)) }
                        );
                    }

                    return new StructureValue(
                            new[] { new LogEventProperty(key, new ScalarValue(form[key])) }
                    );
                });

                return new SequenceValue(formvalues);
            }
            catch
            {
                return new ScalarValue(null);
            }
        }
    }
}
