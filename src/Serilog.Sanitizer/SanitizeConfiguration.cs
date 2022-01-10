using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Serilog.Sanitizer
{
    public class SanitizeConfiguration
    {
        private readonly SanitizeContext _context;
        private readonly LoggerConfiguration _loggerConfiguration;

        public SanitizeConfiguration(LoggerConfiguration loggerConfiguration)
        {
            _context = new SanitizeContext(loggerConfiguration);

            _loggerConfiguration = loggerConfiguration;
        }

        public SanitizeConfiguration IgnoreProp(params string[] props)
        {
            _context.AddIgnoredProp(props);

            return this;
        }


        public SanitizeConfiguration SanitizeViaRegex(string pattern, string value)
        {
            _context.AddSanitizeViaRegex(pattern, value);

            return this;
        }

        public SanitizeConfiguration SanitizeViaRegex(string pattern, Func<string, string> value)
        {
            _context.AddSanitizeViaRegex(pattern, value);

            return this;
        }

        public SanitizeConfiguration Sanitize(string prop, string value)
        {
            _context.AddSanitizeProp(prop, value);

            return this;
        }

        public SanitizeConfiguration Sanitize(string prop, Func<string, string> value)
        {
            _context.AddSanitizeProp(prop, value);

            return this;
        }

        public SanitizeType<TModel> Typed<TModel>()
        {
            return new SanitizeType<TModel>(_context, this);
        }

        //public SanitizeConfiguration Sanitize<TModel>(string value, params Expression<Func<TModel, object>>[] props)
        //{
        //    foreach (var prop in props)
        //    {
        //        _context.AddTypedSanitize(prop, value); 
        //    }

        //    return this;
        //}

        //public SanitizeConfiguration Sanitize<TModel>(Func<TModel, string> value, params Expression<Func<TModel, object>>[] props)
        //{
        //    foreach (var prop in props)
        //    {
        //        _context.AddTypedSanitize(prop, value); 
        //    }

        //    return this;
        //}


        public LoggerConfiguration Build()
        {
            _context.AddDestructure(new SanitizeDestructurePolicy(_context, _loggerConfiguration));

            _context.AddEnrich(new SanitizeEnrich(_context, _loggerConfiguration));

            return _context.GetConfiguration();
        }
    }
}
