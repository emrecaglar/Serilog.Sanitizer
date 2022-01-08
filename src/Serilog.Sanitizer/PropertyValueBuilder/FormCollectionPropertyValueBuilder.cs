#if NETCOREAPP2_0_OR_GREATER
using Serilog.Events;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class FormCollectionPropertyValueBuilder : IPropertyValueBuilder
    {
        private readonly PropertyValueBuilderFactory _propertyValueBuilderFactory;
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public FormCollectionPropertyValueBuilder(PropertyValueBuilderFactory propertyValueBuilderFactory, SanitizeContext context, DestructorLimits destructorLimits)
        {
            _propertyValueBuilderFactory = propertyValueBuilderFactory;
            _context = context;
            _destructorLimits = destructorLimits;
        }

        public LogEventPropertyValue CreateValue(object value)
        {
            try
            {
                if (value == null)
                {
                    return new ScalarValue(null);
                }

                var form = (IFormCollection)value;

                var formvalues = form.Select(x =>
                {
                    if (_context.TryGetValue(x, x.Key, x.Value, out object sanitized))
                    {
                        return new StructureValue(
                            new[] { new LogEventProperty(x.Key, new ScalarValue(sanitized)) }
                        );
                    }

                    return new StructureValue(
                            new[] { new LogEventProperty(x.Key, new ScalarValue(x.Value.ToString())) }
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
#endif