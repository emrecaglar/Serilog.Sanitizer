using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyFinder
{
    public interface IPropertyFinder
    {
        bool Equal(object property);
    }
}
