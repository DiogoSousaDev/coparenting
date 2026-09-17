namespace CoParenting.Domain.Entities;

// Imutável por requisito de negócio: nunca deve ter UPDATE ou DELETE no código de aplicação, só INSERT.
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string? AttachmentUrl { get; set; }

    public Family Family { get; set; } = null!;
}
