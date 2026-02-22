using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer model
/// </summary>
public partial record DealerModel : BaseNopEntityModel
{
    #region Ctor

    public DealerModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableCustomers = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
        SelectedCustomerIds = new List<int>();
        SelectedPaymentMethodSystemNames = new List<string>();
    }

    #endregion

    #region Properties

    [Required]
    public string Name { get; set; }

    [Range(1, int.MaxValue)]
    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public bool Active { get; set; }

    public bool IsStoreOwner { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableCustomers { get; set; }

    public IList<int> SelectedCustomerIds { get; set; }

    public IList<SelectListItem> AvailablePaymentMethods { get; set; }

    public IList<string> SelectedPaymentMethodSystemNames { get; set; }

    #endregion
}
