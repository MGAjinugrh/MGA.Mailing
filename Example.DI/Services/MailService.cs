using NeoMailing;

namespace Example.DI.Services;

public class MailService
{
    private readonly IEmailSender _email;

    public MailService(IEmailSender email)
    {
        _email = email;
    }

    public async Task SendInvitationAsync(string toEmail, CancellationToken ct = default)
    {
        try
        {
            await _email.SendAsync(
                fromName: "Test Sender",
                fromAddress: "test.sender@myapp.com",
                toAddresses: new[] { toEmail },
                subject: "Welcome from DI!",
                htmlBody: "<h1>Hello from DI</h1><p>This email came via dependency injection.</p>",
                textBody: "Hello from DI - This email came via dependency injection.",
                replyTo: "test.sender@myapp.com",
                ct: ct
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
