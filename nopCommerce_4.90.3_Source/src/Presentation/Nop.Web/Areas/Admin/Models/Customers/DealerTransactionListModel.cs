using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

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

    [Display(Name = "Store")]
    public int SearchStoreId { get; set; }

    [Display(Name = "Dealer")]
    public int SearchDealerId { get; set; }

    [Display(Name = "Created from")]
    public DateTime? SearchCreatedFromUtc { get; set; }

    [Display(Name = "Created to")]
    public DateTime? SearchCreatedToUtc { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDealers { get; set; }

    public IList<DealerTransactionListItemModel> Transactions { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal NetBalance { get; set; }

    #endregion
}
