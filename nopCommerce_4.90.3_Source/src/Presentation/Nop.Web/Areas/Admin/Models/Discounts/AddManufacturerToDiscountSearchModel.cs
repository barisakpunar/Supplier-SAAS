using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Discounts;

/// <summary>
/// Represents a manufacturer search model to add to the discount
/// </summary>
public partial record AddManufacturerToDiscountSearchModel : BaseSearchModel
{
    #region Ctor

    public AddManufacturerToDiscountSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public int DiscountId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
    public string SearchManufacturerName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
    public int SearchStoreId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public bool IsStoreOwner { get; set; }

    #endregion
}
