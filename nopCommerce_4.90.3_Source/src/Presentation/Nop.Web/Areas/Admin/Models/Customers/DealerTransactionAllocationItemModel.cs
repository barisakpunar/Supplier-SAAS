using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer transaction allocation item model
/// </summary>
public partial record DealerTransactionAllocationItemModel : BaseNopEntityModel
{
    public int DebitDealerTransactionId { get; set; }

    public string DebitSourceText { get; set; }

    public string DebitSourceUrl { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public bool IsCancelled { get; set; }

    public DateTime? CancelledOnUtc { get; set; }
}
