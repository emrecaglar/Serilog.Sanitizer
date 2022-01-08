using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public class ComplexObjectPropertyValueResolver : IStructureObjectPropertyValueResolver
    {
        private readonly object _value;
        private readonly int _depth;
        private readonly DestructorLimits _destructorLimits;
        private readonly SanitizeContext _context;
        private readonly StructureObjectPropertyValueResolverFactory _valueResolverFactory;

        public ComplexObjectPropertyValueResolver(
            object value,
            int depth,
            DestructorLimits destructorLimits,
            SanitizeContext context,
            StructureObjectPropertyValueResolverFactory valueResolverFactory)
        {
            _value = value;
            _depth = depth;
            _destructorLimits = destructorLimits;
            _context = context;
            _valueResolverFactory = valueResolverFactory;
        }

        public LogEventPropertyValue GetValue(object parentObject)
        {
            if (_depth >= _destructorLimits.MaximumDestructuringDepth)
            {
                return new ScalarValue(null);
            }

            var valueType = _value.GetType();

            var properties = TypeUtil.UsableProperties(_value, _context);

            var structureProperties = new List<LogEventProperty>();

            properties.ForEach(property =>
            {
                if (_context.TryGetValue(valueType, _value, property, property.GetValue(_value), out object sanitized1))
                {
                    structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(sanitized1)));
                }
                else if(_context.TryGetValue(_value, property, property.GetValue(_value), out object sanitized2))
                {
                    structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(sanitized2)));
                }
                else
                {
                    try
                    {
                        var propertyValue = property.GetValue(_value);

                        var valueResolver = _valueResolverFactory.CreatePropertyValueResolver(propertyValue, (_depth + 1));

                        structureProperties.Add(new LogEventProperty(property.Name, valueResolver.GetValue(_value)));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            return new StructureValue(structureProperties, valueType.Name);
        }
    }
}
