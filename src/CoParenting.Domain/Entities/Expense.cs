namespace CoParenting.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public Guid PaidByUserId { get; set; }
    public ExpenseStatus Status { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public Family Family { get; set; } = null!;
}
