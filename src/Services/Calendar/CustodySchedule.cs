namespace CoParenting.Services.Calendar;

public class CustodySchedule
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }

    // Instante em que o primeiro bloco de Segments começa. A hora do dia de cada troca seguinte
    // resulta apenas da soma das durações dos blocos anteriores — não há um campo de "hora de troca"
    // separado, para suportar qualquer padrão (semana alternada, 2-2-3, fim de semana alternado, etc.).
    public DateTime AnchorStartUtc { get; set; }

    public string PaiColor { get; set; } = "#3b82f6";
    public string MaeColor { get; set; } = "#ec4899";

    public List<CustodySegment> Segments { get; set; } = [];
}
