using LinqToDB;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Services.Payments;

/// <summary>
/// Represents a payment plugin manager implementation
/// </summary>
public partial class PaymentPluginManager : PluginManager<IPaymentMethod>, IPaymentPluginManager
{
    #region Fields

    protected readonly ISettingService _settingService;
    protected readonly PaymentSettings _paymentSettings;
    protected readonly IRepository<DealerPaymentMethodMapping> _dealerPaymentMethodMappingRepository;

    #endregion

    #region Ctor

    public PaymentPluginManager(ICustomerService customerService,
        IPluginService pluginService,
        IRepository<DealerPaymentMethodMapping> dealerPaymentMethodMappingRepository,
        ISettingService settingService,
        PaymentSettings paymentSettings) : base(customerService, pluginService)
    {
        _dealerPaymentMethodMappingRepository = dealerPaymentMethodMappingRepository;
        _settingService = settingService;
        _paymentSettings = paymentSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Load active payment methods
    /// </summary>
    /// <param name="customer">Filter by customer; pass null to load all plugins</param>
    /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
    /// <param name="countryId">Filter by country; pass 0 to load all plugins</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of active payment methods
    /// </returns>
    public virtual async Task<IList<IPaymentMethod>> LoadActivePluginsAsync(Customer customer = null, int storeId = 0,
        int countryId = 0)
    {
        var paymentMethods = await LoadActivePluginsAsync(_paymentSettings.ActivePaymentMethodSystemNames, customer, storeId);

        //if explicit payment method capabilities exist for this dealer, apply them
        if (customer?.Id > 0)
        {
            var allowedPaymentSystemNames = await _dealerPaymentMethodMappingRepository.Table
                .Where(mapping => mapping.CustomerId == customer.Id)
                .Select(mapping => mapping.PaymentMethodSystemName)
                .ToListAsync();

            if (allowedPaymentSystemNames.Any())
            {
                var allowedPaymentSystemNamesSet = new HashSet<string>(allowedPaymentSystemNames, StringComparer.InvariantCultureIgnoreCase);
                paymentMethods = paymentMethods
                    .Where(method => allowedPaymentSystemNamesSet.Contains(method.PluginDescriptor.SystemName))
                    .ToList();
            }
        }

        //filter by country
        if (countryId > 0)
            paymentMethods = await paymentMethods.WhereAwait(async method => !(await GetRestrictedCountryIdsAsync(method)).Contains(countryId)).ToListAsync();

        return paymentMethods;
    }

    /// <summary>
    /// Check whether the passed payment method is active
    /// </summary>
    /// <param name="paymentMethod">Payment method to check</param>
    /// <returns>Result</returns>
    public virtual bool IsPluginActive(IPaymentMethod paymentMethod)
    {
        return IsPluginActive(paymentMethod, _paymentSettings.ActivePaymentMethodSystemNames);
    }

    /// <summary>
    /// Check whether the payment method with the passed system name is active
    /// </summary>
    /// <param name="systemName">System name of payment method to check</param>
    /// <param name="customer">Filter by customer; pass null to load all plugins</param>
    /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public virtual async Task<bool> IsPluginActiveAsync(string systemName, Customer customer = null, int storeId = 0)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return false;

        var activeMethods = await LoadActivePluginsAsync(customer, storeId);
        return activeMethods.Any(method => method.PluginDescriptor.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Get countries in which the passed payment method is not allowed
    /// </summary>
    /// <param name="paymentMethod">Payment method</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of country identifiers
    /// </returns>
    public virtual async Task<IList<int>> GetRestrictedCountryIdsAsync(IPaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(paymentMethod);

        var settingKey = string.Format(NopPaymentDefaults.RestrictedCountriesSettingName, paymentMethod.PluginDescriptor.SystemName);

        return await _settingService.GetSettingByKeyAsync<List<int>>(settingKey) ?? new List<int>();
    }

    /// <summary>
    /// Save countries in which the passed payment method is not allowed
    /// </summary>
    /// <param name="paymentMethod">Payment method</param>
    /// <param name="countryIds">List of country identifiers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SaveRestrictedCountriesAsync(IPaymentMethod paymentMethod, IList<int> countryIds)
    {
        ArgumentNullException.ThrowIfNull(paymentMethod);

        var settingKey = string.Format(NopPaymentDefaults.RestrictedCountriesSettingName, paymentMethod.PluginDescriptor.SystemName);

        await _settingService.SetSettingAsync(settingKey, countryIds.ToList());
    }

    #endregion
}
