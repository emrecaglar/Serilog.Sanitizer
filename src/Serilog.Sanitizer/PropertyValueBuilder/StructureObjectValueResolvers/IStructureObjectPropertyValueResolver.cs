using Serilog.Events;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public interface IStructureObjectPropertyValueResolver
    {
        LogEventPropertyValue GetValue(object parentObject);
    }
}
