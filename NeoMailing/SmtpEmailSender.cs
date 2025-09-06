using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NeoMailing;

/// <summary>
/// SMTP implementation of <see cref="IEmailSender"/> using MailKit.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly Func<SmtpClient> _clientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="SmtpEmailSender"/>.
    /// </summary>
    /// <param name="options">SMTP settings bound via configuration.</param>
    /// <param name="clientFactory">Optional factory for creating <see cref="SmtpClient"/> instances.</param>

    public SmtpEmailSender(IOptions<SmtpSettings> options, Func<SmtpClient>? clientFactory = null)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _clientFactory = clientFactory ?? (() => new SmtpClient());
    }

    /// <summary>Sends a plain text email message.</summary>
    public Task SendTextAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string textBody,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject, textBody: textBody, ct: ct);

    /// <summary>Sends an HTML email message.</summary>
    public Task SendHtmlAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject, htmlBody: htmlBody, ct: ct);

    /// <summary>Sends an email containing both text and HTML bodies.</summary>
    public Task SendAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody,
        string? textBody,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject, htmlBody: htmlBody, textBody: textBody, ct: ct);

    /// <summary>Sends an email with optional CC and BCC recipients.</summary>
    public Task SendAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody,
        string? textBody,
        IEnumerable<string>? ccs,
        IEnumerable<string>? bccs,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject,
            htmlBody: htmlBody, textBody: textBody,
            ccs: ccs, bccs: bccs, ct: ct);

    /// <summary>Sends an email with attachments.</summary>
    public Task SendWithAttachmentsAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody,
        string? textBody,
        IEnumerable<(string fileName, byte[] content, string mimeType)> attachments,
        IEnumerable<string>? ccs = null,
        IEnumerable<string>? bccs = null,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject,
            htmlBody: htmlBody, textBody: textBody,
            ccs: ccs, bccs: bccs, attachments: attachments, ct: ct);

    /// <summary>Sends an email with full control (CC, BCC, Reply-To, and attachments).</summary>
    public Task SendAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody,
        string? textBody,
        string replyTo,
        IEnumerable<string>? ccs = null,
        IEnumerable<string>? bccs = null,
        IEnumerable<(string fileName, byte[] content, string mimeType)>? attachments = null,
        CancellationToken ct = default)
        => SendMasterAsync(fromName, fromAddress, toAddresses, subject,
            htmlBody: htmlBody, textBody: textBody,
            ccs: ccs, bccs: bccs, attachments: attachments, replyTo: replyTo, ct: ct);

    /// <summary>
    /// Core method that builds and sends an email message using MailKit.
    /// </summary>
    public async Task SendMasterAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody = null,
        string? textBody = null,
        IEnumerable<string>? ccs = null,
        IEnumerable<string>? bccs = null,
        string? replyTo = null,
        IEnumerable<(string fileName, byte[] content, string mimeType)>? attachments = null,
        CancellationToken ct = default)
    {

        if (toAddresses == null || !toAddresses.Any())
            throw new ArgumentException("At least one recipient must be specified.", nameof(toAddresses));

        if (string.IsNullOrWhiteSpace(textBody) && string.IsNullOrWhiteSpace(htmlBody))
            throw new ArgumentException("Either textBody or htmlBody must be provided.");

        var msg = new MimeMessage
        {
            Subject = subject
        };

        if (!string.IsNullOrWhiteSpace(replyTo))
            msg.ReplyTo.Add(MailboxAddress.Parse(replyTo));

        msg.From.Add(new MailboxAddress(fromName, fromAddress));
        AddAddresses(msg.To, toAddresses);

        if (ccs != null) AddAddresses(msg.Cc, ccs);
        if (bccs != null) AddAddresses(msg.Bcc, bccs);

        var bodyBuilder = new BodyBuilder();

        // Always provide TextBody if possible
        if (!string.IsNullOrWhiteSpace(textBody))
        {
            bodyBuilder.TextBody = textBody;
        }
        else if (!string.IsNullOrWhiteSpace(htmlBody))
        {
            // fallback: derive plain text from HTML so text-only clients still work
            bodyBuilder.TextBody = HtmlToPlainText(htmlBody);
        }

        // Optionally provide HtmlBody
        if (!string.IsNullOrWhiteSpace(htmlBody))
        {
            bodyBuilder.HtmlBody = htmlBody;
        }

        // Attachments (unchanged)
        if (attachments != null)
        {
            foreach (var (fileName, content, mimeType) in attachments)
            {
                if (content == null || content.Length == 0) continue;
                using var stream = new MemoryStream(content, writable: false);
                bodyBuilder.Attachments.Add(fileName, stream, ContentType.Parse(mimeType));
            }
        }

        msg.Body = bodyBuilder.ToMessageBody();

        using var client = _clientFactory();

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
            ct);

        // Some servers allow no-auth; only authenticate if provided.
        switch (_settings.AuthMode)
        {
            case SmtpAuthMode.Basic:
                if (!string.IsNullOrWhiteSpace(_settings.Username))
                    await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
                break;

            case SmtpAuthMode.OAuth2:
                if (!string.IsNullOrWhiteSpace(_settings.Username) &&
                    !string.IsNullOrWhiteSpace(_settings.AccessToken))
                {
                    // MailKit supports XOAUTH2 directly
                    var oauth2 = new SaslMechanismOAuth2(_settings.Username, _settings.AccessToken);
                    await client.AuthenticateAsync(oauth2, ct);
                }
                break;

            case SmtpAuthMode.None:
                // Do nothing
                break;
        }

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }

    /// <summary>
    /// Adds one or more email addresses into a <see cref="InternetAddressList"/>.
    /// </summary>
    private static void AddAddresses(InternetAddressList list, IEnumerable<string> addresses)
    {
        foreach (var a in addresses)
        {
            if (string.IsNullOrWhiteSpace(a)) continue;
            list.Add(MailboxAddress.Parse(a));
        }
    }

    /// <summary>
    /// Converts HTML content into plain text by stripping tags,
    /// decoding HTML entities, and normalizing whitespace.
    /// </summary>
    /// <param name="html">The HTML string to convert. May be <c>null</c> or empty.</param>
    /// <returns>A plain text representation of the HTML string.</returns>
    private static string HtmlToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove script and style sections
        html = Regex.Replace(html, "<(script|style)[^>]*?>.*?</\\1>", string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Replace <br>, <p>, <div>, <li> with line breaks
        html = Regex.Replace(html, @"<(br|p|div|li)[^>]*>", "\n", RegexOptions.IgnoreCase);

        // Replace blockquotes with indentation
        html = Regex.Replace(html, @"<blockquote[^>]*>", "\n> ", RegexOptions.IgnoreCase);

        // Remove all other tags
        html = Regex.Replace(html, "<.*?>", string.Empty);

        // Decode HTML entities (&amp;, &lt;, &nbsp;, etc.)
        html = HttpUtility.HtmlDecode(html);

        // Normalize whitespace (collapse multiple spaces/newlines)
        var lines = html.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));

        return string.Join(Environment.NewLine, lines);
    }
}
