using Newtonsoft.Json.Linq;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class JTokenPropertyValueBuilder : IPropertyValueBuilder
    {
        static readonly HashSet<JTokenType> BuiltInScalarJsonTypes = new HashSet<JTokenType>
        {
            JTokenType.String,
            JTokenType.Integer,
            JTokenType.Float,
            JTokenType.Date,
            JTokenType.TimeSpan,
            JTokenType.Boolean,
            JTokenType.Bytes,
            JTokenType.Guid,
            JTokenType.Uri,
            JTokenType.Undefined,
            JTokenType.None,
            JTokenType.Null
        };

        private readonly PropertyValueBuilderFactory _propertyValueBuilderFactory;
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public JTokenPropertyValueBuilder(PropertyValueBuilderFactory propertyValueBuilderFactory, SanitizeContext context, DestructorLimits destructorLimits)
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

                var jtoken = (JToken)value;

                if (BuiltInScalarJsonTypes.Contains(jtoken.Type))
                {
                    return new ScalarValue(jtoken.ToString());
                }

                switch (jtoken.Type)
                {
                    case JTokenType.Object:
                        {
                            var jobj = (JObject)value;
                            var logEventProperties = jobj.Properties()
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
                    case JTokenType.Array:
                        {
                            var jarr = (JArray)value;
                            var logEventProperties = jarr.Select(x => CreateValue(x)).ToList();
                            return new SequenceValue(logEventProperties);
                        }
                    case JTokenType.Property:
                        {
                            var jprop = (JProperty)value;

                            return CreateValue(jprop.Value);
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
