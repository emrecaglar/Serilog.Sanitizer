using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Serilog.Sanitizer.Tests
{
    public class SanitizerTests
    {
        [Fact]
        public void SanitizeViaRegex_ShouldBe_SanitizeAsConstantValue_When_SturcturedParameter()
        {
            List<LogEvent> events = new List<LogEvent>();

            const string SANITIZE_PASSWORD = "***";

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Pp]assword", SANITIZE_PASSWORD)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var vendor = new { Name = "John", Password = "123" };

            logger.Information(
                    "Sensitive Information {@vendor}",
                    vendor
            );

            var properties = (events[0].Properties["vendor"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object password = properties["Password"];

            Assert.Equal(name.ToString(), vendor.Name);
            Assert.Equal(password.ToString(), SANITIZE_PASSWORD);
        }

        [Fact]
        public void SanitizeViaRegex_ShouldBe_SanitizeAsConstantValue_When_ScalarParameter()
        {
            List<LogEvent> events = new List<LogEvent>();

            const string SANITIZE_PASSWORD = "***";

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Pp]assword", SANITIZE_PASSWORD)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var vendor = new { Name = "John", Password = "123" };

            logger.Information(
                    "Sensitive Information {@Name} {@Password}",
                    vendor.Name,
                    vendor.Password
            );

            var name = (events[0].Properties["Name"] as ScalarValue).Value;
            var password = (events[0].Properties["Password"] as ScalarValue).Value;

            Assert.Equal(name.ToString(), vendor.Name);
            Assert.Equal(password.ToString(), SANITIZE_PASSWORD);
        }
    }
}
