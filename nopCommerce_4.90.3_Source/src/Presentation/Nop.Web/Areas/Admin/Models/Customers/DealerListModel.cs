using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer list model
/// </summary>
public partial record DealerListModel : BaseNopModel
{
    #region Ctor

    public DealerListModel()
    {
        Dealers = new List<DealerListItemModel>();
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public string SearchName { get; set; }

    public int SearchStoreId { get; set; }

    public bool IsStoreOwner { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<DealerListItemModel> Dealers { get; set; }

    #endregion
}

/// <summary>
/// Represents a dealer list item model
/// </summary>
public partial record DealerListItemModel : BaseNopEntityModel
{
    #region Properties

    public string Name { get; set; }

    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public bool Active { get; set; }

    public int CustomerCount { get; set; }

    public string PaymentMethodsSummary { get; set; }

    #endregion
}
