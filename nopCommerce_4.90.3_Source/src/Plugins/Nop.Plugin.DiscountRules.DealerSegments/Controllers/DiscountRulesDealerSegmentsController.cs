using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.DealerSegments.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.DealerSegments.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class DiscountRulesDealerSegmentsController : BasePluginController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IDealerService _dealerService;
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IStoreService _storeService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public DiscountRulesDealerSegmentsController(ICustomerService customerService,
        IDealerService dealerService,
        IDiscountService discountService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _dealerService = dealerService;
        _discountService = discountService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Get errors message from model state
    /// </summary>
    /// <param name="modelState">Model state</param>
    /// <returns>Errors message</returns>
    protected IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
    {
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }

    protected virtual async Task<(bool isStoreOwner, int managedStoreId)> GetStoreOwnerContextAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer is null || await _customerService.IsAdminAsync(customer))
            return (false, 0);

        var isStoreOwner = await _customerService.IsInCustomerRoleAsync(customer, NopCustomerDefaults.StoreOwnersRoleName);
        if (!isStoreOwner)
            return (false, 0);

        return (true, customer.RegisteredInStoreId);
    }

    protected virtual async Task<IList<int>> GetAllowedStoreIdsAsync(Discount discount)
    {
        var (isStoreOwner, managedStoreId) = await GetStoreOwnerContextAsync();
        if (isStoreOwner)
            return managedStoreId > 0 ? [managedStoreId] : [];

        var canManageGlobalStoreScope = await _permissionService.AuthorizeAsync(StandardPermission.Promotions.DISCOUNTS_MANAGE_GLOBAL);
        if (!discount.LimitedToStores && canManageGlobalStoreScope)
            return [];

        return (await _storeMappingService.GetStoresIdsWithAccessAsync(discount)).Distinct().ToList();
    }

    protected virtual async Task<IList<SelectListItem>> PrepareDealerSegmentSelectListAsync(Discount discount, int selectedSegmentId)
    {
        var allowedStoreIds = await GetAllowedStoreIdsAsync(discount);
        if (allowedStoreIds.Count != 1)
            return new List<SelectListItem>();

        var stores = (await _storeService.GetAllStoresAsync())
            .Where(store => allowedStoreIds.Contains(store.Id))
            .Select(store => store.Id)
            .ToList();

        var availableSegments = new List<DealerSegment>();
        foreach (var storeId in stores)
        {
            var segments = await _dealerService.SearchDealerSegmentsAsync(storeId: storeId, active: true);
            availableSegments.AddRange(segments);
        }

        return availableSegments
            .GroupBy(segment => segment.Id)
            .Select(group => group.First())
            .OrderBy(segment => segment.StoreId)
            .ThenBy(segment => segment.DisplayOrder)
            .ThenBy(segment => segment.Name)
            .Select(segment => new SelectListItem
            {
                Text = segment.Name,
                Value = segment.Id.ToString(),
                Selected = segment.Id == selectedSegmentId
            })
            .ToList();
    }

    protected virtual async Task ValidateRequirementModelAsync(RequirementModel model, Discount discount)
    {
        var allowedStoreIds = await GetAllowedStoreIdsAsync(discount);
        if (allowedStoreIds.Count != 1)
        {
            ModelState.AddModelError(nameof(model.DealerSegmentId),
                await _localizationService.GetResourceAsync("Plugins.DiscountRules.DealerSegments.Fields.StoreScope.SingleStoreRequired"));
            return;
        }

        var dealerSegment = await _dealerService.GetDealerSegmentByIdAsync(model.DealerSegmentId);
        if (dealerSegment == null || !dealerSegment.Active || !allowedStoreIds.Contains(dealerSegment.StoreId))
        {
            ModelState.AddModelError(nameof(model.DealerSegmentId),
                await _localizationService.GetResourceAsync("Plugins.DiscountRules.DealerSegments.Fields.DealerSegment.Invalid"));
        }
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        //load the discount
        var discount = await _discountService.GetDiscountByIdAsync(discountId)
                       ?? throw new ArgumentException("Discount could not be loaded");

        //check whether the discount requirement exists
        if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
            return Content("Failed to load requirement.");

        //try to get previously saved restricted dealer segment identifier
        var restrictedSegmentId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SettingsKey, discountRequirementId ?? 0));
        var availableDealerSegments = await PrepareDealerSegmentSelectListAsync(discount, restrictedSegmentId);

        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId,
            DealerSegmentId = restrictedSegmentId,
            AvailableDealerSegments = availableDealerSegments
        };
        model.AvailableDealerSegments.Insert(0, new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Plugins.DiscountRules.DealerSegments.Fields.DealerSegment.Select"),
            Value = "0"
        });

        //set the HTML field prefix
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);

        return View("~/Plugins/DiscountRules.DealerSegments/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (ModelState.IsValid)
        {
            //load the discount
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            await ValidateRequirementModelAsync(model, discount);
            if (!ModelState.IsValid)
                return Ok(new { Errors = GetErrorsFromModelState(ModelState) });

            //get the discount requirement
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

            //the discount requirement does not exist, so create a new one
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SystemName
                };

                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
            }

            //save restricted dealer segment identifier
            await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SettingsKey, discountRequirement.Id), model.DealerSegmentId);

            return Ok(new { NewRequirementId = discountRequirement.Id });
        }

        return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
    }

    #endregion
}
