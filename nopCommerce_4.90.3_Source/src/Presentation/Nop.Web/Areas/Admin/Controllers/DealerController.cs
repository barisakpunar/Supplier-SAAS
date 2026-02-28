using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

public partial class DealerController : BaseAdminController
{
    #region Fields

    protected const decimal MaxDealerCreditLimit = 99999999999999.9999m;
    protected const int DealerTransactionPreviewPageSize = 100;
    protected readonly ICustomerService _customerService;
    protected readonly IDealerService _dealerService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IStoreService _storeService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public DealerController(ICustomerService customerService,
        IDealerService dealerService,
        IPaymentPluginManager paymentPluginManager,
        IStoreService storeService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _dealerService = dealerService;
        _paymentPluginManager = paymentPluginManager;
        _storeService = storeService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task<(Customer customer, bool isStoreOwner, int managedStoreId, int managedDealerId)> GetAccessContextAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var isStoreOwner = !await _customerService.IsAdminAsync(customer)
                           && await _customerService.IsInCustomerRoleAsync(customer, NopCustomerDefaults.StoreOwnersRoleName);

        var managedStoreId = isStoreOwner ? customer.RegisteredInStoreId : 0;
        var managedDealerId = isStoreOwner ? await _dealerService.GetDealerIdByCustomerIdAsync(customer.Id) : 0;

        return (customer, isStoreOwner, managedStoreId, managedDealerId);
    }

    protected virtual string GetDealerTransactionTypeText(int transactionTypeId)
    {
        return transactionTypeId switch
        {
            (int)DealerTransactionType.OpenAccountOrder => "Open account order",
            (int)DealerTransactionType.OpenAccountCollection => "Open account collection",
            (int)DealerTransactionType.ManualDebitAdjustment => "Manual debit adjustment",
            (int)DealerTransactionType.ManualCreditAdjustment => "Manual credit adjustment",
            _ => $"Type #{transactionTypeId}"
        };
    }

    protected virtual string GetDealerTransactionDirectionText(int directionId)
    {
        return directionId switch
        {
            (int)DealerTransactionDirection.Debit => "Debit",
            (int)DealerTransactionDirection.Credit => "Credit",
            _ => $"Direction #{directionId}"
        };
    }

    protected virtual int GetDirectionIdByManualTransactionType(int manualTransactionTypeId)
    {
        return manualTransactionTypeId switch
        {
            (int)DealerTransactionType.ManualDebitAdjustment => (int)DealerTransactionDirection.Debit,
            (int)DealerTransactionType.ManualCreditAdjustment => (int)DealerTransactionDirection.Credit,
            _ => 0
        };
    }

    protected virtual bool TryParseDecimalValue(string rawValue, out decimal value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        var normalizedValue = rawValue.Trim().Replace(" ", string.Empty);

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
            return true;

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return true;

        if (decimal.TryParse(normalizedValue.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return true;

        return false;
    }

    protected virtual void NormalizeDecimalInput(DealerModel model, string fieldName, Action<decimal> setter)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(setter);

        if (!Request.HasFormContentType || !Request.Form.ContainsKey(fieldName))
            return;

        var rawValue = Request.Form[fieldName].ToString();
        if (!TryParseDecimalValue(rawValue, out var parsedValue))
            return;

        setter(parsedValue);
        ModelState.Remove(fieldName);
    }

    protected virtual async Task PrepareDealerModelAsync(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (_, isStoreOwner, managedStoreId, managedDealerId) = await GetAccessContextAsync();
        model.IsStoreOwner = isStoreOwner;

        var stores = await _storeService.GetAllStoresAsync();
        if (isStoreOwner && managedStoreId > 0)
            model.StoreId = managedStoreId;

        if (!isStoreOwner)
        {
            model.AvailableStores = stores
                .Select(store => new SelectListItem { Text = store.Name, Value = store.Id.ToString() })
                .ToList();
        }
        else
        {
            model.StoreName = stores.FirstOrDefault(store => store.Id == model.StoreId)?.Name ?? "-";
            model.AvailableStores = [];
        }

        if (model.StoreId <= 0)
            model.StoreId = stores.FirstOrDefault()?.Id ?? 0;

        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        var registeredRoleIds = registeredRole is not null ? new[] { registeredRole.Id } : null;

        var customers = await _customerService.GetAllCustomersAsync(
            customerRoleIds: registeredRoleIds,
            storeId: model.StoreId,
            pageSize: int.MaxValue);

        var selectedCustomerIds = new HashSet<int>((model.SelectedCustomerIds ?? []).Where(id => id > 0));
        var selectedCustomers = selectedCustomerIds.Any()
            ? await _customerService.GetCustomersByIdsAsync(selectedCustomerIds.ToArray())
            : [];

        var availableCustomers = customers.Union(selectedCustomers)
            .DistinctBy(customer => customer.Id)
            .OrderBy(customer => customer.Email)
            .ThenBy(customer => customer.Id)
            .Select(customer =>
            {
                var identity = !string.IsNullOrWhiteSpace(customer.Email) ? customer.Email : customer.Username;
                return new SelectListItem
                {
                    Value = customer.Id.ToString(),
                    Text = $"{identity} (#{customer.Id})"
                };
            })
            .ToList();

        model.AvailableCustomers = availableCustomers;
        model.SelectedCustomerIds = model.SelectedCustomerIds ?? [];

        var paymentMethods = await _paymentPluginManager.LoadAllPluginsAsync(storeId: model.StoreId);
        var availablePaymentMethods = paymentMethods
            .OrderBy(method => method.PluginDescriptor.FriendlyName)
            .ThenBy(method => method.PluginDescriptor.SystemName)
            .Select(method => new SelectListItem
            {
                Value = method.PluginDescriptor.SystemName,
                Text = $"{method.PluginDescriptor.FriendlyName} ({method.PluginDescriptor.SystemName})"
            })
            .ToList();

        if (isStoreOwner && (model.Id <= 0 || managedDealerId <= 0 || model.Id != managedDealerId))
        {
            var ownerAllowedSystemNamesSet = await GetStoreOwnerAllowedPaymentMethodSystemNamesAsync(managedDealerId, model.StoreId);
            availablePaymentMethods = availablePaymentMethods
                .Where(item => ownerAllowedSystemNamesSet.Contains(item.Value))
                .ToList();
        }

        model.AvailablePaymentMethods = availablePaymentMethods;
        model.SelectedPaymentMethodSystemNames = model.SelectedPaymentMethodSystemNames ?? [];
        model.AvailableManualTransactionTypes =
        [
            new SelectListItem
            {
                Text = "Manual credit adjustment",
                Value = ((int)DealerTransactionType.ManualCreditAdjustment).ToString()
            },
            new SelectListItem
            {
                Text = "Manual debit adjustment",
                Value = ((int)DealerTransactionType.ManualDebitAdjustment).ToString()
            }
        ];

        var availableManualTypeIds = model.AvailableManualTransactionTypes
            .Select(item => int.TryParse(item.Value, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToHashSet();

        if (!availableManualTypeIds.Contains(model.ManualTransactionTypeId))
            model.ManualTransactionTypeId = (int)DealerTransactionType.ManualCreditAdjustment;

        if (model.Id > 0)
        {
            model.CurrentDebt = await _dealerService.GetOpenAccountCurrentDebtAsync(model.Id);
            model.AvailableCredit = await _dealerService.GetOpenAccountAvailableCreditAsync(model.Id);
            model.Transactions = (await _dealerService.GetDealerTransactionsAsync(model.Id, DealerTransactionPreviewPageSize))
                .Select(transaction => new DealerTransactionItemModel
                {
                    OrderId = transaction.OrderId,
                    CustomerId = transaction.CustomerId,
                    TransactionType = GetDealerTransactionTypeText(transaction.TransactionTypeId),
                    Direction = GetDealerTransactionDirectionText(transaction.DirectionId),
                    Amount = transaction.Amount,
                    Note = transaction.Note,
                    CreatedOnUtc = transaction.CreatedOnUtc
                })
                .ToList();
        }
        else
        {
            model.CurrentDebt = 0;
            model.AvailableCredit = model.OpenAccountEnabled ? model.CreditLimit : 0;
            model.Transactions = [];
        }
    }

    protected virtual async Task SaveDealerMappingsAsync(DealerInfo dealer, DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(model);

        var selectedCustomerIds = (model.SelectedCustomerIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (selectedCustomerIds.Any())
        {
            var selectedCustomers = await _customerService.GetCustomersByIdsAsync(selectedCustomerIds.ToArray());
            selectedCustomerIds = selectedCustomers
                .Where(customer => customer.RegisteredInStoreId == dealer.StoreId)
                .Select(customer => customer.Id)
                .Distinct()
                .ToList();
        }

        var existingCustomerIds = await _dealerService.GetCustomerIdsByDealerIdAsync(dealer.Id);

        foreach (var customerId in existingCustomerIds.Except(selectedCustomerIds).ToList())
            await _dealerService.UnmapCustomerFromDealerAsync(dealer.Id, customerId);

        foreach (var customerId in selectedCustomerIds.Except(existingCustomerIds).ToList())
            await _dealerService.MapCustomerToDealerAsync(dealer.Id, customerId);

        var (_, isStoreOwner, managedStoreId, managedDealerId) = await GetAccessContextAsync();
        if (isStoreOwner && (managedStoreId <= 0 || dealer.StoreId != managedStoreId))
            throw new InvalidOperationException("Store owner cannot manage payment methods outside own store.");

        var availablePaymentMethodSystemNames = (await _paymentPluginManager.LoadAllPluginsAsync(storeId: dealer.StoreId))
            .Select(method => method.PluginDescriptor.SystemName)
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        var selectedPaymentMethodSystemNames = (model.SelectedPaymentMethodSystemNames ?? [])
            .Where(systemName => !string.IsNullOrWhiteSpace(systemName))
            .Select(systemName => systemName.Trim())
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToList();

        var isStoreOwnerEditingOwnDealer = isStoreOwner && managedDealerId > 0 && dealer.Id == managedDealerId;

        if (isStoreOwner && !isStoreOwnerEditingOwnDealer)
        {
            availablePaymentMethodSystemNames = await GetStoreOwnerAllowedPaymentMethodSystemNamesAsync(managedDealerId, dealer.StoreId);

            var existingDealerSystemNames = (await _dealerService.GetAllowedPaymentMethodSystemNamesAsync(dealer.Id))
                .Where(systemName => !string.IsNullOrWhiteSpace(systemName))
                .Select(systemName => systemName.Trim())
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();
            var preservedOutsideScope = existingDealerSystemNames
                .Where(systemName => !availablePaymentMethodSystemNames.Contains(systemName));

            selectedPaymentMethodSystemNames = selectedPaymentMethodSystemNames
                .Where(availablePaymentMethodSystemNames.Contains)
                .Concat(preservedOutsideScope)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }
        else
        {
            selectedPaymentMethodSystemNames = selectedPaymentMethodSystemNames
                .Where(availablePaymentMethodSystemNames.Contains)
                .ToList();
        }

        await _dealerService.SetAllowedPaymentMethodSystemNamesAsync(dealer.Id, selectedPaymentMethodSystemNames);
    }

    protected virtual async Task<HashSet<string>> GetStoreOwnerAllowedPaymentMethodSystemNamesAsync(int managedDealerId, int storeId)
    {
        var storePaymentMethodSystemNames = (await _paymentPluginManager.LoadAllPluginsAsync(storeId: storeId))
            .Select(method => method.PluginDescriptor.SystemName)
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        if (managedDealerId <= 0)
            return storePaymentMethodSystemNames;

        var ownerAllowedSystemNames = await _dealerService.GetAllowedPaymentMethodSystemNamesAsync(managedDealerId);

        //empty mapping means "all active payment methods" for the dealer
        if (!ownerAllowedSystemNames.Any())
            return storePaymentMethodSystemNames;

        storePaymentMethodSystemNames.IntersectWith(ownerAllowedSystemNames);
        return storePaymentMethodSystemNames;
    }

    protected virtual void NormalizeSelectedPaymentMethodsFromRequest(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.SelectedPaymentMethodSystemNames = [];

        if (!Request.HasFormContentType || !Request.Form.ContainsKey(nameof(DealerModel.SelectedPaymentMethodSystemNames)))
            return;

        var postedValues = Request.Form[nameof(DealerModel.SelectedPaymentMethodSystemNames)];
        if (postedValues.Count == 0)
            return;

        model.SelectedPaymentMethodSystemNames = postedValues
            .SelectMany(item => (item ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToList();
    }

    protected virtual void ValidateDealerFinancialInputs(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.CreditLimit < 0 || model.CreditLimit > MaxDealerCreditLimit)
            ModelState.AddModelError(nameof(model.CreditLimit), $"Credit limit must be between 0 and {MaxDealerCreditLimit:0.####}.");

        if (decimal.Round(model.CreditLimit, 4) != model.CreditLimit)
            ModelState.AddModelError(nameof(model.CreditLimit), "Credit limit can contain up to 4 decimal digits.");
    }

    protected virtual void ValidateManualTransactionInputs(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var directionId = GetDirectionIdByManualTransactionType(model.ManualTransactionTypeId);
        if (directionId <= 0)
            ModelState.AddModelError(nameof(model.ManualTransactionTypeId), "A valid transaction type is required.");

        if (model.ManualTransactionAmount <= 0 || model.ManualTransactionAmount > MaxDealerCreditLimit)
            ModelState.AddModelError(nameof(model.ManualTransactionAmount), $"Amount must be between 0.0001 and {MaxDealerCreditLimit:0.####}.");

        if (decimal.Round(model.ManualTransactionAmount, 4) != model.ManualTransactionAmount)
            ModelState.AddModelError(nameof(model.ManualTransactionAmount), "Amount can contain up to 4 decimal digits.");
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> List(DealerListModel searchModel)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        searchModel ??= new DealerListModel();
        searchModel.IsStoreOwner = isStoreOwner;

        var stores = await _storeService.GetAllStoresAsync();
        if (!isStoreOwner)
        {
            searchModel.AvailableStores = new List<SelectListItem>
            {
                new() { Text = "All", Value = "0" }
            };

            foreach (var store in stores)
                searchModel.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
        }
        else
        {
            searchModel.SearchStoreId = managedStoreId;
            searchModel.AvailableStores = [];
        }

        IList<DealerInfo> dealers;
        if (isStoreOwner)
            dealers = await _dealerService.SearchDealersAsync(name: searchModel.SearchName, storeId: managedStoreId, pageSize: int.MaxValue);
        else
            dealers = await _dealerService.SearchDealersAsync(name: searchModel.SearchName, storeId: searchModel.SearchStoreId, pageSize: int.MaxValue);

        var storesById = stores.ToDictionary(store => store.Id);

        searchModel.Dealers = new List<DealerListItemModel>();
        foreach (var dealer in dealers.OrderBy(item => item.Name).ThenBy(item => item.Id))
        {
            var customerIds = await _dealerService.GetCustomerIdsByDealerIdAsync(dealer.Id);
            var paymentMethodSystemNames = await _dealerService.GetAllowedPaymentMethodSystemNamesAsync(dealer.Id);
            var paymentMethodsSummary = !paymentMethodSystemNames.Any()
                ? "All active payment methods"
                : string.Join(", ", paymentMethodSystemNames.Take(4))
                  + (paymentMethodSystemNames.Count > 4 ? ", ..." : string.Empty);

            searchModel.Dealers.Add(new DealerListItemModel
            {
                Id = dealer.Id,
                Name = dealer.Name,
                StoreId = dealer.StoreId,
                StoreName = storesById.TryGetValue(dealer.StoreId, out var store) ? store.Name : "-",
                Active = dealer.Active,
                CustomerCount = customerIds.Count,
                PaymentMethodsSummary = paymentMethodsSummary
            });
        }

        return View(searchModel);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        var (_, isStoreOwner, _, _) = await GetAccessContextAsync();
        if (isStoreOwner)
            return AccessDeniedView();

        var model = new DealerModel { Active = true };
        await PrepareDealerModelAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(DealerModel model, bool continueEditing)
    {
        var (_, isStoreOwner, _, _) = await GetAccessContextAsync();
        if (isStoreOwner)
            return AccessDeniedView();

        NormalizeSelectedPaymentMethodsFromRequest(model);
        NormalizeDecimalInput(model, nameof(DealerModel.CreditLimit), value => model.CreditLimit = value);

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store is null)
            ModelState.AddModelError(nameof(model.StoreId), "A valid store is required.");
        ValidateDealerFinancialInputs(model);

        if (!ModelState.IsValid)
        {
            await PrepareDealerModelAsync(model);
            return View(model);
        }

        var dealer = new DealerInfo
        {
            Name = model.Name.Trim(),
            StoreId = model.StoreId,
            Active = model.Active
        };

        await _dealerService.InsertDealerAsync(dealer);
        await _dealerService.UpsertDealerFinancialProfileAsync(dealer.Id, model.OpenAccountEnabled, model.CreditLimit);
        await SaveDealerMappingsAsync(dealer, model);

        if (continueEditing)
            return RedirectToAction(nameof(Edit), new { id = dealer.Id });

        return RedirectToAction(nameof(List));
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();

        var dealer = await _dealerService.GetDealerByIdAsync(id);
        if (dealer is null)
            return RedirectToAction(nameof(List));

        if (isStoreOwner)
        {
            if (managedStoreId > 0 && dealer.StoreId != managedStoreId)
                return AccessDeniedView();
        }

        var model = new DealerModel
        {
            Id = dealer.Id,
            Name = dealer.Name,
            StoreId = dealer.StoreId,
            Active = dealer.Active,
            SelectedCustomerIds = (await _dealerService.GetCustomerIdsByDealerIdAsync(dealer.Id)).ToList(),
            SelectedPaymentMethodSystemNames = (await _dealerService.GetAllowedPaymentMethodSystemNamesAsync(dealer.Id)).ToList()
        };
        var financialProfile = await _dealerService.GetDealerFinancialProfileByDealerIdAsync(dealer.Id);
        model.OpenAccountEnabled = financialProfile?.OpenAccountEnabled ?? false;
        model.CreditLimit = financialProfile?.CreditLimit ?? 0;

        await PrepareDealerModelAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [ActionName("Edit")]
    [FormValueRequired("add-transaction")]
    public virtual async Task<IActionResult> AddTransaction(DealerModel model)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();

        var dealer = await _dealerService.GetDealerByIdAsync(model.Id);
        if (dealer is null)
            return RedirectToAction(nameof(List));

        if (isStoreOwner && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        NormalizeDecimalInput(model, nameof(DealerModel.ManualTransactionAmount), value => model.ManualTransactionAmount = value);
        ModelState.Remove(nameof(DealerModel.CreditLimit));

        var directionId = GetDirectionIdByManualTransactionType(model.ManualTransactionTypeId);
        ValidateManualTransactionInputs(model);

        if (ModelState.IsValid)
        {
            await _dealerService.InsertDealerTransactionAsync(new DealerTransaction
            {
                DealerId = dealer.Id,
                TransactionTypeId = model.ManualTransactionTypeId,
                DirectionId = directionId,
                Amount = model.ManualTransactionAmount,
                Note = string.IsNullOrWhiteSpace(model.ManualTransactionNote) ? null : model.ManualTransactionNote.Trim()
            });

            return RedirectToAction(nameof(Edit), new { id = dealer.Id });
        }

        model.Id = dealer.Id;
        model.Name = dealer.Name;
        model.StoreId = dealer.StoreId;
        model.Active = dealer.Active;
        model.SelectedCustomerIds = (await _dealerService.GetCustomerIdsByDealerIdAsync(dealer.Id)).ToList();
        model.SelectedPaymentMethodSystemNames = (await _dealerService.GetAllowedPaymentMethodSystemNamesAsync(dealer.Id)).ToList();

        var financialProfile = await _dealerService.GetDealerFinancialProfileByDealerIdAsync(dealer.Id);
        model.OpenAccountEnabled = financialProfile?.OpenAccountEnabled ?? false;
        model.CreditLimit = financialProfile?.CreditLimit ?? 0;

        await PrepareDealerModelAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(DealerModel model, bool continueEditing)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        NormalizeSelectedPaymentMethodsFromRequest(model);
        NormalizeDecimalInput(model, nameof(DealerModel.CreditLimit), value => model.CreditLimit = value);

        var dealer = await _dealerService.GetDealerByIdAsync(model.Id);
        if (dealer is null)
            return RedirectToAction(nameof(List));

        if (isStoreOwner)
        {
            if (managedStoreId > 0 && dealer.StoreId != managedStoreId)
                return AccessDeniedView();
            model.StoreId = managedStoreId;
        }

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store is null)
            ModelState.AddModelError(nameof(model.StoreId), "A valid store is required.");
        ValidateDealerFinancialInputs(model);

        if (!ModelState.IsValid)
        {
            await PrepareDealerModelAsync(model);
            return View(model);
        }

        dealer.Name = model.Name.Trim();
        dealer.StoreId = model.StoreId;
        dealer.Active = model.Active;

        await _dealerService.UpdateDealerAsync(dealer);
        await _dealerService.UpsertDealerFinancialProfileAsync(dealer.Id, model.OpenAccountEnabled, model.CreditLimit);

        //customer mappings are readonly on edit; preserve existing mappings
        model.SelectedCustomerIds = (await _dealerService.GetCustomerIdsByDealerIdAsync(dealer.Id)).ToList();
        await SaveDealerMappingsAsync(dealer, model);

        if (continueEditing)
            return RedirectToAction(nameof(Edit), new { id = dealer.Id });

        return RedirectToAction(nameof(List));
    }

    #endregion
}
