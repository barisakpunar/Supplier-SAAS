namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents dealer transaction type
/// </summary>
public enum DealerTransactionType
{
    /// <summary>
    /// Open account order debt posting
    /// </summary>
    OpenAccountOrder = 10,

    /// <summary>
    /// Open account collection posting
    /// </summary>
    OpenAccountCollection = 20,

    /// <summary>
    /// Manual debit adjustment
    /// </summary>
    ManualDebitAdjustment = 30,

    /// <summary>
    /// Manual credit adjustment
    /// </summary>
    ManualCreditAdjustment = 40
}
