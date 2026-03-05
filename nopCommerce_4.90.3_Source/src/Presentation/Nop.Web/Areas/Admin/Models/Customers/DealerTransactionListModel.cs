using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer transaction list model
/// </summary>
public partial record DealerTransactionListModel : BaseNopModel
{
    #region Ctor

    public DealerTransactionListModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableDealers = new List<SelectListItem>();
        Transactions = new List<DealerTransactionListItemModel>();
    }

    #endregion

    #region Properties

    public bool IsStoreOwner { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Transactions.Fields.Store")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Transactions.Fields.Dealer")]
    public int SearchDealerId { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Transactions.Fields.CreatedFrom")]
    public DateTime? SearchCreatedFromUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Transactions.Fields.CreatedTo")]
    public DateTime? SearchCreatedToUtc { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDealers { get; set; }

    public IList<DealerTransactionListItemModel> Transactions { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal NetBalance { get; set; }

    public bool IsStatementMode { get; set; }

    public string StatementDealerName { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal ClosingBalance { get; set; }

    #endregion
}
