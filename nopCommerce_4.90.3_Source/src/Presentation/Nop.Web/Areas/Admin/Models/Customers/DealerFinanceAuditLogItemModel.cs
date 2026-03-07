using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer finance audit log item model
/// </summary>
public partial record DealerFinanceAuditLogItemModel : BaseNopEntityModel
{
    public string EntityType { get; set; }

    public string ActionType { get; set; }

    public string StatusTransition { get; set; }

    public string PerformedByCustomerName { get; set; }

    public DateTime PerformedOnUtc { get; set; }

    public string Note { get; set; }
}
