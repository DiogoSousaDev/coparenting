using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CoParenting.Application.Families;
using CoParenting.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/families")]
public class FamiliesController(IFamilyService familyService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record FamilySummaryResponse(Guid FamilyId, string Name, FamilyRole Role);

    [HttpGet]
    public async Task<IActionResult> GetMyFamilies(CancellationToken cancellationToken)
    {
        var families = await familyService.GetMyFamiliesAsync(CurrentUserId, cancellationToken);
        return Ok(families.Select(f => new FamilySummaryResponse(f.FamilyId, f.Name, f.Role)));
    }

    public record CreateFamilyRequest([Required, MaxLength(200)] string Name, FamilyRole CreatorRole);

    public record CreateFamilyResponse(Guid FamilyId);

    [HttpPost]
    public async Task<IActionResult> CreateFamily(CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        var result = await familyService.CreateFamilyAsync(CurrentUserId, request.Name, request.CreatorRole, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Created(string.Empty, new CreateFamilyResponse(result.FamilyId!.Value));
    }

    public record CreateInviteRequest([Required, EmailAddress] string Email, FamilyRole Role);

    public record CreateInviteResponse(DateTime ExpiresAtUtc, string? DevToken, string? Warning);

    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> CreateInvite(Guid id, CreateInviteRequest request, [FromServices] IHostEnvironment env, CancellationToken cancellationToken)
    {
        var result = await familyService.CreateInviteAsync(CurrentUserId, id, request.Email, request.Role, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        // DevToken só é devolvido em desenvolvimento, para testar via .http sem ler os logs.
        var devToken = env.IsDevelopment() ? result.RawToken : null;
        return Ok(new CreateInviteResponse(result.ExpiresAtUtc!.Value, devToken, result.Warning));
    }
}
