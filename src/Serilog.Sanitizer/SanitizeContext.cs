using Serilog.Core;
using Serilog.Debugging;
using Serilog.Sanitizer.PropertyFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Serilog.Sanitizer
{
    public class IgnoredPropertyList
    {
        public StringComparison Comparison { get; set; }

        public string[] Properties { get; set; }

        public bool OnlyPrimitive { get; set; }
    }

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

        public List<IgnoredPropertyList> IgnoredProperties { get; set; } = new List<IgnoredPropertyList>();


        public bool TryGetValue(Type model, object value, object property, object propertyValue, out object sanitized)
        {
            try
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

                if (valueExpression is Func<string, string> sanitizeFunc)
                {
                    sanitizeResult = sanitizeFunc($"{propertyValue}");
                }
                else if (valueExpression is LambdaExpression lambdaExpression)
                {
                    sanitizeResult = lambdaExpression.Compile().DynamicInvoke(value);
                }
                else if (valueExpression is Delegate func)
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
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.ToString());

                sanitized = "[sanitize error]";

                return true;
            }
        }

        public bool TryGetValue(object value, object property, object propertyValue, out object sanitized)
        {
            try
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
            catch (Exception ex)
            {
                sanitized = "[sanitize error]";

                SelfLog.WriteLine(ex.ToString());

                return true;
            }
        }

        internal void AddIgnoredProp(string[] props, bool ignoreCase = false, bool onlyPrimitive = false)
        {
            IgnoredProperties.Add(new IgnoredPropertyList
            {
                Comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture,
                Properties = props,
                OnlyPrimitive = onlyPrimitive
            });
        }

        //internal void AddIgnoredProp(string[] props)
        //{
        //    IgnoredProperties.Properties.AddRange(
        //        props.Where(x => !IgnoredProperties.Properties.Contains(x))
        //    );
        //}

        public void AddSanitizeViaRegex(string pattern, string value, bool ignoreCase = false, bool onlyPrimitive = false)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(
                    new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None),
                    new PropertyFinderConfigration() { IgnoreCase = ignoreCase, OnlyPrimitive = onlyPrimitive }
                ),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeViaRegex(string pattern, Func<string, string> value, bool ignoreCase = false, bool onlyPrimitive = false)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(
                    new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None),
                    new PropertyFinderConfigration() { IgnoreCase = ignoreCase, OnlyPrimitive = onlyPrimitive }),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeProp(string prop, string value, bool ignoreCase, bool onlyPrimitive = false)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(prop, new PropertyFinderConfigration() { IgnoreCase = ignoreCase, OnlyPrimitive = onlyPrimitive }),
                ValueExpression = () => value
            });
        }

        public void AddSanitizeProp(string prop, Func<string, string> value, bool ignoreCase = false, bool onlyPrimitive = false)
        {
            ByExpression.Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(null).CreateFinder(prop, new PropertyFinderConfigration() { IgnoreCase = ignoreCase, OnlyPrimitive = onlyPrimitive }),
                ValueExpression = () => value
            });
        }

        public void AddTypedSanitize<TModel, TProperty>(Expression<Func<TModel, TProperty>> prop, string value)
        {
            if (!ByModel.ContainsKey(typeof(TModel)))
            {
                ByModel.Add(typeof(TModel), new List<SanitizeDefinition>());
            }

            ByModel[typeof(TModel)].Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(x).CreateFinder((PropertyInfo)((MemberExpression)prop.Body).Member, new PropertyFinderConfigration()),
                ValueExpression = () => value
            });
        }

        public void AddTypedSanitize<TModel, TProperty>(Expression<Func<TModel, TProperty>> prop, Func<TProperty, string> value)
        {
            if (!ByModel.ContainsKey(typeof(TModel)))
            {
                ByModel.Add(typeof(TModel), new List<SanitizeDefinition>());
            }

            ByModel[typeof(TModel)].Add(new SanitizeDefinition
            {
                Finder = x => new PropertyFinderFactory(x).CreateFinder((PropertyInfo)((MemberExpression)prop.Body).Member, new PropertyFinderConfigration()),
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
            return IgnoredProperties.Any(list => list.Properties.Any(p => p.Equals(propName, list.Comparison)));
        }

        public LoggerConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}
