using Serilog.Core;
using Serilog.Events;
using Serilog.Sanitizer.PropertyValueBuilder;

namespace Serilog.Sanitizer
{
    public class SanitizingDestructurePolicy : IDestructuringPolicy
    {
        private readonly SanitizeContext _context;
        private readonly LoggerConfiguration _loggerConfiguration;

        public SanitizingDestructurePolicy(SanitizeContext context, LoggerConfiguration loggerConfiguration)
        {
            _context = context;
            _loggerConfiguration = loggerConfiguration;
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            var propertyValueBuilderFactory = new PropertyValueBuilderFactory(
                _context, 
                DestructorLimits.GetFromLoggerConfiguration(_loggerConfiguration)
            );

            var propertyValueBuilder = propertyValueBuilderFactory.CreatePropertyValueBuilder(value);

            result = propertyValueBuilder.CreateValue(value);

            return true;
        }
    }
}
