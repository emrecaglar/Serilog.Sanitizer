using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer
{
    public class SanitizeType<TModel>
    {
        private readonly SanitizeContext _context;
        private readonly SanitizeConfiguration _sanitizeConfiguration;

        public SanitizeType(SanitizeContext context, SanitizeConfiguration sanitizeConfiguration)
        {
            _context = context;
            _sanitizeConfiguration = sanitizeConfiguration;
        }

        public SanitizeType<TModel> Sanitize<TProperty>(Expression<Func<TModel, TProperty>> prop, string value)
        {
            _context.AddTypedSanitize(prop, value);

            return this;
        }

        public SanitizeType<TModel> Sanitize<TProperty>(Expression<Func<TModel, TProperty>> prop, Func<TProperty, string> value)
        {
            _context.AddTypedSanitize(prop, value);

            return this;
        }

        public SanitizeConfiguration Continue()
        {
            return _sanitizeConfiguration;
        }
    }
}
