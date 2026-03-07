namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents an allocation between a credit transaction and a debit transaction
/// </summary>
public partial class DealerTransactionAllocation : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the related collection identifier
    /// </summary>
    public int? DealerCollectionId { get; set; }

    /// <summary>
    /// Gets or sets the credit transaction identifier
    /// </summary>
    public int CreditDealerTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the debit transaction identifier
    /// </summary>
    public int DebitDealerTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the allocated amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the creator customer identifier
    /// </summary>
    public int CreatedByCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the cancellation customer identifier
    /// </summary>
    public int? CancelledByCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the cancelled date and time in UTC
    /// </summary>
    public DateTime? CancelledOnUtc { get; set; }
}
