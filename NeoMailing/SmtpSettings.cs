namespace NeoMailing;

/// <summary>
/// Defines supported authentication modes for SMTP.
/// </summary>
public enum SmtpAuthMode
{
    /// <summary>Use username + password authentication.</summary>
    Basic,

    /// <summary>Use username + OAuth2 access token authentication.</summary>
    OAuth2,

    /// <summary>No authentication (only for local/dev SMTP servers).</summary>
    None
}

/// <summary>
/// Represents SMTP configuration settings.
/// </summary>
public sealed class SmtpSettings
{
    /// <summary>SMTP server hostname or IP address.</summary>
    public string Host { get; set; } = default!;

    /// <summary>Port number (default: 587).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Whether STARTTLS should be used to upgrade the connection.</summary>
    public bool UseStartTls { get; set; } = true;

    /// <summary>Authentication mode for the SMTP connection.</summary>
    public SmtpAuthMode AuthMode { get; set; } = SmtpAuthMode.Basic;

    /// <summary>Username for Basic authentication.</summary>
    public string Username { get; set; } = default!;

    /// <summary>Password for Basic authentication.</summary>
    public string Password { get; set; } = default!;

    /// <summary>Access token for OAuth2 authentication.</summary>
    public string? AccessToken { get; set; }
}