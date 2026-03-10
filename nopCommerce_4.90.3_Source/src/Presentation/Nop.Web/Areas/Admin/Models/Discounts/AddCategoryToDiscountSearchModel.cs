using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Discounts;

/// <summary>
/// Represents a category search model to add to the discount
/// </summary>
public partial record AddCategoryToDiscountSearchModel : BaseSearchModel
{
    #region Ctor

    public AddCategoryToDiscountSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public int DiscountId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
    public int SearchStoreId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public bool IsStoreOwner { get; set; }

    #endregion
}
