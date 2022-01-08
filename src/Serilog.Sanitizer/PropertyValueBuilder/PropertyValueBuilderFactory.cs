using Newtonsoft.Json.Linq;

#if NETCOREAPP2_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif

using System.Collections.Specialized;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class PropertyValueBuilderFactory
    {
        private readonly SanitizeContext _context;
        private readonly DestructorLimits _destructorLimits;

        public PropertyValueBuilderFactory(SanitizeContext context, DestructorLimits destructorLimits)
        {
            _context = context;
            _destructorLimits = destructorLimits;
        }

        public IPropertyValueBuilder CreatePropertyValueBuilder(object value)
        {
            if (value == null)
            {
                return new NullPropertyValueBuilder();
            }

            if (value is JToken)
            {
                return new JTokenPropertyValueBuilder(this, _context, _destructorLimits);
            }
#if NETCOREAPP2_0_OR_GREATER
            else if (value is IFormCollection)
            {
                return new FormCollectionPropertyValueBuilder(this, _context, _destructorLimits);
            }
#endif
            else if(value is NameValueCollection)
            {
                return new NameValueCollectionPropertyValueBuilder(this, _context, _destructorLimits);
            }
            return new StructureObjectPropertyValueBuilder(this, _context, _destructorLimits);
        }
    }
}
