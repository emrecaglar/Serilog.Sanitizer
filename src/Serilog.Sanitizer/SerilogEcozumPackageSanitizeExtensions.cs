using Serilog.Sanitizer;

namespace Serilog
{
    public static class SerilogEcozumPackageSanitizeExtensions
    {
        public static SanitizeConfiguration Sanitizer(this LoggerConfiguration configuration)
        {
            return new SanitizeConfiguration(configuration);
        }
    }
}
