using CoParenting.Services.Calendar;

namespace CoParenting.UnitTests.Calendar;

public class CustodyScheduleExpanderTests
{
    private static CustodySchedule AlternatingWeekSchedule(DateTime anchor) => new()
    {
        AnchorStartUtc = anchor,
        Segments =
        [
            new CustodySegment { OrderIndex = 0, Role = CustodyRole.Pai, DurationHours = 168 },
            new CustodySegment { OrderIndex = 1, Role = CustodyRole.Mae, DurationHours = 168 },
        ],
    };

    [Fact]
    public void GetPeriods_AlternatesWeekly_WhenRangeStartsAtAnchor()
    {
        var anchor = new DateTime(2026, 9, 25, 17, 0, 0, DateTimeKind.Utc); // sexta-feira
        var schedule = AlternatingWeekSchedule(anchor);

        var periods = CustodyScheduleExpander.GetPeriods(schedule, anchor, anchor.AddDays(21));

        Assert.Equal(3, periods.Count);
        Assert.Equal(CustodyRole.Pai, periods[0].Role);
        Assert.Equal(anchor, periods[0].Start);
        Assert.Equal(anchor.AddDays(7), periods[0].End);
        Assert.Equal(CustodyRole.Mae, periods[1].Role);
        Assert.Equal(anchor.AddDays(7), periods[1].Start);
        Assert.Equal(anchor.AddDays(14), periods[1].End);
        Assert.Equal(CustodyRole.Pai, periods[2].Role);
    }

    [Fact]
    public void GetPeriods_HandlesRangeBeforeAnchor_UsingNegativeModulo()
    {
        var anchor = new DateTime(2026, 9, 25, 17, 0, 0, DateTimeKind.Utc);
        var schedule = AlternatingWeekSchedule(anchor);

        // Uma semana antes da âncora deveria pertencer ao bloco anterior (Mãe), que é o
        // "espelho" do bloco que vem a seguir à âncora, já que o ciclo tem 2 blocos iguais.
        var periods = CustodyScheduleExpander.GetPeriods(schedule, anchor.AddDays(-7), anchor);

        Assert.Single(periods);
        Assert.Equal(CustodyRole.Mae, periods[0].Role);
        Assert.Equal(anchor.AddDays(-7), periods[0].Start);
        Assert.Equal(anchor, periods[0].End);
    }

    [Fact]
    public void GetPeriods_ReturnsUnequalDurations_ForCustomRotation()
    {
        // Padrão tipo "2-2-3": Pai 2 dias, Mãe 2 dias, Pai 3 dias, repete a cada 7 dias.
        var anchor = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var schedule = new CustodySchedule
        {
            AnchorStartUtc = anchor,
            Segments =
            [
                new CustodySegment { OrderIndex = 0, Role = CustodyRole.Pai, DurationHours = 48 },
                new CustodySegment { OrderIndex = 1, Role = CustodyRole.Mae, DurationHours = 48 },
                new CustodySegment { OrderIndex = 2, Role = CustodyRole.Pai, DurationHours = 72 },
            ],
        };

        var periods = CustodyScheduleExpander.GetPeriods(schedule, anchor, anchor.AddDays(7));

        Assert.Equal(3, periods.Count);
        Assert.Equal((CustodyRole.Pai, TimeSpan.FromHours(48)), (periods[0].Role, periods[0].End - periods[0].Start));
        Assert.Equal((CustodyRole.Mae, TimeSpan.FromHours(48)), (periods[1].Role, periods[1].End - periods[1].Start));
        Assert.Equal((CustodyRole.Pai, TimeSpan.FromHours(72)), (periods[2].Role, periods[2].End - periods[2].Start));
    }

    [Fact]
    public void GetPeriods_SpansMultipleCycleRepetitions()
    {
        var anchor = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc); // sexta-feira
        var schedule = AlternatingWeekSchedule(anchor);

        var periods = CustodyScheduleExpander.GetPeriods(schedule, anchor, anchor.AddDays(70));

        Assert.Equal(10, periods.Count);
        for (var i = 0; i < periods.Count; i++)
        {
            Assert.Equal(i % 2 == 0 ? CustodyRole.Pai : CustodyRole.Mae, periods[i].Role);
        }
    }

    [Fact]
    public void GetPeriods_ShiftsHandoffTimeOfDay_WhenDurationIsNotMultipleOf24Hours()
    {
        // Um bloco de 45h (não múltiplo de 24h) desloca a hora da troca seguinte.
        var anchor = new DateTime(2026, 1, 1, 17, 0, 0, DateTimeKind.Utc);
        var schedule = new CustodySchedule
        {
            AnchorStartUtc = anchor,
            Segments =
            [
                new CustodySegment { OrderIndex = 0, Role = CustodyRole.Pai, DurationHours = 45 },
                new CustodySegment { OrderIndex = 1, Role = CustodyRole.Mae, DurationHours = 45 },
            ],
        };

        var periods = CustodyScheduleExpander.GetPeriods(schedule, anchor, anchor.AddHours(90));

        Assert.Equal(2, periods.Count);
        Assert.Equal(new DateTime(2026, 1, 3, 14, 0, 0, DateTimeKind.Utc), periods[0].End); // 17:00 + 45h = dia 3, 14:00
        Assert.Equal(periods[0].End, periods[1].Start);
    }

    [Fact]
    public void GetPeriods_ReturnsEmpty_WhenNoSegments()
    {
        var schedule = new CustodySchedule { AnchorStartUtc = DateTime.UtcNow, Segments = [] };

        var periods = CustodyScheduleExpander.GetPeriods(schedule, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        Assert.Empty(periods);
    }
}
