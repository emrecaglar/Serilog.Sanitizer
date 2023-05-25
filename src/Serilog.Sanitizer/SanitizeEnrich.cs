using Serilog.Core;
using Serilog.Events;
using Serilog.Sanitizer.PropertyValueBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer
{
    public class SanitizeEnrich : ILogEventEnricher
    {
        private readonly SanitizeContext _context;
        private readonly LoggerConfiguration _loggerConfiguration;

        public SanitizeEnrich(SanitizeContext context, LoggerConfiguration loggerConfiguration)
        {
            _context = context;
            _loggerConfiguration = loggerConfiguration;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.Properties.ToList().ForEach(property => 
            {
                var propertyValueBuilderFactory = new PropertyValueBuilderFactory(
                   _context,
                   DestructorLimits.GetFromLoggerConfiguration(_loggerConfiguration)
                );

                var propertyValueBuilder = propertyValueBuilderFactory.CreatePropertyValueBuilder(property);

                var result = propertyValueBuilder.CreateValue(property);

                logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, result));
            });
        }
    }
}
