namespace CoParenting.Services.Calendar;

// Bloco de um ciclo de guarda: começa quando o bloco anterior termina, dura DurationHours,
// e é atribuído ao progenitor Role. A lista de blocos repete-se indefinidamente a partir de
// CustodySchedule.AnchorStartUtc (ver CustodyScheduleExpander).
public class CustodySegment
{
    public int OrderIndex { get; set; }
    public CustodyRole Role { get; set; }
    public int DurationHours { get; set; }
}
