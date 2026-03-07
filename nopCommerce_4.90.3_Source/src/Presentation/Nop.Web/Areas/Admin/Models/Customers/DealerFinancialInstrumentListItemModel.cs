using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer financial instrument list item model
/// </summary>
public partial record DealerFinancialInstrumentListItemModel : BaseNopEntityModel
{
    public int DealerId { get; set; }

    public string DealerName { get; set; }

    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string InstrumentType { get; set; }

    public string InstrumentStatus { get; set; }

    public decimal Amount { get; set; }

    public string InstrumentNo { get; set; }

    public DateTime? IssueDateUtc { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public int? DealerCollectionId { get; set; }
}
