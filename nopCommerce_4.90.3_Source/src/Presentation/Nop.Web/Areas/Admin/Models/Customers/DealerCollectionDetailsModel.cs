using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents dealer collection details model
/// </summary>
public partial record DealerCollectionDetailsModel : BaseNopEntityModel
{
    public bool CanCancel { get; set; }

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

    public string DocumentNo { get; set; }

    public DateTime? IssueDateUtc { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public string Note { get; set; }

    public string CreatedByCustomerName { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public int? DealerTransactionId { get; set; }

    public DateTime? DealerTransactionCreatedOnUtc { get; set; }

    public int? DealerFinancialInstrumentId { get; set; }

    public string DealerFinancialInstrumentType { get; set; }

    public string DealerFinancialInstrumentStatus { get; set; }

    public int? CancelledDealerTransactionId { get; set; }

    public DateTime? CancelledDealerTransactionCreatedOnUtc { get; set; }

    public string CancelledByCustomerName { get; set; }

    public DateTime? CancelledOnUtc { get; set; }
}
