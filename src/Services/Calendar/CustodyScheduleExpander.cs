namespace CoParenting.Services.Calendar;

public record CustodyPeriod(DateTime Start, DateTime End, CustodyRole Role);

// Expande a lista de blocos de um CustodySchedule (que se repete em ciclo a partir de
// AnchorStartUtc) nos períodos concretos que intersetam o intervalo pedido. Função pura,
// sem acesso a BD — ao estilo de RecurrenceExpander para os eventos do calendário.
public static class CustodyScheduleExpander
{
    public static IReadOnlyList<CustodyPeriod> GetPeriods(CustodySchedule schedule, DateTime rangeStart, DateTime rangeEnd)
    {
        if (rangeEnd <= rangeStart)
        {
            return [];
        }

        var segments = schedule.Segments.OrderBy(s => s.OrderIndex).ToList();
        if (segments.Count == 0)
        {
            return [];
        }

        var cycleHours = segments.Sum(s => (long)s.DurationHours);
        if (cycleHours <= 0)
        {
            return [];
        }

        // Offsets acumulados (em horas) de cada bloco desde o início do ciclo.
        var offsets = new long[segments.Count + 1];
        for (var i = 0; i < segments.Count; i++)
        {
            offsets[i + 1] = offsets[i] + segments[i].DurationHours;
        }

        var elapsedHours = (rangeStart - schedule.AnchorStartUtc).TotalHours;
        var cycleIndex = (long)Math.Floor(elapsedHours / cycleHours);
        var offsetInCycle = elapsedHours - (cycleIndex * cycleHours);

        var segmentIndex = segments.Count - 1;
        for (var i = 0; i < segments.Count; i++)
        {
            if (offsetInCycle < offsets[i + 1])
            {
                segmentIndex = i;
                break;
            }
        }

        var results = new List<CustodyPeriod>();
        var currentStart = schedule.AnchorStartUtc.AddHours((cycleIndex * cycleHours) + offsets[segmentIndex]);

        while (currentStart < rangeEnd)
        {
            var segment = segments[segmentIndex];
            var currentEnd = currentStart.AddHours(segment.DurationHours);

            results.Add(new CustodyPeriod(currentStart, currentEnd, segment.Role));

            segmentIndex++;
            if (segmentIndex == segments.Count)
            {
                segmentIndex = 0;
            }

            currentStart = currentEnd;
        }

        return results;
    }
}
