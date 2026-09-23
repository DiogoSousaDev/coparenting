namespace CoParenting.Services.Calendar;

public interface ICustodyService
{
    Task<CustodyScheduleDto?> GetScheduleAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default);

    Task<UpsertCustodyScheduleResult> UpsertScheduleAsync(
        Guid userId,
        Guid familyId,
        DateTime anchorStartUtc,
        string paiColor,
        string maeColor,
        IReadOnlyList<CustodySegmentDto> segments,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustodyPeriodDto>> GetPeriodsAsync(
        Guid userId,
        Guid familyId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken = default);
}
