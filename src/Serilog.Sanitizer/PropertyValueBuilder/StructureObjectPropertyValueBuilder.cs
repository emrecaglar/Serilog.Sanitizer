using Serilog.Events;
using Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class StructureObjectPropertyValueBuilder : IPropertyValueBuilder
    {
        private readonly PropertyValueBuilderFactory _propertyValueBuilderFactory;
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public StructureObjectPropertyValueBuilder(PropertyValueBuilderFactory propertyValueBuilderFactory, SanitizeContext context, DestructorLimits destructorLimits)
        {
            _propertyValueBuilderFactory = propertyValueBuilderFactory;
            _context = context;
            _destructorLimits = destructorLimits;
        }

        public LogEventPropertyValue CreateValue(object value)
        {
            return CreateValueRecursive(value, depth: 0);
        }

        private LogEventPropertyValue CreateValueRecursive(object value, int depth)
        {
            var structureObjectPropertyValueResolverFactory = new StructureObjectPropertyValueResolverFactory(_context, _destructorLimits);

            if (structureObjectPropertyValueResolverFactory.IsIgnoredType(value))
            {
                return new ScalarValue(null);
            }

            var propertyValueResolver = structureObjectPropertyValueResolverFactory.CreatePropertyValueResolver(value, depth);

            return propertyValueResolver.GetValue(value);
        }
    }
}
