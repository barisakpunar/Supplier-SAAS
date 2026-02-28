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
        AvailableManualTransactionTypes = new List<SelectListItem>();
        Transactions = new List<DealerTransactionItemModel>();
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

    public bool OpenAccountEnabled { get; set; }

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

    public int ManualTransactionTypeId { get; set; }

    public decimal ManualTransactionAmount { get; set; }

    [StringLength(1000)]
    public string ManualTransactionNote { get; set; }

    public List<DealerTransactionItemModel> Transactions { get; set; }

    #endregion
}
