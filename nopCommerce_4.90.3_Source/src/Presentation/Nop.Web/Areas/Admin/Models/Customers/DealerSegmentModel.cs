using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer segment model
/// </summary>
public partial record DealerSegmentModel : BaseNopEntityModel
{
    #region Ctor

    public DealerSegmentModel()
    {
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [Required]
    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Name")]
    public string Name { get; set; }

    [Required]
    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Code")]
    public string Code { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Description")]
    public string Description { get; set; }

    [Range(1, int.MaxValue)]
    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Store")]
    public int StoreId { get; set; }

    public string StoreName { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerSegments.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public bool IsStoreOwner { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    #endregion
}
