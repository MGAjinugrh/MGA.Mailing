using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace NeoMailing;

/// <summary>
/// Abstraction for sending email messages through a transport mechanism (e.g. SMTP).
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends a plain text email message.</summary>
    public Task SendTextAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string textBody,
        CancellationToken ct = default);

    /// <summary>Sends an HTML email message.</summary>
    public Task SendHtmlAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string htmlBody,
        CancellationToken ct = default);

    /// <summary>Sends an email containing both text and HTML bodies.</summary>
    public Task SendAsync(
        string fromName,
        string fromAddress,
        IEnumerable<string> toAddresses,
        string subject,
        string? htmlBody,
        string? textBody,
        CancellationToken ct = default);

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
        CancellationToken ct = default);

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
        CancellationToken ct = default);

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
        CancellationToken ct = default);
}
