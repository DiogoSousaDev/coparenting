using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using IcsEvent = Ical.Net.CalendarComponents.CalendarEvent;

namespace CoParenting.Services.Calendar;

// Expande a RRULE (RFC 5545) de um evento recorrente em ocorrências concretas dentro
// de um intervalo — usa o Ical.Net em vez de reimplementar o cálculo de recorrência.
internal static class RecurrenceExpander
{
    public static bool IsValidRule(string recurrenceRule)
    {
        try
        {
            _ = new RecurrencePattern(recurrenceRule);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IEnumerable<(DateTime Start, DateTime End)> GetOccurrences(CalendarEvent calendarEvent, DateTime rangeStart, DateTime rangeEnd)
    {
        if (!calendarEvent.IsRecurring || string.IsNullOrWhiteSpace(calendarEvent.RecurrenceRule))
        {
            if (calendarEvent.StartDate < rangeEnd && calendarEvent.EndDate > rangeStart)
            {
                yield return (calendarEvent.StartDate, calendarEvent.EndDate);
            }

            yield break;
        }

        var icsEvent = new IcsEvent
        {
            Start = new CalDateTime(calendarEvent.StartDate),
            End = new CalDateTime(calendarEvent.EndDate),
            RecurrenceRule = new RecurrencePattern(calendarEvent.RecurrenceRule)
        };

        var occurrences = icsEvent.GetOccurrences(new CalDateTime(rangeStart), new EvaluationOptions())
            .TakeWhile(o => o.Period.StartTime.Value < rangeEnd);

        foreach (var occurrence in occurrences)
        {
            yield return (occurrence.Period.StartTime.Value, occurrence.Period.EffectiveEndTime!.Value);
        }
    }
}
