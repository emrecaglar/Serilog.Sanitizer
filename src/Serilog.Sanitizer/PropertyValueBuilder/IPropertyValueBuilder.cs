using Serilog.Events;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public interface IPropertyValueBuilder
    {
        LogEventPropertyValue CreateValue(object value);
    }
}
