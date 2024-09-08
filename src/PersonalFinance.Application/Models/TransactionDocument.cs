using MongoDB.Bson;

namespace PersonalFinance.Application.Models;

public class TransactionDocument
{
    public ObjectId Id { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
}
