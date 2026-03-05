using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer collection list item model
/// </summary>
public partial record DealerCollectionListItemModel : BaseNopEntityModel
{
    public int DealerId { get; set; }

    public string DealerName { get; set; }

    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string CollectionMethod { get; set; }

    public string CollectionStatus { get; set; }

    public decimal Amount { get; set; }

    public DateTime CollectionDateUtc { get; set; }

    public string ReferenceNo { get; set; }

    public string Note { get; set; }
}
