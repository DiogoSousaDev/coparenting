using System.Text.RegularExpressions;
using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoParenting.Services.Calendar;

public class CustodyService(
    ICalendarDbContext dbContext,
    IFamilyAccessService familyAccessService,
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions,
    ILogger<CustodyService> logger) : ICustodyService
{
    private static readonly Regex HexColorRegex = new(@"^#[0-9A-Fa-f]{6}$");

    public async Task<CustodyScheduleDto?> GetScheduleAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(userId, familyId, cancellationToken))
        {
            return null;
        }

        var schedule = await dbContext.CustodySchedules.FirstOrDefaultAsync(s => s.FamilyId == familyId, cancellationToken);
        return schedule is null ? null : ToDto(schedule);
    }

    public async Task<UpsertCustodyScheduleResult> UpsertScheduleAsync(
        Guid userId,
        Guid familyId,
        DateTime anchorStartUtc,
        string paiColor,
        string maeColor,
        IReadOnlyList<CustodySegmentDto> segments,
        CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(userId, familyId, cancellationToken))
        {
            return UpsertCustodyScheduleResult.Failure("Não tens acesso a esta família.");
        }

        var validationError = ValidateInput(paiColor, maeColor, segments);
        if (validationError is not null)
        {
            return UpsertCustodyScheduleResult.Failure(validationError);
        }

        var schedule = await dbContext.CustodySchedules.FirstOrDefaultAsync(s => s.FamilyId == familyId, cancellationToken);
        if (schedule is null)
        {
            schedule = new CustodySchedule { Id = Guid.NewGuid(), FamilyId = familyId };
            dbContext.CustodySchedules.Add(schedule);
        }

        schedule.AnchorStartUtc = anchorStartUtc;
        schedule.PaiColor = paiColor;
        schedule.MaeColor = maeColor;

        // Substituir a lista toda (clear + re-add) em vez de reatribuir a propriedade —
        // garante que o EF deteta corretamente os blocos removidos da guarda anterior.
        schedule.Segments.Clear();
        for (var i = 0; i < segments.Count; i++)
        {
            schedule.Segments.Add(new CustodySegment { OrderIndex = i, Role = segments[i].Role, DurationHours = segments[i].DurationHours });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await NotifyOtherMembersAsync(familyId, userId, cancellationToken);

        return UpsertCustodyScheduleResult.Success();
    }

    public async Task<IReadOnlyList<CustodyPeriodDto>> GetPeriodsAsync(
        Guid userId,
        Guid familyId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(userId, familyId, cancellationToken))
        {
            return [];
        }

        var schedule = await dbContext.CustodySchedules.FirstOrDefaultAsync(s => s.FamilyId == familyId, cancellationToken);
        if (schedule is null)
        {
            return [];
        }

        var periods = CustodyScheduleExpander.GetPeriods(schedule, rangeStart, rangeEnd);
        return [.. periods.Select(p => new CustodyPeriodDto(p.Start, p.End, p.Role, p.Role == CustodyRole.Pai ? schedule.PaiColor : schedule.MaeColor))];
    }

    private static CustodyScheduleDto ToDto(CustodySchedule schedule) =>
        new(
            schedule.AnchorStartUtc,
            schedule.PaiColor,
            schedule.MaeColor,
            [.. schedule.Segments.OrderBy(s => s.OrderIndex).Select(s => new CustodySegmentDto(s.Role, s.DurationHours))]);

    private static string? ValidateInput(string paiColor, string maeColor, IReadOnlyList<CustodySegmentDto> segments)
    {
        if (segments.Count == 0)
        {
            return "É preciso pelo menos um bloco de guarda.";
        }

        if (segments.Any(s => s.DurationHours <= 0))
        {
            return "A duração de cada bloco tem de ser maior que zero.";
        }

        if (!HexColorRegex.IsMatch(paiColor) || !HexColorRegex.IsMatch(maeColor))
        {
            return "As cores têm de estar no formato #RRGGBB.";
        }

        return null;
    }

    private async Task NotifyOtherMembersAsync(Guid familyId, Guid actingUserId, CancellationToken cancellationToken)
    {
        var memberIds = await familyAccessService.GetMemberUserIdsAsync(familyId, cancellationToken);

        foreach (var memberId in memberIds.Where(id => id != actingUserId))
        {
            var email = await identityService.FindEmailByUserIdAsync(memberId);
            if (email is null)
            {
                continue;
            }

            try
            {
                var calendarLink = $"{appOptions.Value.ClientBaseUrl}/calendar";
                await emailSender.SendAsync(
                    email,
                    "Guarda partilhada atualizada — CoParenting",
                    $"O outro progenitor configurou/alterou a guarda partilhada. <a href=\"{calendarLink}\">Ver calendário</a>.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Mesma política do CalendarService: falha no envio não bloqueia a operação.
                logger.LogWarning(ex, "Falha ao enviar notificação de guarda partilhada para {Email}", email);
            }
        }
    }
}
