using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents dealer financial instrument details model
/// </summary>
public partial record DealerFinancialInstrumentDetailsModel : BaseNopEntityModel
{
    public DealerFinancialInstrumentDetailsModel()
    {
        AuditTrail = new List<DealerFinanceAuditLogItemModel>();
    }

    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public int DealerId { get; set; }

    public string DealerName { get; set; }

    public int? DealerCollectionId { get; set; }

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string InstrumentType { get; set; }

    public string InstrumentStatus { get; set; }

    public decimal Amount { get; set; }

    public string InstrumentNo { get; set; }

    public DateTime? IssueDateUtc { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public string BankName { get; set; }

    public string BranchName { get; set; }

    public string AccountNo { get; set; }

    public string DrawerName { get; set; }

    public string Note { get; set; }

    public string CreatedByCustomerName { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool CanMarkCollected { get; set; }

    public bool CanMarkReturned { get; set; }

    public bool CanMarkProtested { get; set; }

    public IList<DealerFinanceAuditLogItemModel> AuditTrail { get; set; }
}
