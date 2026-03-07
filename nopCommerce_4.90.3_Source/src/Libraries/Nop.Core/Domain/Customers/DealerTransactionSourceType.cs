namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer transaction source type
/// </summary>
public enum DealerTransactionSourceType
{
    /// <summary>
    /// No specific source
    /// </summary>
    None = 0,

    /// <summary>
    /// Order source
    /// </summary>
    Order = 10,

    /// <summary>
    /// Collection source
    /// </summary>
    Collection = 20,

    /// <summary>
    /// Manual adjustment source
    /// </summary>
    ManualAdjustment = 30
}
