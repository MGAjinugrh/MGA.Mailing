using Example.DI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Example.DI.Controllers;

[ApiController, Route("api/v1/mail")]
public class MailController : ControllerBase
{
    private readonly MailService _mail;

    public MailController(MailService mail)
    {
        _mail = mail;
    }

    [HttpGet("send")]
    public async Task<IActionResult> Send([FromQuery] string to)
    {
        try
        {
            await _mail.SendInvitationAsync(to);
            return Ok($"Mail sent to {to}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
