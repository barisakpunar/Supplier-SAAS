namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer financial instrument
/// </summary>
public partial class DealerFinancialInstrument : BaseEntity
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
    /// Gets or sets the related customer identifier
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the instrument type identifier
    /// </summary>
    public int InstrumentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the instrument status identifier
    /// </summary>
    public int InstrumentStatusId { get; set; }

    /// <summary>
    /// Gets or sets the instrument amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the instrument number
    /// </summary>
    public string InstrumentNo { get; set; }

    /// <summary>
    /// Gets or sets the issue date in UTC
    /// </summary>
    public DateTime? IssueDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the due date in UTC
    /// </summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the bank name
    /// </summary>
    public string BankName { get; set; }

    /// <summary>
    /// Gets or sets the branch name
    /// </summary>
    public string BranchName { get; set; }

    /// <summary>
    /// Gets or sets the account number
    /// </summary>
    public string AccountNo { get; set; }

    /// <summary>
    /// Gets or sets the drawer name
    /// </summary>
    public string DrawerName { get; set; }

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
    /// Gets or sets the instrument type
    /// </summary>
    public DealerFinancialInstrumentType InstrumentType
    {
        get => (DealerFinancialInstrumentType)InstrumentTypeId;
        set => InstrumentTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets the instrument status
    /// </summary>
    public DealerFinancialInstrumentStatus InstrumentStatus
    {
        get => (DealerFinancialInstrumentStatus)InstrumentStatusId;
        set => InstrumentStatusId = (int)value;
    }
}
