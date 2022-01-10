using Serilog.Core;
using Serilog.Sanitizer.PropertyFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Serilog.Sanitizer
{
    public class SanitizeContext
    {
        private readonly LoggerConfiguration _configuration;

        public SanitizeContext(LoggerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<Type, List<SanitizeDefinition>> ByModel { get; set; } = new Dictionary<Type, List<SanitizeDefinition>>();

        public List<SanitizeDefinition> ByExpression { get; set; } = new List<SanitizeDefinition>();

        public List<Type> IgnoredTypes { get; set; } = new List<Type>()
        {
            typeof(MulticastDelegate),
            typeof(Type),
            typeof(TypeInfo)
        };

        public List<string> IgnoredProperties { get; set; } = new List<string>();


        public bool TryGetValue(Type model, object value, object property, object propertyValue, out object sanitized)
        {
            List<SanitizeDefinition> list = null;

            if (!ByModel.TryGetValue(model, out list))
            {
                list = ByExpression;
            }

            var sanitizeDefinition = list.FirstOrDefault(x => x.Finder(value).Equal(property));

            if (sanitizeDefinition == null)
            {
                sanitized = null;
                return false;
            }

            var valueExpression = sanitizeDefinition.ValueExpression();

            object sanitizeResult = null;

            if(valueExpression is Func<string, string> sanitizeFunc)
            {
                sanitizeResult = sanitizeFunc($"{propertyValue}");
            }
            else if (valueExpression is LambdaExpression lambdaExpression)
            {
                sanitizeResult = lambdaExpression.Compile().DynamicInvoke(value);
            }
            else if(valueExpression is Delegate func)
            {
                sanitizeResult = func.DynamicInvoke(value);
            }
            else if (valueExpression is string constantValue)
            {
                sanitizeResult = constantValue;
            }

            sanitized = sanitizeResult;

            return true;
        }

        public bool TryGetValue(object value, object property, object propertyValue, out object sanitized)
        {
            var sanitizeDefinition = ByExpression.FirstOrDefault(x => x.Finder(value).Equal(property));

            if (sanitizeDefinition == null)
            {
                sanitized = null;
                return false;
            }

            var valueExpression = sanitizeDefinition.ValueExpression();

            object sanitizeResult = null;

            if (valueExpression is Func<string, string> sanitizeFunc)
            {
                sanitizeResult = sanitizeFunc($"{propertyValue}");
            }
            else if (valueExpression is LambdaExpression lambdaExpression)
            {
                sanitizeResult = lambdaExpression.Compile().DynamicInvoke(value);
            }
            else if (valueExpression is string constantValue)
            {
                sanitizeResult = constantValue;
            }

            sanitized = sanitizeResult;

            return true;
        }


        internal void AddIgnoredProp(string[] props)
        {
            IgnoredProperties.AddRange(
                props.Where(x => !IgnoredProperties.Contains(x))
            );
        }

        public void AddSanitizeViaRegex(string pattern, string value)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(new Regex(pattern)),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeViaRegex(string pattern, Func<string, string> value)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(new Regex(pattern)),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeProp(string prop, string value)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(prop),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeProp(string prop, Func<string, string> value)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(prop),
                ValueExpression = () => value
            });
        }

        public void AddTypedSanitize<TModel>(Expression<Func<TModel, object>> prop, string value)
        {
            if (!ByModel.ContainsKey(typeof(TModel)))
            {
                ByModel.Add(typeof(TModel), new List<SanitizeDefinition>());
            }

            ByModel[typeof(TModel)].Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(x).CreateFinder((PropertyInfo)((MemberExpression)prop.Body).Member),
                ValueExpression = () => value
            });
        }

        public void AddTypedSanitize<TModel>(Expression<Func<TModel, object>> prop, Func<TModel, string> value)
        {
            if (!ByModel.ContainsKey(typeof(TModel)))
            {
                ByModel.Add(typeof(TModel), new List<SanitizeDefinition>());
            }

            ByModel[typeof(TModel)].Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(x).CreateFinder((PropertyInfo)((MemberExpression)prop.Body).Member),
                ValueExpression = () => value
            });
        }

        public void AddIfNotExistIgnoredType<T>()
        {
            if (!IgnoredTypes.Contains(typeof(T)))
            {
                IgnoredTypes.Add(typeof(T));
            }
        }


        public void AddDestructure(IDestructuringPolicy policy)
        {
            _configuration.Destructure.With(policy);
        }

        internal void AddEnrich(SanitizeEnrich sanitizeEnrich)
        {
            _configuration.Enrich.With(sanitizeEnrich);
        }

        public bool IsIgnoredType(Type propertyType)
        {
            return IgnoredTypes.Contains(propertyType) && IgnoredTypes.Any(x => x.BaseType == propertyType);
        }

        public bool IsIgnoredProp(string propName)
        {
            return IgnoredProperties.Contains(propName);
        }

        public LoggerConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}
