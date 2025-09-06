using Microsoft.Extensions.Options;
using MGA.Mailing;

const string FromName = "Test Sender";
const string FromAddress = "test.sender@myapp.com";
const string ToAddress1 = "test.recipient1@myapp.com";
const string ToAddress2 = "test.recipient2@myapp.com";
const string AppName = "MGA.Mailing Demo";
const string ToName = "User";
const string DashboardLink = "http://localhost";

// 1. Configure settings manually (normally from appsettings.json or env vars)

// Local Dev Mail Server (No Auth) (You can use Mailhog, Papercut or some other local SMTP server to test out)
//var settings = new SmtpSettings
//{
//    Host = "localhost",
//    Port = 25,
//    UseStartTls = false,
//    AuthMode = SmtpAuthMode.None
//};

// Gmail (App Password — Basic)
var settings = new SmtpSettings
{
    Host = "smtp.gmail.com", // 
    Port = 587, // Standard ports
    AuthMode = SmtpAuthMode.Basic,
    UseStartTls = true,
    Username = "myusername@gmail.com",
    Password = "mypassword"
};

// Gmail (OAuth2 Access Token)
//var settings = new SmtpSettings
//{
//    Host = "smtp.gmail.com",
//    Port = 587,
//    UseStartTls = true,
//    AuthMode = SmtpAuthMode.OAuth2,
//    Username = "me@gmail.com",
//    AccessToken = "ya29.A0AVA9y..." // From Google OAuth2 flow
//};

// Outlook / Office 365 (OAuth2)
//var settings = new SmtpSettings
//{
//    Host = "smtp.office365.com",
//    Port = 587,
//    UseStartTls = true,
//    AuthMode = SmtpAuthMode.OAuth2,
//    Username = "me@company.com",
//    AccessToken = "<msft access token>"
//};

// 2. Create sender manually
var emailSender = new SmtpEmailSender(Options.Create(settings));
// OR new SmtpEmailSender(settings) if your actor takes SmtpSettings directly

// 3. Send plain text only
await emailSender.SendTextAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1 },
    subject: "Plain Text Example",
    textBody: "Hello! This is a plain text message."
);

// 4. Send HTML only
var htmlPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "sample-templates", "Welcome.scriban.html");
var htmlTemplate = File.ReadAllText(htmlPath);
var html = TemplateRenderer.Render(htmlTemplate, new
{
    appName = AppName,
    name = ToName,
    dashboardLink = DashboardLink
});

await emailSender.SendHtmlAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1 },
    subject: "HTML Example",
    htmlBody: html
);

// 5. Send both text + HTML to multiple recipients
await emailSender.SendAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1, ToAddress2 },
    subject: "Multipart Example",
    textBody: $"Hi {ToName}. Thanks for joining {AppName}. We’re excited to have you on board! Go to Dashboard",
    htmlBody: html
);

// 6. Send with CC + BCC
await emailSender.SendAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1 },
    subject: "With CC + BCC",
    textBody: "Plain text with CC and BCC.",
    htmlBody: "<p>Plain text with CC and BCC.</p>",
    ccs: new[] { "manager@example.com" },
    bccs: new[] { "auditor@example.com" }
);

// 7. Send with Reply-To
await emailSender.SendAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1 },
    subject: "With Reply-To",
    textBody: "Please reply to another address.",
    htmlBody: "<p>Please reply to another address.</p>",
    replyTo: "support@example.com"
);

// 8. Send with attachments (load from sample-files)
var attachmentPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "sample-files", "report.pdf");

await emailSender.SendWithAttachmentsAsync(
    fromName: FromName,
    fromAddress: FromAddress,
    toAddresses: new[] { ToAddress1 },
    subject: "Report with Attachments",
    textBody: "Please see the attached report.",
    htmlBody: "<p>Please see the attached <b>report</b>.</p>",
    attachments: new[]
    {
        ("report.pdf", File.ReadAllBytes(attachmentPath), "application/pdf")
    }
);
