# MGA.Mailing

`MGA.Mailing` is a lightweight .NET library for **sending emails via SMTP** with optional **HTML templating** powered by [Scriban](https://github.com/scriban/scriban).

It provides a clean abstraction (`IEmailSender`) and a flexible SMTP implementation (`SmtpEmailSender`) that supports:

- Plain text, HTML, or **both** (multipart/alternative).
- **CC, BCC, Reply-To**, and **attachments**.
- **Authentication modes**: Basic (username/password), OAuth2, or None.
- Integration with **ASP.NET Core DI** or **manual instantiation**.
- Fast and reusable **HTML templating** with Scriban and caching.

---

## ✨ Features

- 📧 Send **plain text, HTML, or both** in the same message.
- 👥 Multiple recipients, CCs, BCCs, and Reply-To support.
- 📎 File attachments with proper MIME type handling.
- 🔑 Authentication: Basic, OAuth2 (e.g. Gmail, Office365), or no-auth for local dev servers.
- 🧩 Integrates easily with `IServiceCollection` or can be instantiated manually.
- 🎨 HTML templating via Scriban with **caching** and **case-insensitive rendering**.

---

## 🚦 When to Use

✅ **Best suited for**:  
- Applications that need to send notifications, onboarding emails, password resets, or transactional reports.  
- Systems where **SMTP configuration** varies by environment (dev/staging/prod).  

⚠️ **Not suited for**:  
- High-volume mailing (consider Mailgun, SendGrid, Amazon SES for bulk).  
- Tracking analytics, bounce handling, or campaign management.  

---

## 📦 Installation

From NuGet:

```sh
dotnet add package MGA.Mailing
```

Or reference locally in your solution:

```xml
<ProjectReference Include="..\MGA.Mailing\MGA.Mailing.csproj" />
```

---

## 🚀 Usage

### Manual Instantiation

```csharp
using MGA.Mailing;

var settings = new SmtpSettings
{
    Host = "smtp.gmail.com",
    Port = 587,
    AuthMode = SmtpAuthMode.Basic,
    UseStartTls = true,
    Username = "myusername@gmail.com",
    Password = "mypassword"
};

var emailSender = new SmtpEmailSender(
    Microsoft.Extensions.Options.Options.Create(settings)
);

// Send simple HTML email
await emailSender.SendHtmlAsync(
    fromName: "NeoClinic",
    fromAddress: "noreply@neoclinic.com",
    toAddresses: new[] { "user@example.com" },
    subject: "Welcome!",
    htmlBody: "<h1>Hello</h1><p>Thanks for joining NeoClinic!</p>"
);
```

---

### With Dependency Injection (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddMailing(builder.Configuration, "Smtp");

// appsettings.json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseStartTls": true,
  "AuthMode": "Basic",
  "Username": "myusername@gmail.com",
  "Password": "mypassword"
}
```

Injected usage:

```csharp
public class MailService
{
    private readonly IEmailSender _email;

    public MailService(IEmailSender email) => _email = email;

    public async Task SendOtpAsync(string toEmail, string otp, CancellationToken ct)
    {
        await _email.SendTextAsync(
            "NeoClinic",
            "noreply@neoclinic.com",
            new[] { toEmail },
            "Your OTP Code",
            $"Your one-time code is: {otp}",
            ct
        );
    }
}
```

---

### With Attachments

```csharp
await emailSender.SendWithAttachmentsAsync(
    "NeoClinic Reports",
    "reports@neoclinic.com",
    new[] { "user@example.com" },
    "Monthly Report",
    htmlBody: "<p>See attached report.</p>",
    textBody: "See attached report.",
    attachments: new[]
    {
        ("report.pdf", File.ReadAllBytes("sample-files/report.pdf"), "application/pdf")
    }
);
```

---

## 🎨 Templating (Scriban)

`MGA.Mailing` includes `TemplateRenderer` to render Scriban templates with model binding and caching.

```csharp
using MGA.Mailing;

var template = "<p>Hello {{ name }}!</p><p>Your OTP: {{ otp }}</p>";
var model = new { name = "John", otp = "123456" };

var html = TemplateRenderer.Render(template, model);

// Then pass into email
await emailSender.SendHtmlAsync("NeoClinic", "noreply@neoclinic.com",
    new[] { "user@example.com" },
    "OTP Verification",
    html);
```

Case-insensitive rendering:

```csharp
TemplateRenderer.RenderCaseInsensitive("Hello {{ NAME }}", new { Name = "John" });
// -> "Hello John"
```

---

## 📝 Notes

- Works with **Gmail, Outlook/Office365, Yahoo**, and custom SMTP servers.  
- For Gmail/Office365 **OAuth2**, you must first obtain an access token via their respective OAuth flows.  
- For dev environments, you can use **local mail servers** like [MailHog](https://github.com/mailhog/MailHog) or [Papercut SMTP](https://github.com/ChangemakerStudios/Papercut-SMTP).  
- Emails are sent as **multipart/alternative** when both text and HTML bodies are provided, ensuring compatibility with all clients.  

---

## 📖 License

This project is licensed under the [MIT License](LICENSE).  
