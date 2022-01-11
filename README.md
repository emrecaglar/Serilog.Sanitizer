# Serilog Log Sanitize Library
## Serilog sanitize package is usable to mask or completely delete sensitive data.

---
### Multiple Sanitize And Ignore Properties:

```csharp
var logger = new LoggerConfiguration()
                    .Sanitizer()
                        .SanitizeViaRegex(new[] { "[Cc]vv", "[Cc]vc", "[Cc]vv" }, "***")
                        .SanitizeViaRegex(new[] { "[Cc]ard", "[Cc]ard[Nn]umber", "[Pp]an", "[Cc]ard[Nn]o" }, x => string.Concat(x.Substring(0, 6), "******", x.Substring(12, 4)))
                        .IgnoreProp(ignoreCase: true, "expmonth", "expyear", "expire", "expireYear", "expireMonth")
                        .Build()
                    .WriteTo.Debug()
                    .CreateLogger();

var model = new { CardNumber = "4355084355084358", Cvv = "000", ExpireMonth = "12", ExpireYear = "2026" };

logger.Information(
        "Sensitive Information {@p}",
        model
);
```
### Output:
```cmd
[13:36:28 INF] Sensitive Information {"CardNumber":"435508******4358","Cvv":"***","$type":"<>f__AnonymousType0`4"}
```
---
### Typed Sanitize:

```csharp
class Person
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }
}

var logger = new LoggerConfiguration()
                    .Sanitizer()
                        .Typed<Person>()
                            .Sanitize(x => x.Name, x => string.Concat(x.Substring(0, 2), "***"))
                            .Sanitize(x => x.Surname, x => string.Concat(x.Substring(0, 2), "***"))
                            .Sanitize(x => x.Email, x => string.Concat(x.Substring(0, 2), "***"))
                            .Sanitize(x => x.Phone, x => string.Concat(x.Substring(0, 2), "***"))
                            .Continue()
                        .Build()
                        .WriteTo.Debug()
                        .CreateLogger();

            var model = new Person { Name = "Bruce", Surname = "Lee", Email = "brucelee@gmai.com", Phone = "5076589898" };

logger.Information(
    "Sensitive Information {@p1} {@p2}",
    model,
    company
);
```

### Output:
```cmd
[13:53:52 INF] Sensitive Information {"Name":"Br***","Surname":"Le***","Phone":"50***","Email":"br***","$type":"Person"} {"Phone":"2226663355","Name":"Foo corp.","Email":"foo@bar.com","$type":"Company"}
```

---

### Sanitize Via Property Name

```csharp
var logger = new LoggerConfiguration()
                    .Sanitizer()
                        .Sanitize("Phone", phone => string.Concat(phone.Substring(0, 4), "****", phone.Substring(8, 2)))
                        .Build()
                    .WriteTo.Debug()
                    .CreateLogger();

var model = new { Name = "Bruce", Phone = "5076589898", WorkPhone = "2123256985" };

logger.Information(
        "Sensitive Information {@p}",
        model
);
```

### Output:

```cmd
[13:56:19 INF] Sensitive Information {"Name":"Bruce","Phone":"5076****98","WorkPhone":"2123256985","$type":"<>f__AnonymousType1`3"}
```
---
### Sanitize Via Regex

```csharp
var logger = new LoggerConfiguration()
                    .Sanitizer()
                        .SanitizeViaRegex("[Pp]hone", phone => string.Concat(phone.Substring(0, 4), "****", phone.Substring(8, 2)))
                        .Build()
                    .WriteTo.Debug()
                    .CreateLogger();

var model = new { Name = "Bruce", Phone = "5076589898", WorkPhone = "2123256985" };

logger.Information(
        "Sensitive Information {@p}",
        model
);
```

### Output:
```cmd
[13:59:18 INF] Sensitive Information {"Name":"Bruce","Phone":"5076****98","WorkPhone":"2123****85","$type":"<>f__AnonymousType1`3"}
```
---

### Sanitize Property With Constant Value

```csharp
const string SANITIZE_PASSWORD = "***";

var logger = new LoggerConfiguration()
                    .Sanitizer()
                        .SanitizeViaRegex("[Pp]assword", SANITIZE_PASSWORD)
                    .Build()
                    .WriteTo.Debug()
                    .CreateLogger();

var model = new { Name = "Bruce", Password = "123" };

logger.Information(
        "Sensitive Information {@p}",
        model
);
```

### Output:
```cmd
[14:01:22 INF] Sensitive Information {"Name":"Bruce","Password":"***","$type":"<>f__AnonymousType2`2"}
```