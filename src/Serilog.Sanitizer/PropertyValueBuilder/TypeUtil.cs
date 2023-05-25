using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class TypeUtil
    {
        static readonly HashSet<Type> BuiltInScalarTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(char),
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
            typeof(string),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Guid), typeof(Uri), typeof(Version)
        };

        public static bool IsBuiltinType(object val)
        {
            return val == null || IsBuiltinType(val.GetType());
        }

        public static bool IsBuiltinType(Type t)
        {
            return BuiltInScalarTypes.Contains(t) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) || t.IsEnum;
        }


        public static List<PropertyInfo> UsableProperties(object value, SanitizeContext context)
        {
            return value.GetType()
                        .GetRuntimeProperties()
                        .Where(x => x.CanRead && !context.IsIgnoredType(x.PropertyType) && !context.IsIgnoredProp(x.Name))
                        .ToList();
        }
    }
}
