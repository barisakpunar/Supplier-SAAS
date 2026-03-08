using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer segment list model
/// </summary>
public partial record DealerSegmentListModel : BaseNopModel
{
    #region Ctor

    public DealerSegmentListModel()
    {
        Segments = new List<DealerSegmentListItemModel>();
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.Customers.DealerSegments.List.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Store")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Active")]
    public bool? SearchActive { get; set; }

    public bool IsStoreOwner { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<DealerSegmentListItemModel> Segments { get; set; }

    #endregion
}

/// <summary>
/// Represents a dealer segment list item model
/// </summary>
public partial record DealerSegmentListItemModel : BaseNopEntityModel
{
    #region Properties

    public string Name { get; set; }

    public string Code { get; set; }

    public string StoreName { get; set; }

    public bool Active { get; set; }

    public int DisplayOrder { get; set; }

    public int DealerCount { get; set; }

    #endregion
}
