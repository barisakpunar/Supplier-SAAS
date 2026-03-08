using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.DealerSegments.Models;

public class RequirementModel
{
    public RequirementModel()
    {
        AvailableDealerSegments = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugins.DiscountRules.DealerSegments.Fields.DealerSegment")]
    public int DealerSegmentId { get; set; }

    public int DiscountId { get; set; }

    public int RequirementId { get; set; }

    public IList<SelectListItem> AvailableDealerSegments { get; set; }
}