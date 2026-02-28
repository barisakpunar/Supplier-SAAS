using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.OpenAccount.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.OpenAccount.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class PaymentOpenAccountController : BasePaymentController
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public PaymentOpenAccountController(ILanguageService languageService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _languageService = languageService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var openAccountPaymentSettings = await _settingService.LoadSettingAsync<OpenAccountPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            DescriptionText = openAccountPaymentSettings.DescriptionText,
            ActiveStoreScopeConfiguration = storeScope
        };

        await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
        {
            locale.DescriptionText = await _localizationService.GetLocalizedSettingAsync(openAccountPaymentSettings,
                x => x.DescriptionText, languageId, 0, false, false);
        });

        if (storeScope > 0)
        {
            model.DescriptionText_OverrideForStore = await _settingService
                .SettingExistsAsync(openAccountPaymentSettings, x => x.DescriptionText, storeScope);
        }

        return View("~/Plugins/Payments.OpenAccount/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var openAccountPaymentSettings = await _settingService.LoadSettingAsync<OpenAccountPaymentSettings>(storeScope);

        openAccountPaymentSettings.DescriptionText = model.DescriptionText;

        await _settingService.SaveSettingOverridablePerStoreAsync(openAccountPaymentSettings,
            x => x.DescriptionText,
            model.DescriptionText_OverrideForStore,
            storeScope,
            false);

        await _settingService.ClearCacheAsync();

        foreach (var localized in model.Locales)
        {
            await _localizationService.SaveLocalizedSettingAsync(openAccountPaymentSettings,
                x => x.DescriptionText,
                localized.LanguageId,
                localized.DescriptionText);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}
