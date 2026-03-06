using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer collection model
/// </summary>
public partial record DealerCollectionModel : BaseNopEntityModel
{
    #region Ctor

    public DealerCollectionModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableDealers = new List<SelectListItem>();
        AvailableCustomers = new List<SelectListItem>();
        AvailableCollectionMethods = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public bool IsStoreOwner { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Store")]
    public int StoreId { get; set; }

    public string StoreName { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Dealer")]
    public int DealerId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Customer")]
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.CollectionMethod")]
    public int CollectionMethodId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Amount")]
    public decimal Amount { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.CollectionDate")]
    public DateTime CollectionDateUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.ReferenceNo")]
    public string ReferenceNo { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.DocumentNo")]
    public string DocumentNo { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.IssueDate")]
    public DateTime? IssueDateUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.DueDate")]
    public DateTime? DueDateUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerCollections.Fields.Note")]
    public string Note { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDealers { get; set; }

    public IList<SelectListItem> AvailableCustomers { get; set; }

    public IList<SelectListItem> AvailableCollectionMethods { get; set; }

    #endregion
}
