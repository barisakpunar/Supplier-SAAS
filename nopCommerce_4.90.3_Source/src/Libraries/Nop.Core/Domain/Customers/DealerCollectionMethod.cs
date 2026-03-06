namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents dealer collection method
/// </summary>
public enum DealerCollectionMethod
{
    /// <summary>
    /// Cash collection
    /// </summary>
    Cash = 10,

    /// <summary>
    /// Bank transfer collection
    /// </summary>
    BankTransfer = 20,

    /// <summary>
    /// Check collection
    /// </summary>
    Check = 30,

    /// <summary>
    /// Promissory note collection
    /// </summary>
    PromissoryNote = 40,

    /// <summary>
    /// Credit card collection
    /// </summary>
    CreditCard = 50,

    /// <summary>
    /// Other collection
    /// </summary>
    Other = 60
}
