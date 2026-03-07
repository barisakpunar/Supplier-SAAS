namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents an immutable dealer finance audit log entry
/// </summary>
public partial class DealerFinanceAuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the main entity type identifier
    /// </summary>
    public int EntityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the related collection identifier
    /// </summary>
    public int? DealerCollectionId { get; set; }

    /// <summary>
    /// Gets or sets the related financial instrument identifier
    /// </summary>
    public int? DealerFinancialInstrumentId { get; set; }

    /// <summary>
    /// Gets or sets the action type identifier
    /// </summary>
    public int ActionTypeId { get; set; }

    /// <summary>
    /// Gets or sets the optional status before action
    /// </summary>
    public int? StatusBeforeId { get; set; }

    /// <summary>
    /// Gets or sets the optional status after action
    /// </summary>
    public int? StatusAfterId { get; set; }

    /// <summary>
    /// Gets or sets the optional note
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the operator customer identifier
    /// </summary>
    public int? PerformedByCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the performed date in UTC
    /// </summary>
    public DateTime PerformedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the entity type
    /// </summary>
    public DealerFinanceAuditEntityType EntityType
    {
        get => (DealerFinanceAuditEntityType)EntityTypeId;
        set => EntityTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets the action type
    /// </summary>
    public DealerFinanceAuditActionType ActionType
    {
        get => (DealerFinanceAuditActionType)ActionTypeId;
        set => ActionTypeId = (int)value;
    }
}
