namespace Nop.Plugin.DiscountRules.DealerSegments;

/// <summary>
/// Represents defaults for the discount requirement rule
/// </summary>
public static class DiscountRequirementDefaults
{
    /// <summary>
    /// The system name of the discount requirement rule
    /// </summary>
    public static string SystemName => "DiscountRequirement.MustBeInDealerSegment";

    /// <summary>
    /// The key of the settings to save restricted dealer segments
    /// </summary>
    public static string SettingsKey => "DiscountRequirement.MustBeInDealerSegment-{0}";

    /// <summary>
    /// The HTML field prefix for discount requirements
    /// </summary>
    public static string HtmlFieldPrefix => "DiscountRulesDealerSegments{0}";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "DiscountRequirement.MustBeInDealerSegment.Configure";
}
