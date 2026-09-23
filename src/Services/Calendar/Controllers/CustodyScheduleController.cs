using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.Services.Calendar.Controllers;

[ApiController]
[Authorize]
public class CustodyScheduleController(ICustodyService custodyService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record CustodySegmentRequest(CustodyRole Role, [Range(1, int.MaxValue)] int DurationHours);

    public record UpsertCustodyScheduleRequest(
        DateTime AnchorStart,
        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")] string PaiColor,
        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")] string MaeColor,
        [Required, MinLength(1)] List<CustodySegmentRequest> Segments);

    public record CustodySegmentResponse(CustodyRole Role, int DurationHours);

    public record CustodyScheduleResponse(DateTime AnchorStart, string PaiColor, string MaeColor, IReadOnlyList<CustodySegmentResponse> Segments);

    public record CustodyPeriodResponse(DateTime Start, DateTime End, CustodyRole Role, string Color);

    [HttpGet("api/families/{familyId:guid}/custody-schedule")]
    public async Task<IActionResult> GetSchedule(Guid familyId, CancellationToken cancellationToken)
    {
        var schedule = await custodyService.GetScheduleAsync(CurrentUserId, familyId, cancellationToken);
        if (schedule is null)
        {
            return NoContent();
        }

        return Ok(new CustodyScheduleResponse(
            schedule.AnchorStartUtc,
            schedule.PaiColor,
            schedule.MaeColor,
            [.. schedule.Segments.Select(s => new CustodySegmentResponse(s.Role, s.DurationHours))]));
    }

    [HttpPut("api/families/{familyId:guid}/custody-schedule")]
    public async Task<IActionResult> UpsertSchedule(Guid familyId, UpsertCustodyScheduleRequest request, CancellationToken cancellationToken)
    {
        var segments = request.Segments.Select(s => new CustodySegmentDto(s.Role, s.DurationHours)).ToList();

        var result = await custodyService.UpsertScheduleAsync(
            CurrentUserId, familyId, request.AnchorStart, request.PaiColor, request.MaeColor, segments, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    [HttpGet("api/families/{familyId:guid}/custody-periods")]
    public async Task<IActionResult> GetPeriods(Guid familyId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var periods = await custodyService.GetPeriodsAsync(CurrentUserId, familyId, from, to, cancellationToken);
        return Ok(periods.Select(p => new CustodyPeriodResponse(p.Start, p.End, p.Role, p.Color)));
    }
}
