namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer finance audit action type
/// </summary>
public enum DealerFinanceAuditActionType
{
    /// <summary>
    /// Collection created
    /// </summary>
    CollectionCreated = 10,

    /// <summary>
    /// Collection cancelled
    /// </summary>
    CollectionCancelled = 20,

    /// <summary>
    /// Financial instrument created
    /// </summary>
    FinancialInstrumentCreated = 30,

    /// <summary>
    /// Financial instrument status changed
    /// </summary>
    FinancialInstrumentStatusChanged = 40
}
