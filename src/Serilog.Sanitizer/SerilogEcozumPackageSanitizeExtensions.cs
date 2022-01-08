using Serilog.Sanitizer;

namespace Serilog
{
    public static class SerilogEcozumPackageSanitizeExtensions
    {
        public static SanitizingConfiguration Sanitizer(this LoggerConfiguration configuration)
        {
            return new SanitizingConfiguration(configuration);
        }
    }
}
