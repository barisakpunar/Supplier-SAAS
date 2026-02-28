using System.Globalization;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.OpenAccount.Components;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.OpenAccount;

/// <summary>
/// Open account payment processor
/// </summary>
public class OpenAccountPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields

    protected const string ValidationNoDealerResourceKey = "Plugins.Payment.OpenAccount.Validation.NoDealer";
    protected const string ValidationDealerInactiveResourceKey = "Plugins.Payment.OpenAccount.Validation.DealerInactive";
    protected const string ValidationOpenAccountDisabledResourceKey = "Plugins.Payment.OpenAccount.Validation.NotEnabled";
    protected const string ValidationCreditExceededResourceKey = "Plugins.Payment.OpenAccount.Validation.CreditLimitExceeded";

    protected readonly IDealerService _dealerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly OpenAccountPaymentSettings _openAccountPaymentSettings;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public OpenAccountPaymentProcessor(IDealerService dealerService,
        ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        OpenAccountPaymentSettings openAccountPaymentSettings,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _dealerService = dealerService;
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _openAccountPaymentSettings = openAccountPaymentSettings;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> GetOpenAccountValidationErrorAsync(int customerId, decimal orderTotal)
    {
        if (customerId <= 0)
            return await _localizationService.GetResourceAsync(ValidationNoDealerResourceKey);

        var dealerId = await _dealerService.GetDealerIdByCustomerIdAsync(customerId);
        if (dealerId <= 0)
            return await _localizationService.GetResourceAsync(ValidationNoDealerResourceKey);

        var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
        if (dealer == null || !dealer.Active)
            return await _localizationService.GetResourceAsync(ValidationDealerInactiveResourceKey);

        var financialProfile = await _dealerService.GetDealerFinancialProfileByDealerIdAsync(dealerId);
        if (financialProfile == null || !financialProfile.OpenAccountEnabled)
            return await _localizationService.GetResourceAsync(ValidationOpenAccountDisabledResourceKey);

        var availableCredit = await _dealerService.GetOpenAccountAvailableCreditAsync(dealerId);
        if (availableCredit >= orderTotal)
            return string.Empty;

        var defaultTemplate = "Open account credit limit is exceeded. Available credit: {0}, order total: {1}.";
        var template = await _localizationService.GetResourceAsync(ValidationCreditExceededResourceKey,
            0,
            false,
            defaultTemplate,
            false);

        return string.Format(CultureInfo.InvariantCulture,
            template,
            availableCredit.ToString("0.####", CultureInfo.InvariantCulture),
            orderTotal.ToString("0.####", CultureInfo.InvariantCulture));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult
        {
            NewPaymentStatus = PaymentStatus.Pending
        };

        var validationError = await GetOpenAccountValidationErrorAsync(processPaymentRequest.CustomerId, processPaymentRequest.OrderTotal);
        if (!string.IsNullOrEmpty(validationError))
            result.AddError(validationError);

        return result;
    }

    /// <summary>
    /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
    /// </summary>
    /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        //nothing
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a value indicating whether payment method should be hidden during checkout
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true - hide; false - display.
    /// </returns>
    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        if (cart == null || !cart.Any())
            return true;

        var customerId = cart.First().CustomerId;
        var cartTotal = (await _orderTotalCalculationService
            .GetShoppingCartTotalAsync(cart, usePaymentMethodAdditionalFee: false)).shoppingCartTotal ?? 0;

        var validationError = await GetOpenAccountValidationErrorAsync(customerId, cartTotal);
        return !string.IsNullOrEmpty(validationError);
    }

    /// <summary>
    /// Gets additional handling fee
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the additional handling fee
    /// </returns>
    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(decimal.Zero);
    }

    /// <summary>
    /// Captures payment
    /// </summary>
    /// <param name="capturePaymentRequest">Capture payment request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the capture payment result
    /// </returns>
    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="refundPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
    }

    /// <summary>
    /// Voids a payment
    /// </summary>
    /// <param name="voidPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
    }

    /// <summary>
    /// Process recurring payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Cancels a recurring payment
    /// </summary>
    /// <param name="cancelPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        //it's not a redirection payment method. So we always return false
        return Task.FromResult(false);
    }

    /// <summary>
    /// Validate payment form
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of validating errors
    /// </returns>
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }

    /// <summary>
    /// Get payment information
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the payment info holder
    /// </returns>
    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        return Task.FromResult(new ProcessPaymentRequest());
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentOpenAccount/Configure";
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    /// <returns>View component type</returns>
    public Type GetPublicViewComponent()
    {
        return typeof(OpenAccountViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        var settings = new OpenAccountPaymentSettings
        {
            DescriptionText = "<p>Open account payment is available according to your dealer credit limit and financial profile.</p>"
        };
        await _settingService.SaveSettingAsync(settings);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payment.OpenAccount.DescriptionText"] = "Description",
            ["Plugins.Payment.OpenAccount.DescriptionText.Hint"] = "Information shown on checkout for open account payment.",
            ["Plugins.Payment.OpenAccount.PaymentMethodDescription"] = "Pay with open account (dealer credit)",
            [ValidationNoDealerResourceKey] = "Open account is not available for this customer.",
            [ValidationDealerInactiveResourceKey] = "Open account is not available because dealer is inactive.",
            [ValidationOpenAccountDisabledResourceKey] = "Open account is disabled for this dealer.",
            [ValidationCreditExceededResourceKey] = "Open account credit limit is exceeded. Available credit: {0}, order total: {1}."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<OpenAccountPaymentSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payment.OpenAccount");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payment.OpenAccount.PaymentMethodDescription");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public bool SupportCapture => false;

    /// <summary>
    /// Gets a value indicating whether partial refund is supported
    /// </summary>
    public bool SupportPartiallyRefund => false;

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public bool SupportRefund => false;

    /// <summary>
    /// Gets a value indicating whether void is supported
    /// </summary>
    public bool SupportVoid => false;

    /// <summary>
    /// Gets a recurring payment type of payment method
    /// </summary>
    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    /// <summary>
    /// Gets a payment method type
    /// </summary>
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

    /// <summary>
    /// Gets a value indicating whether we should display a payment information page for this plugin
    /// </summary>
    public bool SkipPaymentInfo => false;

    #endregion
}
