using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
