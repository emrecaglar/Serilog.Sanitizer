using Serilog.Events;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class NullPropertyValueBuilder : IPropertyValueBuilder
    {
        public LogEventPropertyValue CreateValue(object value)
        {
            return new ScalarValue(null);
        }
    }
}
