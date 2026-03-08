using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.DealerSegments;

public class DealerSegmentDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
{
    #region Fields

    protected readonly IDealerService _dealerService;
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INopUrlHelper _nopUrlHelper;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public DealerSegmentDiscountRequirementRule(IDiscountService discountService,
        IDealerService dealerService,
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _dealerService = dealerService;
        _discountService = discountService;
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check discount requirement
    /// </summary>
    /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        //invalid by default
        var result = new DiscountRequirementValidationResult();

        if (request.Customer == null)
            return result;

        //try to get saved restricted dealer segment identifier
        var restrictedSegmentId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SettingsKey, request.DiscountRequirementId));
        if (restrictedSegmentId == 0)
            return result;

        var dealer = await _dealerService.GetDealerByCustomerIdAsync(request.Customer.Id);
        if (dealer == null)
            return result;

        var dealerSegment = await _dealerService.GetDealerSegmentByIdAsync(restrictedSegmentId);
        if (dealerSegment == null || !dealerSegment.Active)
            return result;

        //result is valid if the customer's dealer is mapped to the configured segment
        result.IsValid = await _dealerService.IsDealerMappedToSegmentAsync(dealer.Id, restrictedSegmentId);

        return result;
    }

    /// <summary>
    /// Get URL for rule configuration
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
    /// <returns>URL</returns>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        return _nopUrlHelper.RouteUrl(DiscountRequirementDefaults.ConfigurationRouteName,
            new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.DiscountRules.DealerSegments.Fields.DealerSegment"] = "Required dealer segment",
            ["Plugins.DiscountRules.DealerSegments.Fields.DealerSegment.Hint"] = "Discount will be applied if the current customer's dealer is mapped to the selected dealer segment.",
            ["Plugins.DiscountRules.DealerSegments.Fields.DealerSegment.Select"] = "Select dealer segment",
            ["Plugins.DiscountRules.DealerSegments.Fields.DealerSegmentId.Required"] = "Dealer segment is required",
            ["Plugins.DiscountRules.DealerSegments.Fields.DiscountId.Required"] = "Discount is required",
            ["Plugins.DiscountRules.DealerSegments.Fields.StoreScope.Required"] = "This discount must be limited to at least one store before configuring a dealer segment requirement.",
            ["Plugins.DiscountRules.DealerSegments.Fields.DealerSegment.Invalid"] = "Selected dealer segment is not available for this discount."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //discount requirements
        var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
        foreach (var discountRequirement in discountRequirements)
        {
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.DealerSegments");

        await base.UninstallAsync();
    }

    #endregion
}
