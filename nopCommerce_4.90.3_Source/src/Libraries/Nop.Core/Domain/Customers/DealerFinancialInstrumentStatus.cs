namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer financial instrument status
/// </summary>
public enum DealerFinancialInstrumentStatus
{
    /// <summary>
    /// Instrument posted and active
    /// </summary>
    Posted = 10,

    /// <summary>
    /// Instrument collected
    /// </summary>
    Collected = 20,

    /// <summary>
    /// Instrument returned
    /// </summary>
    Returned = 30,

    /// <summary>
    /// Instrument protested
    /// </summary>
    Protested = 40,

    /// <summary>
    /// Instrument cancelled
    /// </summary>
    Cancelled = 50
}
