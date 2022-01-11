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
        public void SanitizeViaExpression_ShouldBe_SanitizeIgnoreCaseRegexOptions_When_SanitizeViaLambda()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex(new[] { "cvv", "cvc", "cvv" }, "***", ignoreCase: true)
                                .SanitizeViaRegex("expireYear", "****", ignoreCase: true)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { Cvv = "000", ExpireYear = "2026" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object cvv = properties["Cvv"];
            object expYear = properties["ExpireYear"];

            Assert.Equal("***", cvv.ToString());
            Assert.Equal("****", expYear.ToString());
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeCardInformation_And_RemoveExpireProps_When_SanitizeViaLambda()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex(new[] { "[Cc]vv", "[Cc]vc", "[Cc]vv" }, "***")
                                .SanitizeViaRegex(new[] { "[Cc]ard", "[Cc]ard[Nn]umber", "[Pp]an", "[Cc]ard[Nn]o" }, x => string.Concat(x.Substring(0, 6), "******", x.Substring(12, 4)))
                                .IgnoreProp(ignoreCase: true, "expmonth", "expyear", "expire", "expireYear", "expireMonth")
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new { CardNumber = "4355084355084358", Cvv = "000", ExpireMonth = "12", ExpireYear = "2026" };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var properties = (events[0].Properties["p"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object cardNumber = properties["CardNumber"];
            object Cvv = properties["Cvv"];

            Assert.Equal("435508******4358", cardNumber.ToString());
            Assert.Equal("***", Cvv.ToString());
            Assert.False(properties.ContainsKey("ExpireMonth"));
            Assert.False(properties.ContainsKey("ExpireYear"));
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizePersonProperties_When_SanitizeViaLambda()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Typed<Person>()
                                    .Sanitize(x => x.Name, x => string.Concat(x.Substring(0, 2), "***"))
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
        public void SanitizeViaExpression_ShouldBe_SanitizePersonProperties_And_SkipOtherType_When_SanitizeViaLambda()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Typed<Person>()
                                    .Sanitize(x => x.Name, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Sanitize(x => x.Surname, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Sanitize(x => x.Email, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Sanitize(x => x.Phone, x => string.Concat(x.Substring(0, 2), "***"))
                                    .Continue()
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new Person { Name = "Bruce", Surname = "Lee", Email = "brucelee@gmai.com", Phone = "5076589898" };
            var company = new Company { Name = "Foo corp.", Email = "foo@bar.com", Phone = "2226663355" };

            logger.Information(
                    "Sensitive Information {@p1} {@p2}",
                    model,
                    company
            );

            var p1_properties = (events[0].Properties["p1"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);
            var p2_properties = (events[0].Properties["p2"] as StructureValue)?.Properties.ToDictionary(x => x.Name, x => ((ScalarValue)x.Value).Value);

            object p1_name = p1_properties["Name"];
            object p1_surname = p1_properties["Surname"];
            object p1_phone = p1_properties["Phone"];
            object p1_email = p1_properties["Email"];

            object p2_name = p2_properties["Name"];
            object p2_phone = p2_properties["Phone"];
            object p2_email = p2_properties["Email"];

            Assert.Equal(model.Name.Substring(0, 2) + "***", p1_name.ToString());
            Assert.Equal(model.Surname.Substring(0, 2) + "***", p1_surname.ToString());
            Assert.Equal(model.Phone.Substring(0, 2) + "***", p1_phone.ToString());
            Assert.Equal(model.Email.Substring(0, 2) + "***", p1_email.ToString());

            Assert.Equal(company.Name, p2_name.ToString());
            Assert.Equal(company.Phone, p2_phone.ToString());
            Assert.Equal(company.Email, p2_email.ToString());
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

    class Company
    {
        public string Phone { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
