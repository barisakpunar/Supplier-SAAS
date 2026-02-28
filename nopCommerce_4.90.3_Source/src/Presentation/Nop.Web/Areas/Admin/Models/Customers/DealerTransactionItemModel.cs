using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer transaction item model
/// </summary>
public partial record DealerTransactionItemModel : BaseNopModel
{
    /// <summary>
    /// Gets or sets the related order identifier
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the related customer identifier
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the transaction type
    /// </summary>
    public string TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the transaction direction
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction note
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
