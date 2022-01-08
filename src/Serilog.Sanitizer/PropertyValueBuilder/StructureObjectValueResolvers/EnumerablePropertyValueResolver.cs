using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public class EnumerablePropertyValueResolver : IStructureObjectPropertyValueResolver
    {
        private readonly object _value;
        private readonly int _depth;
        private readonly DestructorLimits _destructorLimits;
        private readonly SanitizeContext _context;
        private readonly StructureObjectPropertyValueResolverFactory _valueResolverFactory;

        public EnumerablePropertyValueResolver(
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

            var enumerable = (IEnumerable)_value;

            var sequenceValue = new List<LogEventPropertyValue>();

            int collectionCount = 0;
            foreach (var item in enumerable)
            {
                if (collectionCount > _destructorLimits.MaximumCollectionCount)
                {
                    break;
                }

                if (TypeUtil.IsBuiltinType(item))
                {
                    sequenceValue.Add(new ScalarValue(item));
                }
                else
                {
                    var structured = new List<LogEventProperty>();

                    var enumerationPropertties = TypeUtil.UsableProperties(item, _context);

                    foreach (var p in enumerationPropertties)
                    {
                        var itemPropertyValue = p.GetValue(item);

                        //block circular reference
                        if (itemPropertyValue == parentObject)
                        {
                            structured.Add(new LogEventProperty(p.Name, new ScalarValue(null)));
                        }
                        else
                        {
                            var valueResolver = _valueResolverFactory.CreatePropertyValueResolver(itemPropertyValue, (_depth + 1));

                            structured.Add(new LogEventProperty(p.Name, valueResolver.GetValue(item)));
                        }
                    }

                    sequenceValue.Add(new StructureValue(structured));
                }

                collectionCount++;
            }

            return new SequenceValue(sequenceValue);

        }
    }
}
