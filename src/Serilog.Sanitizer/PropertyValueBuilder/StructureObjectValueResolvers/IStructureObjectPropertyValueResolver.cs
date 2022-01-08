using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder.StructureObjectValueResolvers
{
    public interface IStructureObjectPropertyValueResolver
    {
        LogEventPropertyValue GetValue(object parentObject);
    }
}
