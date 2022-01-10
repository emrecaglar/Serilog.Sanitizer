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
        public void SanitizeViaExpression_ShouldBe_SanitizePersonProperties_When_SanitizeViaLambda()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Typed<Person>()
                                    .Sanitize(x => x.Name, x => string.Concat(x.Substring(0,2),"***"))
                                    .Sanitize(x => x.Surname, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Sanitize(x => x.Email, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Sanitize(x => x.Phone, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Continue()
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new Person { Name = "Bruce", Surname = "Lee", Email = "brucelee@gmai.com", Phone = "5076589898" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object surname = properties["Surname"];
            object phone = properties["Phone"];
            object email = properties["Email"];

            Assert.Equal(model.Name.Substring(0, 2) + "***", name.ToString());
            Assert.Equal(model.Surname.Substring(0, 2) + "***", surname.ToString());
            Assert.Equal(model.Phone.Substring(0, 2) + "***", phone.ToString());
            Assert.Equal(model.Email.Substring(0, 2) + "***", email.ToString());
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeOnlyPhoneProperty_When_SanitizeViaPropertyName()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("Phone", phone => string.Concat(phone.Substring(0, 4), "****", phone.Substring(8, 2)))
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Name = "John", Phone = "5076589898", WorkPhone = "2123256985" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object phone = properties["Phone"];
            object workPhone = properties["WorkPhone"];

            Assert.Equal(model.Name, name.ToString());
            Assert.Equal("5076****98", phone.ToString());
            Assert.Equal("2123256985", workPhone.ToString());
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeMatchAllPhoneProperties_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Pp]hone", phone => string.Concat(phone.Substring(0, 4), "****", phone.Substring(8, 2)))
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Name = "John", Phone = "5076589898", WorkPhone = "2123256985" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object phone = properties["Phone"];
            object workPhone = properties["WorkPhone"];

            Assert.Equal(model.Name, name.ToString());
            Assert.Equal("5076****98", phone.ToString());
            Assert.Equal("2123****85", workPhone.ToString());
        }

        [Fact]
        public void SanitizeViaConstant_ShouldBe_SanitizePassword_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            const string SANITIZE_PASSWORD = "***";

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Pp]assword", SANITIZE_PASSWORD)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Name = "Bruce", Password = "123" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object password = properties["Password"];

            Assert.Equal(model.Name, name.ToString());
            Assert.Equal(SANITIZE_PASSWORD, password.ToString());
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeEmail_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Ee]mail", email =>
                                {
                                    var p = email.Split(new[] { '@' });

                                    return string.Concat(p[0].Substring(0, 2), "****@", p[1].Substring(0, 3), "****");
                                })
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Name = "Bruce", Email = "bruce.lee@gmail.com" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object name = properties["Name"];
            object email = properties["Email"];

            Assert.Equal(model.Name, name.ToString());
            Assert.Equal("br****@gma****", email.ToString());
        }

        [Fact]
        public void SanitizeViaConstant_ShouldBe_SanitizePasswordProperty_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            const string SANITIZE_PASSWORD = "***";

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("[Pp]assword", SANITIZE_PASSWORD)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Name = "Bruce", Password = "123" };

            logger.Information(
                    "Sensitive Information {@Name} {@Password}",
                    model.Name,
                    model.Password
            );

            var name = (events[0].Properties["Name"] as ScalarValue).Value;
            var password = (events[0].Properties["Password"] as ScalarValue).Value;

            Assert.Equal(model.Name, name.ToString());
            Assert.Equal(SANITIZE_PASSWORD, password.ToString());
        }
    }

    class Person
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
