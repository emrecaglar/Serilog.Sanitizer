using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json;

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
            if(value is JsonDocument || value is JsonElement)
            {
                return new SystemTextJsonObjectPropertyValueBuilder(this, _context, _destructorLimits);
            }
            if (value is JToken)
            {
                return new JTokenPropertyValueBuilder(this, _context, _destructorLimits);
            }
            else if (value is IFormCollection)
            {
                return new FormCollectionPropertyValueBuilder(this, _context, _destructorLimits);
            }
            else if(value is NameValueCollection)
            {
                return new NameValueCollectionPropertyValueBuilder(this, _context, _destructorLimits);
            }
            else if(value is KeyValuePair<string, LogEventPropertyValue>)
            {
                return new SerilogPropertyValueBuilder(_context);
            }

            return new StructureObjectPropertyValueBuilder(this, _context, _destructorLimits);
        }
    }
}
