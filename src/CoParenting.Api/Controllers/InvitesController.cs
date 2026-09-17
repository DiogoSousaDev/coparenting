using System.Security.Claims;
using CoParenting.Application.Families;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.Api.Controllers;

[ApiController]
[Route("api/invites")]
public class InvitesController(IFamilyService familyService) : ControllerBase
{
    public record InviteDetailsResponse(string FamilyName, string InvitedEmail, bool Expired, bool AlreadyAccepted);

    [HttpGet("{token}")]
    public async Task<IActionResult> GetDetails(string token, CancellationToken cancellationToken)
    {
        var details = await familyService.GetInviteDetailsAsync(token, cancellationToken);
        if (!details.Found)
        {
            return NotFound();
        }

        return Ok(new InviteDetailsResponse(details.FamilyName!, details.InvitedEmail!, details.Expired, details.AlreadyAccepted));
    }

    public record AcceptInviteResponse(Guid FamilyId);

    [HttpPost("{token}/accept")]
    [Authorize]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userEmail = User.FindFirstValue(ClaimTypes.Email)!;

        var result = await familyService.AcceptInviteAsync(userId, userEmail, token, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Ok(new AcceptInviteResponse(result.FamilyId!.Value));
    }
}
