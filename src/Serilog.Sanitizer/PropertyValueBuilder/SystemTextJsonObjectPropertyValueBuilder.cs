using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class SystemTextJsonObjectPropertyValueBuilder : IPropertyValueBuilder
    {
        static readonly HashSet<JsonValueKind> BuiltInScalarJsonTypes = new HashSet<JsonValueKind>
        {
            JsonValueKind.String,
            JsonValueKind.Number,
            JsonValueKind.True,
            JsonValueKind.False,
            JsonValueKind.Undefined,
            JsonValueKind.Null
        };

        private readonly PropertyValueBuilderFactory _propertyValueBuilderFactory;
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public SystemTextJsonObjectPropertyValueBuilder(PropertyValueBuilderFactory propertyValueBuilderFactory, SanitizeContext context, DestructorLimits destructorLimits)
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

                JsonElement element;

                if (value is JsonDocument document)
                {
                    element = document.RootElement;
                }
                else if (value is JsonElement elm)
                {
                    element = elm;
                }
                else
                {
                    return new ScalarValue(null);
                }

                if (BuiltInScalarJsonTypes.Contains(element.ValueKind))
                {
                    return new ScalarValue(element.ToString());
                }

                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        {
                            var logEventProperties = element.EnumerateObject()
                                                            .Select(x =>
                                                            {
                                                                if (_context.TryGetValue(value, x, x.Value, out object sanitized2))
                                                                {
                                                                    return new LogEventProperty(x.Name, new ScalarValue(sanitized2));
                                                                }

                                                                return new LogEventProperty(x.Name, CreateValue(x.Value));
                                                            })
                                                            .ToList();

                            return new StructureValue(logEventProperties);
                        }
                    case JsonValueKind.Array:
                        {
                            var logEventProperties = element.EnumerateArray().Select(x => CreateValue(x)).ToList();
                            return new SequenceValue(logEventProperties);
                        }
                    default:
                        return new ScalarValue(null);
                }
            }
            catch
            {
                return new ScalarValue(null);
            }
        }
    }
}
