using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.OpenAccount.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.OpenAccount.Components;

public class OpenAccountViewComponent : NopViewComponent
{
    protected readonly ILocalizationService _localizationService;
    protected readonly OpenAccountPaymentSettings _openAccountPaymentSettings;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;

    public OpenAccountViewComponent(ILocalizationService localizationService,
        OpenAccountPaymentSettings openAccountPaymentSettings,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _openAccountPaymentSettings = openAccountPaymentSettings;
        _storeContext = storeContext;
        _workContext = workContext;
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var language = await _workContext.GetWorkingLanguageAsync();

        var model = new PaymentInfoModel
        {
            DescriptionText = await _localizationService.GetLocalizedSettingAsync(_openAccountPaymentSettings,
                x => x.DescriptionText,
                language.Id,
                store.Id)
        };

        return View("~/Plugins/Payments.OpenAccount/Views/PaymentInfo.cshtml", model);
    }
}
