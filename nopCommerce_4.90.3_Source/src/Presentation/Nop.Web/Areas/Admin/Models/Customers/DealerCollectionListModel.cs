using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer collection list model
/// </summary>
public partial record DealerCollectionListModel : BaseNopModel
{
    #region Ctor

    public DealerCollectionListModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableDealers = new List<SelectListItem>();
        AvailableCollectionMethods = new List<SelectListItem>();
        AvailableCollectionStatuses = new List<SelectListItem>();
        Collections = new List<DealerCollectionListItemModel>();
    }

    #endregion

    #region Properties

    public bool IsStoreOwner { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Store")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Dealer")]
    public int SearchDealerId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.CollectionMethod")]
    public int SearchCollectionMethodId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Status")]
    public int SearchCollectionStatusId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.CollectionDateFrom")]
    public DateTime? SearchCollectionDateFromUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.CollectionDateTo")]
    public DateTime? SearchCollectionDateToUtc { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDealers { get; set; }

    public IList<SelectListItem> AvailableCollectionMethods { get; set; }

    public IList<SelectListItem> AvailableCollectionStatuses { get; set; }

    public IList<DealerCollectionListItemModel> Collections { get; set; }

    #endregion
}
