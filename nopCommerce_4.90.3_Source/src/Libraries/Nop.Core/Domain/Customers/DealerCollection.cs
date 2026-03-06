namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer collection document
/// </summary>
public partial class DealerCollection : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the related customer identifier
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the related dealer transaction identifier
    /// </summary>
    public int? DealerTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the cancellation dealer transaction identifier
    /// </summary>
    public int? CancelledDealerTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the collection method identifier
    /// </summary>
    public int CollectionMethodId { get; set; }

    /// <summary>
    /// Gets or sets the collection status identifier
    /// </summary>
    public int CollectionStatusId { get; set; }

    /// <summary>
    /// Gets or sets the collection amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the collection date and time in UTC
    /// </summary>
    public DateTime CollectionDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional reference number
    /// </summary>
    public string ReferenceNo { get; set; }

    /// <summary>
    /// Gets or sets the optional note
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the creator customer identifier
    /// </summary>
    public int CreatedByCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated date and time in UTC
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the cancellation customer identifier
    /// </summary>
    public int? CancelledByCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the cancelled date and time in UTC
    /// </summary>
    public DateTime? CancelledOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the collection method
    /// </summary>
    public DealerCollectionMethod CollectionMethod
    {
        get => (DealerCollectionMethod)CollectionMethodId;
        set => CollectionMethodId = (int)value;
    }

    /// <summary>
    /// Gets or sets the collection status
    /// </summary>
    public DealerCollectionStatus CollectionStatus
    {
        get => (DealerCollectionStatus)CollectionStatusId;
        set => CollectionStatusId = (int)value;
    }
}
