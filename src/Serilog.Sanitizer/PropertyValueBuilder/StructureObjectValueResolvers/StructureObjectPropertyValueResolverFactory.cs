using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public class StructureObjectPropertyValueResolverFactory
    {
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public StructureObjectPropertyValueResolverFactory(SanitizeContext context, DestructorLimits destructorLimits)
        {
            _context = context;
            _destructorLimits = destructorLimits;
        }

        public bool IsIgnoredType(Type type)
        {
            return _context.IsIgnoredType(type);
        }

        public bool IsIgnoredType(object value)
        {
            if (value == null)
            {
                return false;
            }

            return _context.IsIgnoredType(value.GetType());
        }

        public IStructureObjectPropertyValueResolver CreatePropertyValueResolver(object value, int depth)
        {
            if (TypeUtil.IsBuiltinType(value))
            {
                return new BuiltinTypePropertyValueResolver(value, _destructorLimits);
            }
            else if (value is IEnumerable)
            {
                return new EnumerablePropertyValueResolver(value, depth, _destructorLimits, _context, this);
            }

            return new ComplexObjectPropertyValueResolver(value, depth, _destructorLimits, _context, this);
        }
    }
}
