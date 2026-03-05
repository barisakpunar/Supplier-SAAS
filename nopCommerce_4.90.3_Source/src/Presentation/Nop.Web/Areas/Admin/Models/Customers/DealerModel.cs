using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

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
        AvailableManualTransactionTypes = new List<SelectListItem>();
        Transactions = new List<DealerTransactionItemModel>();
        SelectedCustomerIds = new List<int>();
        SelectedPaymentMethodSystemNames = new List<string>();
    }

    #endregion

    #region Properties

    [Required]
    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.Name")]
    public string Name { get; set; }

    [Range(1, int.MaxValue)]
    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.Store")]
    public int StoreId { get; set; }

    public string StoreName { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.OpenAccountEnabled")]
    public bool OpenAccountEnabled { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.CreditLimit")]
    public decimal CreditLimit { get; set; }

    public decimal CurrentDebt { get; set; }

    public decimal AvailableCredit { get; set; }

    public bool IsStoreOwner { get; set; }

    public List<SelectListItem> AvailableStores { get; set; }

    public List<SelectListItem> AvailableCustomers { get; set; }

    public List<int> SelectedCustomerIds { get; set; }

    public List<SelectListItem> AvailablePaymentMethods { get; set; }

    public List<string> SelectedPaymentMethodSystemNames { get; set; }

    public List<SelectListItem> AvailableManualTransactionTypes { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.ManualTransactionType")]
    public int ManualTransactionTypeId { get; set; }

    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.ManualTransactionAmount")]
    public decimal ManualTransactionAmount { get; set; }

    [StringLength(1000)]
    [NopResourceDisplayName("Admin.Customers.Dealers.Fields.ManualTransactionNote")]
    public string ManualTransactionNote { get; set; }

    public List<DealerTransactionItemModel> Transactions { get; set; }

    #endregion
}
