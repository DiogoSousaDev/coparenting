namespace CoParenting.Services.Calendar;

public record CustodySegmentDto(CustodyRole Role, int DurationHours);

public record CustodyScheduleDto(DateTime AnchorStartUtc, string PaiColor, string MaeColor, IReadOnlyList<CustodySegmentDto> Segments);

public record CustodyPeriodDto(DateTime Start, DateTime End, CustodyRole Role, string Color);

public record UpsertCustodyScheduleResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static UpsertCustodyScheduleResult Success() => new(true, []);
    public static UpsertCustodyScheduleResult Failure(params string[] errors) => new(false, errors);
}
