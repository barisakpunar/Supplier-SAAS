namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents dealer transaction entry
/// </summary>
public partial class DealerTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the related order identifier
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the related customer identifier
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the source type identifier
    /// </summary>
    public int SourceTypeId { get; set; }

    /// <summary>
    /// Gets or sets the source identifier
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference number
    /// </summary>
    public string ReferenceNo { get; set; }

    /// <summary>
    /// Gets or sets the transaction type identifier
    /// </summary>
    public int TransactionTypeId { get; set; }

    /// <summary>
    /// Gets or sets the transaction direction identifier
    /// </summary>
    public int DirectionId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets optional transaction note
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the source type
    /// </summary>
    public DealerTransactionSourceType SourceType
    {
        get => (DealerTransactionSourceType)SourceTypeId;
        set => SourceTypeId = (int)value;
    }
}
