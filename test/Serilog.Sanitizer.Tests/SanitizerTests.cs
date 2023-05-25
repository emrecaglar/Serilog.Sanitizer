using Newtonsoft.Json;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        public void SanitizeOnlyPrimitiveIsTrue_ShouldBe_SkipCardComplexType_SanitizeCreditCardPrimitiveProperty_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("fullName", x =>
                                {
                                    var items = x.Split(' ');

                                    return $"{items[0].Substring(0, 2)}**** {items[1].Substring(0, 2)}****";
                                }, ignoreCase: true)
                                .SanitizeViaRegex("creditCard", "#### #### #### ####", ignoreCase: true, onlyPrimitive: true)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new
            {
                OrderNumber = Guid.NewGuid(),
                FullName = "Sibel Çağlar",
                CreatedAt = DateTime.Now,
                CreditCard = new
                {
                    CreditCard = "4355084355084358",
                    Cvv = "000",
                    ExpireMonth = "12",
                    ExpireYear = "2026"
                }
            };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var cardNumberSanitized = (ScalarValue)((LogEventProperty[])((StructureValue)((LogEventProperty[])((StructureValue)events[0].Properties["p"]).Properties)[3].Value).Properties)[0].Value;

            Assert.Equal("#### #### #### ####", cardNumberSanitized.Value.ToString());
        }

        [Fact]
        public void SanitizeOnlyPrimitiveIsTrue_ShouldBe_SkipCardJsonType_SanitizeCreditCardPrimitiveProperty_When_SanitizeViaRegex()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .SanitizeViaRegex("fullName", x =>
                                {
                                    var items = x.Split(' ');

                                    return $"{items[0].Substring(0, 2)}**** {items[1].Substring(0, 2)}****";
                                }, ignoreCase: true)
                                .SanitizeViaRegex("creditCard", "#### #### #### ####", ignoreCase: true, onlyPrimitive: true)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
            {
                OrderNumber = Guid.NewGuid(),
                FullName = "Sibel Çağlar",
                CreatedAt = DateTime.Now,
                CreditCard = new
                {
                    CreditCard = "4355084355084358",
                    Cvv = "000",
                    ExpireMonth = "12",
                    ExpireYear = "2026"
                }
            }));

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var cardNumberSanitized = (ScalarValue)((LogEventProperty[])((StructureValue)((LogEventProperty[])((StructureValue)events[0].Properties["p"]).Properties)[3].Value).Properties)[0].Value;

            Assert.Equal("#### #### #### ####", cardNumberSanitized.Value.ToString());
        }

        [Fact]
        public void SanitizeOnlyPrimitiveIsTrue_ShouldBe_SkipCardComplexType_SanitizeCreditCardPrimitiveProperty_When_SanitizeProperty()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("fullName", x =>
                                {
                                    var items = x.Split(' ');

                                    return $"{items[0].Substring(0, 2)}**** {items[1].Substring(0, 2)}****";
                                }, ignoreCase: true)
                                .Sanitize("creditCard", "#### #### #### ####", ignoreCase: true, onlyPrimitive: true)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new
            {
                OrderNumber = Guid.NewGuid(),
                FullName = "Sibel Çağlar",
                CreatedAt = DateTime.Now,
                CreditCard = new
                {
                    CreditCard = "4355084355084358",
                    Cvv = "000",
                    ExpireMonth = "12",
                    ExpireYear = "2026"
                }
            };

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var cardNumberSanitized = (ScalarValue)((LogEventProperty[])((StructureValue)((LogEventProperty[])((StructureValue)events[0].Properties["p"]).Properties)[3].Value).Properties)[0].Value;

            Assert.Equal("#### #### #### ####", cardNumberSanitized.Value.ToString());
        }

        [Fact]
        public void SanitizeOnlyPrimitiveIsTrue_ShouldBe_SkipCardJsonType_SanitizeCreditCardPrimitiveProperty_When_SanitizeViaProperty()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("fullName", x =>
                                {
                                    var items = x.Split(' ');

                                    return $"{items[0].Substring(0, 2)}**** {items[1].Substring(0, 2)}****";
                                }, ignoreCase: true)
                                .Sanitize("creditCard", "#### #### #### ####", ignoreCase: true, onlyPrimitive: true)
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
            {
                OrderNumber = Guid.NewGuid(),
                FullName = "Sibel Çağlar",
                CreatedAt = DateTime.Now,
                CreditCard = new
                {
                    CreditCard = "4355084355084358",
                    Cvv = "000",
                    ExpireMonth = "12",
                    ExpireYear = "2026"
                }
            }));

            logger.Information(
                    "Sensitive Information {@p}",
                    model
            );

            var cardNumberSanitized = (ScalarValue)((LogEventProperty[])((StructureValue)((LogEventProperty[])((StructureValue)events[0].Properties["p"]).Properties)[3].Value).Properties)[0].Value;

            Assert.Equal("#### #### #### ####", cardNumberSanitized.Value.ToString());
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

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeArrayItems_When_Sanitize()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("Name", x => x.Substring(0, 2) + "****")
                                .Sanitize("Surname", x => x.Substring(0, 2) + "****")
                                .Sanitize("Phone", x => x.Substring(0, 2) + "****")
                                .Sanitize("Email", x => x.Substring(0, 2) + "****")
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new Department
            {
                DepartmentName = "IT",
                Persons = new Person[]
                {
                    new Person{ Email = "abc@email.com", Name = "Person-1-Name",  Surname = "Person-1-Surname", Phone = "111111111", Code = "ABC123" },
                    new Person{ Email = "def@email.com", Name = "Person-2-Name",  Surname = "Person-2-Surname", Phone = "222222222", Code = "DEF456" },
                    new Person{ Email = "hij@email.com", Name = "Person-3-Name",  Surname = "Person-3-Surname", Phone = "333333333", Code = "HIJ789" },
                }
            };

            logger.Information(
                    "Sensitive Information {@department}",
                    model
            );

            var modelProperties = ((StructureValue)events[0].Properties["department"]).Properties;

            string departmentNameExpected = ((ScalarValue)modelProperties[0].Value).Value.ToString();

            Assert.Equal(departmentNameExpected, model.DepartmentName);
            Assert.All(((SequenceValue)modelProperties[1].Value).Elements, (item) =>
            {
                var obj = ((StructureValue)item);

                var name = ((ScalarValue)obj.Properties[0].Value).Value.ToString();
                var surname = ((ScalarValue)obj.Properties[1].Value).Value.ToString();
                var phone = ((ScalarValue)obj.Properties[2].Value).Value.ToString();
                var email = ((ScalarValue)obj.Properties[3].Value).Value.ToString();
                var code = ((ScalarValue)obj.Properties[4].Value).Value.ToString();

                var p = model.Persons.FirstOrDefault(x => x.Code == code);

                Assert.Equal(email, p.Email.Substring(0, 2) + "****");
                Assert.Equal(name, p.Name.Substring(0, 2) + "****");
                Assert.Equal(surname, p.Surname.Substring(0, 2) + "****");
                Assert.Equal(phone, p.Phone.Substring(0, 2) + "****");
            });
        }

        [Fact]
        public void SanitizeViaExpression_ShouldBe_SanitizeNestedArrayItems_When_Sanitize()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("Street", x => x.Substring(0, 5) + "****")
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new Department
            {
                DepartmentName = "IT",
                Persons = new Person[]
                {
                    new Person
                    {
                        Email = "abc@email.com",
                        Name = "Person-1-Name",
                        Surname = "Person-1-Surname",
                        Phone = "111111111",
                        Code = "ABC123",
                        Addresses = new Address[]
                        {
                            new Address{ Description = "Home", City = "İstanbul", Street = "Fatih cad." },
                            new Address{ Description = "Work", City = "İstanbul", Street = "Çiftehavuzlar mh." }
                        }
                    }
                }
            };

            logger.Information(
                    "Sensitive Information {@department}",
                    model
            );

            var department = (StructureValue)events[0].Properties["department"];
            var personArray = (SequenceValue)department.Properties[1].Value;

            var person = (StructureValue)personArray.Elements[0];

            var addressArray = (SequenceValue)person.Properties[5].Value;

            Assert.All(addressArray.Elements, (address) =>
            {
                var item = (StructureValue)address;

                var description = ((ScalarValue)item.Properties[0].Value).Value.ToString();
                var city = ((ScalarValue)item.Properties[1].Value).Value.ToString();
                var street = ((ScalarValue)item.Properties[2].Value).Value.ToString();

                var a = model.Persons[0].Addresses.FirstOrDefault(x => x.Description == description);

                Assert.Equal(description, a.Description);
                Assert.Equal(city, a.City);
                Assert.Equal(street, a.Street.Substring(0, 5) + "****");
            });
        }

        [Fact]
        public void Sanitize_ShouldBe_Sanitize_When_SystemTextJsonDocument()
        {
            List<LogEvent> events = new List<LogEvent>();

            var logger = new LoggerConfiguration()
                            .Sanitizer()
                                .Sanitize("Email", x => x.Substring(0, 3).PadRight(10, '*'))
                            .Build()
                            .WriteTo.Sink(new SerilogStubSink(events))
                            .CreateLogger();

            var model = new Department
            {
                DepartmentName = "IT",
                Persons = new Person[]
                {
                    new Person
                    {
                        Email = "abc@email.com",
                        Name = "Person-1-Name",
                        Surname = "Person-1-Surname",
                        Phone = "111111111",
                        Code = "ABC123",
                        Addresses = new Address[]
                        {
                            new Address{ Description = "Home", City = "İstanbul", Street = "Fatih cad." },
                            new Address{ Description = "Work", City = "İstanbul", Street = "Çiftehavuzlar mh." }
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(model));

            logger.Information(
                    "Sensitive Information {@department}",
                    json
            );

            var department = (StructureValue)events[0].Properties["department"];
            var personArray = (SequenceValue)department.Properties[1].Value;

            var person = (StructureValue)personArray.Elements[0];
            var emailProperty = person.Properties.FirstOrDefault(x => x.Name == "Email");
            string sanitizedVal = ((ScalarValue)emailProperty.Value).Value.ToString();

            Assert.Equal(model.Persons[0].Email.Substring(0, 3).PadRight(10, '*'), sanitizedVal);
        }
    }

    class Department
    {
        public string DepartmentName { get; set; }

        public Person[] Persons { get; set; }
    }

    class Person
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public Address[] Addresses { get; set; }
    }

    class Address
    {
        public string Description { get; set; }

        public string City { get; set; }

        public string Street { get; set; }
    }

    class Company
    {
        public string Phone { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
