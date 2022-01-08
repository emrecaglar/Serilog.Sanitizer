using Serilog.Sanitizer.PropertyFinder;
using System;

namespace Serilog.Sanitizer
{
    public class SanitizeDefinition
    {
        public Func<object> ValueExpression { get; set; }

        public Func<object, IPropertyFinder> Finder { get; set; }
    }
}
