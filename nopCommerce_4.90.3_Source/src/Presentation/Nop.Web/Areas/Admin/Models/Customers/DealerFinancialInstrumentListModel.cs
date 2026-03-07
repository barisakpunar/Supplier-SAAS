using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer financial instrument list model
/// </summary>
public partial record DealerFinancialInstrumentListModel : BaseNopModel
{
    public DealerFinancialInstrumentListModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableDealers = new List<SelectListItem>();
        AvailableInstrumentTypes = new List<SelectListItem>();
        AvailableInstrumentStatuses = new List<SelectListItem>();
        Instruments = new List<DealerFinancialInstrumentListItemModel>();
    }

    public bool IsStoreOwner { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.Store")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.Dealer")]
    public int SearchDealerId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.Type")]
    public int SearchInstrumentTypeId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.Status")]
    public int SearchInstrumentStatusId { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.DueDateFrom")]
    public DateTime? SearchDueDateFromUtc { get; set; }

    [NopResourceDisplayName("Admin.Customers.DealerFinancialInstruments.Fields.DueDateTo")]
    public DateTime? SearchDueDateToUtc { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDealers { get; set; }

    public IList<SelectListItem> AvailableInstrumentTypes { get; set; }

    public IList<SelectListItem> AvailableInstrumentStatuses { get; set; }

    public IList<DealerFinancialInstrumentListItemModel> Instruments { get; set; }
}
