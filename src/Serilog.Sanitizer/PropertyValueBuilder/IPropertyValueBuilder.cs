using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public interface IPropertyValueBuilder
    {
        LogEventPropertyValue CreateValue(object value);
    }
}
