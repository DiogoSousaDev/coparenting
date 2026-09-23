namespace CoParenting.Services.Calendar;

// Mesma numeração implícita de Families.FamilyRole (Pai=0, Mae=1), duplicado propositadamente
// para o Calendar não depender de tipos do Service Families (ver CalendarEvent para o mesmo padrão).
public enum CustodyRole
{
    Pai,
    Mae
}
