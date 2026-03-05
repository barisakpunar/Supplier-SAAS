using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Localization;
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
    protected readonly ILocalizationService _localizationService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IStoreService _storeService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public DealerController(ICustomerService customerService,
        IDealerService dealerService,
        ILocalizationService localizationService,
        IPaymentPluginManager paymentPluginManager,
        IStoreService storeService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _dealerService = dealerService;
        _localizationService = localizationService;
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

    protected virtual string GetDealerTransactionTypeResourceKey(int transactionTypeId)
    {
        return transactionTypeId switch
        {
            (int)DealerTransactionType.OpenAccountOrder => "Admin.Customers.Dealers.Transactions.Type.OpenAccountOrder",
            (int)DealerTransactionType.OpenAccountCollection => "Admin.Customers.Dealers.Transactions.Type.OpenAccountCollection",
            (int)DealerTransactionType.ManualDebitAdjustment => "Admin.Customers.Dealers.Transactions.Type.ManualDebitAdjustment",
            (int)DealerTransactionType.ManualCreditAdjustment => "Admin.Customers.Dealers.Transactions.Type.ManualCreditAdjustment",
            _ => string.Empty
        };
    }

    protected virtual string GetDealerTransactionDirectionResourceKey(int directionId)
    {
        return directionId switch
        {
            (int)DealerTransactionDirection.Debit => "Admin.Customers.Dealers.Transactions.Direction.Debit",
            (int)DealerTransactionDirection.Credit => "Admin.Customers.Dealers.Transactions.Direction.Credit",
            _ => string.Empty
        };
    }

    protected virtual async Task<string> GetDealerTransactionTypeTextAsync(int transactionTypeId)
    {
        var resourceKey = GetDealerTransactionTypeResourceKey(transactionTypeId);
        if (string.IsNullOrEmpty(resourceKey))
            return string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Type.Unknown"), transactionTypeId);

        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerTransactionDirectionTextAsync(int directionId)
    {
        var resourceKey = GetDealerTransactionDirectionResourceKey(directionId);
        if (string.IsNullOrEmpty(resourceKey))
            return string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Direction.Unknown"), directionId);

        return await _localizationService.GetResourceAsync(resourceKey);
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
                Text = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Type.ManualCreditAdjustment"),
                Value = ((int)DealerTransactionType.ManualCreditAdjustment).ToString()
            },
            new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Type.ManualDebitAdjustment"),
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
            model.Transactions = new List<DealerTransactionItemModel>();
            var transactions = await _dealerService.GetDealerTransactionsAsync(model.Id, DealerTransactionPreviewPageSize);
            foreach (var transaction in transactions)
            {
                model.Transactions.Add(new DealerTransactionItemModel
                {
                    OrderId = transaction.OrderId,
                    CustomerId = transaction.CustomerId,
                    TransactionType = await GetDealerTransactionTypeTextAsync(transaction.TransactionTypeId),
                    Direction = await GetDealerTransactionDirectionTextAsync(transaction.DirectionId),
                    Amount = transaction.Amount,
                    Note = transaction.Note,
                    CreatedOnUtc = transaction.CreatedOnUtc
                });
            }
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
            throw new InvalidOperationException(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Errors.CannotManageOutsideStore"));

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

    protected virtual async Task ValidateDealerFinancialInputsAsync(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.CreditLimit < 0 || model.CreditLimit > MaxDealerCreditLimit)
            ModelState.AddModelError(nameof(model.CreditLimit),
                string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.CreditLimitRange"), MaxDealerCreditLimit.ToString("0.####", CultureInfo.InvariantCulture)));

        if (decimal.Round(model.CreditLimit, 4) != model.CreditLimit)
            ModelState.AddModelError(nameof(model.CreditLimit),
                await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.CreditLimitScale"));
    }

    protected virtual async Task ValidateManualTransactionInputsAsync(DealerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var directionId = GetDirectionIdByManualTransactionType(model.ManualTransactionTypeId);
        if (directionId <= 0)
            ModelState.AddModelError(nameof(model.ManualTransactionTypeId),
                await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.ManualTransactionType"));

        if (model.ManualTransactionAmount <= 0 || model.ManualTransactionAmount > MaxDealerCreditLimit)
            ModelState.AddModelError(nameof(model.ManualTransactionAmount),
                string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.ManualTransactionAmountRange"), MaxDealerCreditLimit.ToString("0.####", CultureInfo.InvariantCulture)));

        if (decimal.Round(model.ManualTransactionAmount, 4) != model.ManualTransactionAmount)
            ModelState.AddModelError(nameof(model.ManualTransactionAmount),
                await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.ManualTransactionAmountScale"));
    }

    protected virtual DateTime? NormalizeDateFilterFrom(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
    }

    protected virtual DateTime? NormalizeDateFilterTo(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var endOfDay = value.Value.Date.AddDays(1).AddTicks(-1);
        return DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);
    }

    protected virtual string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var escapedValue = value.Replace("\"", "\"\"");
        return escapedValue.Contains(',') || escapedValue.Contains('"') || escapedValue.Contains('\n') || escapedValue.Contains('\r')
            ? $"\"{escapedValue}\""
            : escapedValue;
    }

    protected virtual async Task<DealerTransactionListModel> PrepareDealerTransactionListModelAsync(DealerTransactionListModel searchModel, int pageSize)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        searchModel ??= new DealerTransactionListModel();
        searchModel.IsStoreOwner = isStoreOwner;

        var stores = await _storeService.GetAllStoresAsync();
        if (!isStoreOwner)
        {
            searchModel.AvailableStores = new List<SelectListItem>
            {
                new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
            };

            foreach (var store in stores)
                searchModel.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
        }
        else
        {
            searchModel.SearchStoreId = managedStoreId;
            searchModel.AvailableStores = [];
        }

        var effectiveStoreId = isStoreOwner ? managedStoreId : searchModel.SearchStoreId;
        var filteredDealers = (await _dealerService.SearchDealersAsync(storeId: effectiveStoreId, pageSize: int.MaxValue))
            .OrderBy(dealer => dealer.Name)
            .ThenBy(dealer => dealer.Id)
            .ToList();

        searchModel.AvailableDealers = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };

        foreach (var dealer in filteredDealers)
        {
            searchModel.AvailableDealers.Add(new SelectListItem
            {
                Text = $"{dealer.Name} (#{dealer.Id})",
                Value = dealer.Id.ToString()
            });
        }

        if (searchModel.SearchDealerId > 0 && filteredDealers.All(dealer => dealer.Id != searchModel.SearchDealerId))
            searchModel.SearchDealerId = 0;

        var createdFromUtc = NormalizeDateFilterFrom(searchModel.SearchCreatedFromUtc);
        var createdToUtc = NormalizeDateFilterTo(searchModel.SearchCreatedToUtc);
        searchModel.IsStatementMode = searchModel.SearchDealerId > 0;
        searchModel.StatementDealerName = filteredDealers
            .FirstOrDefault(dealer => dealer.Id == searchModel.SearchDealerId)?.Name;

        var transactions = await _dealerService.SearchDealerTransactionsAsync(
            dealerId: searchModel.SearchDealerId,
            storeId: effectiveStoreId,
            createdFromUtc: createdFromUtc,
            createdToUtc: createdToUtc,
            pageSize: pageSize);

        var dealerById = filteredDealers.ToDictionary(dealer => dealer.Id);
        var storesById = stores.ToDictionary(store => store.Id);

        var unavailableText = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Common.NotAvailable");
        searchModel.Transactions = new List<DealerTransactionListItemModel>();
        var orderedTransactions = searchModel.IsStatementMode
            ? transactions.OrderBy(transaction => transaction.CreatedOnUtc).ThenBy(transaction => transaction.Id).ToList()
            : transactions.OrderByDescending(transaction => transaction.CreatedOnUtc).ThenByDescending(transaction => transaction.Id).ToList();

        var runningBalance = decimal.Zero;
        if (searchModel.IsStatementMode && createdFromUtc.HasValue)
        {
            var openingTransactions = await _dealerService.SearchDealerTransactionsAsync(
                dealerId: searchModel.SearchDealerId,
                storeId: effectiveStoreId,
                createdToUtc: createdFromUtc.Value.AddTicks(-1),
                pageSize: int.MaxValue);

            runningBalance = openingTransactions.Sum(transaction =>
                transaction.DirectionId == (int)DealerTransactionDirection.Debit
                    ? transaction.Amount
                    : -transaction.Amount);
        }

        searchModel.OpeningBalance = searchModel.IsStatementMode ? runningBalance : decimal.Zero;
        foreach (var transaction in orderedTransactions)
        {
            dealerById.TryGetValue(transaction.DealerId, out var dealer);
            var storeName = dealer is not null && storesById.TryGetValue(dealer.StoreId, out var store) ? store.Name : unavailableText;
            var isDebit = transaction.DirectionId == (int)DealerTransactionDirection.Debit;
            var debitAmount = isDebit ? transaction.Amount : decimal.Zero;
            var creditAmount = isDebit ? decimal.Zero : transaction.Amount;

            if (searchModel.IsStatementMode)
                runningBalance += isDebit ? transaction.Amount : -transaction.Amount;

            searchModel.Transactions.Add(new DealerTransactionListItemModel
            {
                Id = transaction.Id,
                DealerId = transaction.DealerId,
                DealerName = dealer?.Name ?? string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.DealerFallback"), transaction.DealerId),
                StoreId = dealer?.StoreId ?? 0,
                StoreName = storeName,
                OrderId = transaction.OrderId,
                CustomerId = transaction.CustomerId,
                TransactionType = await GetDealerTransactionTypeTextAsync(transaction.TransactionTypeId),
                Direction = await GetDealerTransactionDirectionTextAsync(transaction.DirectionId),
                Amount = transaction.Amount,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount,
                RunningBalance = searchModel.IsStatementMode ? runningBalance : null,
                Note = transaction.Note,
                CreatedOnUtc = transaction.CreatedOnUtc
            });
        }

        searchModel.TotalDebit = transactions
            .Where(transaction => transaction.DirectionId == (int)DealerTransactionDirection.Debit)
            .Sum(transaction => transaction.Amount);

        searchModel.TotalCredit = transactions
            .Where(transaction => transaction.DirectionId == (int)DealerTransactionDirection.Credit)
            .Sum(transaction => transaction.Amount);

        searchModel.NetBalance = searchModel.TotalDebit - searchModel.TotalCredit;
        searchModel.ClosingBalance = searchModel.IsStatementMode
            ? searchModel.OpeningBalance + searchModel.NetBalance
            : decimal.Zero;

        return searchModel;
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
                new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
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
                ? await _localizationService.GetResourceAsync("Admin.Customers.Dealers.PaymentMethods.AllActive")
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

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> Transactions(DealerTransactionListModel searchModel)
    {
        var model = await PrepareDealerTransactionListModelAsync(searchModel, 1000);
        return View(model);
    }

    [HttpPost, ActionName("Transactions")]
    [FormValueRequired("search-transactions")]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> TransactionsSearch(DealerTransactionListModel searchModel)
    {
        var model = await PrepareDealerTransactionListModelAsync(searchModel, 1000);
        return View(model);
    }

    [HttpPost, ActionName("Transactions")]
    [FormValueRequired("exportcsv-all")]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> ExportTransactionsCsv(DealerTransactionListModel searchModel)
    {
        var model = await PrepareDealerTransactionListModelAsync(searchModel, int.MaxValue);
        if (!model.Transactions.Any())
        {
            return RedirectToAction(nameof(Transactions), new
            {
                SearchStoreId = model.SearchStoreId,
                SearchDealerId = model.SearchDealerId,
                SearchCreatedFromUtc = model.SearchCreatedFromUtc,
                SearchCreatedToUtc = model.SearchCreatedToUtc
            });
        }

        var builder = new StringBuilder();
        builder.AppendLine("Id,CreatedOnUtc,DealerId,DealerName,StoreName,Type,Direction,DebitAmount,CreditAmount,RunningBalance,OrderId,CustomerId,Note");

        foreach (var item in model.Transactions)
        {
            builder.Append(item.Id).Append(',');
            builder.Append(EscapeCsv(item.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))).Append(',');
            builder.Append(item.DealerId).Append(',');
            builder.Append(EscapeCsv(item.DealerName)).Append(',');
            builder.Append(EscapeCsv(item.StoreName)).Append(',');
            builder.Append(EscapeCsv(item.TransactionType)).Append(',');
            builder.Append(EscapeCsv(item.Direction)).Append(',');
            builder.Append(item.DebitAmount.ToString("0.####", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(item.CreditAmount.ToString("0.####", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(item.RunningBalance?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(item.OrderId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(item.CustomerId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(EscapeCsv(item.Note));
            builder.AppendLine();
        }

        var fileName = $"dealer_transactions_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{CommonHelper.GenerateRandomDigitCode(4)}.csv";
        return File(Encoding.UTF8.GetBytes(builder.ToString()), MimeTypes.TextCsv, fileName);
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

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store is null)
        {
            ModelState.AddModelError(nameof(model.StoreId), await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.StoreRequired"));
        }
        await ValidateDealerFinancialInputsAsync(model);

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

        ModelState.Remove(nameof(DealerModel.CreditLimit));

        var directionId = GetDirectionIdByManualTransactionType(model.ManualTransactionTypeId);
        await ValidateManualTransactionInputsAsync(model);

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
            ModelState.AddModelError(nameof(model.StoreId), await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.StoreRequired"));
        await ValidateDealerFinancialInputsAsync(model);

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
