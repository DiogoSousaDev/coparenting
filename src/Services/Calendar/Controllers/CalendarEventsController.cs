using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.Services.Calendar.Controllers;

[ApiController]
[Authorize]
public class CalendarEventsController(ICalendarService calendarService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record EventOccurrenceResponse(Guid EventId, string Title, DateTime Start, DateTime End, bool IsRecurring, string? RecurrenceRule);

    [HttpGet("api/families/{familyId:guid}/calendar-events")]
    public async Task<IActionResult> GetEvents(Guid familyId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var occurrences = await calendarService.GetEventsAsync(CurrentUserId, familyId, from, to, cancellationToken);
        return Ok(occurrences.Select(o => new EventOccurrenceResponse(o.EventId, o.Title, o.Start, o.End, o.IsRecurring, o.RecurrenceRule)));
    }

    public record CreateEventRequest(
        [Required, MaxLength(200)] string Title,
        DateTime Start,
        DateTime End,
        string? RecurrenceRule);

    public record CreateEventResponse(Guid EventId);

    [HttpPost("api/families/{familyId:guid}/calendar-events")]
    public async Task<IActionResult> CreateEvent(Guid familyId, CreateEventRequest request, CancellationToken cancellationToken)
    {
        var result = await calendarService.CreateEventAsync(
            CurrentUserId, familyId, request.Title, request.Start, request.End, request.RecurrenceRule, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Created(string.Empty, new CreateEventResponse(result.EventId!.Value));
    }

    public record UpdateEventRequest(
        [Required, MaxLength(200)] string Title,
        DateTime Start,
        DateTime End,
        string? RecurrenceRule);

    [HttpPut("api/calendar-events/{eventId:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var result = await calendarService.UpdateEventAsync(
            CurrentUserId, eventId, request.Title, request.Start, request.End, request.RecurrenceRule, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    [HttpDelete("api/calendar-events/{eventId:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await calendarService.DeleteEventAsync(CurrentUserId, eventId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }
}
