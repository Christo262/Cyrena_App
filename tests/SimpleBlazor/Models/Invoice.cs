using System;

namespace SimpleBlazor.Models;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
