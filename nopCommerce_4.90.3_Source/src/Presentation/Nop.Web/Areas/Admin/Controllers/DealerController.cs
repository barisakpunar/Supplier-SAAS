using System.Globalization;
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

    protected virtual async Task<string> GetDealerTransactionSourceTextAsync(DealerTransaction transaction, DealerCollection sourceCollection = null)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var sourceType = transaction.SourceType;
        var sourceId = transaction.SourceId ?? sourceCollection?.Id ?? transaction.OrderId;

        return sourceType switch
        {
            DealerTransactionSourceType.Order when !string.IsNullOrWhiteSpace(transaction.ReferenceNo)
                => string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Source.OrderByReference"), transaction.ReferenceNo),
            DealerTransactionSourceType.Order when sourceId.HasValue
                => string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Source.Order"), sourceId.Value),
            DealerTransactionSourceType.Collection when sourceId.HasValue
                => string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Source.Collection"), sourceId.Value),
            DealerTransactionSourceType.ManualAdjustment
                => await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Source.ManualAdjustment"),
            _ when sourceCollection is not null
                => string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.Source.Collection"), sourceCollection.Id),
            _ => string.Empty
        };
    }

    protected virtual string GetDealerTransactionSourceUrl(DealerTransaction transaction, DealerCollection sourceCollection = null)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var sourceType = transaction.SourceType;
        var sourceId = transaction.SourceId ?? sourceCollection?.Id ?? transaction.OrderId;

        return sourceType switch
        {
            DealerTransactionSourceType.Order when sourceId.HasValue
                => Url.Action("Edit", "Order", new { id = sourceId.Value }),
            DealerTransactionSourceType.Collection when sourceId.HasValue
                => Url.Action(nameof(CollectionDetails), "Dealer", new { id = sourceId.Value }),
            _ when sourceCollection is not null
                => Url.Action(nameof(CollectionDetails), "Dealer", new { id = sourceCollection.Id }),
            _ => string.Empty
        };
    }

    protected virtual async Task<string> GetDealerCollectionMethodTextAsync(int collectionMethodId)
    {
        var resourceKey = $"Admin.Customers.DealerCollections.Method.{((DealerCollectionMethod)collectionMethodId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerCollectionStatusTextAsync(int collectionStatusId)
    {
        var resourceKey = $"Admin.Customers.DealerCollections.Status.{((DealerCollectionStatus)collectionStatusId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerFinancialInstrumentTypeTextAsync(int instrumentTypeId)
    {
        var resourceKey = $"Admin.Customers.DealerFinancialInstruments.Type.{((DealerFinancialInstrumentType)instrumentTypeId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerFinancialInstrumentStatusTextAsync(int instrumentStatusId)
    {
        var resourceKey = $"Admin.Customers.DealerFinancialInstruments.Status.{((DealerFinancialInstrumentStatus)instrumentStatusId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerFinanceAuditEntityTypeTextAsync(int entityTypeId)
    {
        var resourceKey = $"Admin.Customers.DealerFinanceAudit.EntityType.{((DealerFinanceAuditEntityType)entityTypeId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual async Task<string> GetDealerFinanceAuditActionTypeTextAsync(int actionTypeId)
    {
        var resourceKey = $"Admin.Customers.DealerFinanceAudit.ActionType.{((DealerFinanceAuditActionType)actionTypeId)}";
        return await _localizationService.GetResourceAsync(resourceKey);
    }

    protected virtual bool RequiresFinancialInstrument(int collectionMethodId)
    {
        return collectionMethodId == (int)DealerCollectionMethod.Check
               || collectionMethodId == (int)DealerCollectionMethod.PromissoryNote;
    }

    protected virtual bool CanSetFinancialInstrumentStatus(int currentStatusId, DealerFinancialInstrumentStatus nextStatus)
    {
        if (!Enum.IsDefined(typeof(DealerFinancialInstrumentStatus), currentStatusId))
            return false;

        return currentStatusId != (int)DealerFinancialInstrumentStatus.Cancelled
               && currentStatusId != (int)nextStatus;
    }

    protected virtual async Task<IList<DealerFinanceAuditLogItemModel>> PrepareDealerFinanceAuditLogItemsAsync(IList<DealerFinanceAuditLog> logs)
    {
        if (logs is null || !logs.Any())
            return new List<DealerFinanceAuditLogItemModel>();

        var customerIds = logs
            .Where(item => item.PerformedByCustomerId.HasValue)
            .Select(item => item.PerformedByCustomerId!.Value)
            .Distinct()
            .ToArray();

        var customersById = customerIds.Any()
            ? (await _customerService.GetCustomersByIdsAsync(customerIds)).ToDictionary(customer => customer.Id)
            : new Dictionary<int, Customer>();

        var result = new List<DealerFinanceAuditLogItemModel>();
        foreach (var log in logs.OrderByDescending(item => item.PerformedOnUtc).ThenByDescending(item => item.Id))
        {
            customersById.TryGetValue(log.PerformedByCustomerId ?? 0, out var performedByCustomer);

            var statusTransition = string.Empty;
            if (log.StatusBeforeId.HasValue || log.StatusAfterId.HasValue)
            {
                var beforeText = log.StatusBeforeId.HasValue
                    ? await GetDealerFinancialInstrumentStatusTextAsync(log.StatusBeforeId.Value)
                    : "-";
                var afterText = log.StatusAfterId.HasValue
                    ? await GetDealerFinancialInstrumentStatusTextAsync(log.StatusAfterId.Value)
                    : "-";
                statusTransition = $"{beforeText} -> {afterText}";
            }

            result.Add(new DealerFinanceAuditLogItemModel
            {
                Id = log.Id,
                EntityType = await GetDealerFinanceAuditEntityTypeTextAsync(log.EntityTypeId),
                ActionType = await GetDealerFinanceAuditActionTypeTextAsync(log.ActionTypeId),
                StatusTransition = statusTransition,
                PerformedByCustomerName = GetCustomerDisplayText(performedByCustomer),
                PerformedOnUtc = log.PerformedOnUtc,
                Note = log.Note
            });
        }

        return result;
    }

    protected virtual string GetCustomerDisplayText(Customer customer)
    {
        if (customer is null)
            return string.Empty;

        return !string.IsNullOrWhiteSpace(customer.Email) ? customer.Email : customer.Username;
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

    protected virtual async Task PrepareDealerCollectionModelAsync(DealerCollectionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
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

        var dealers = await _dealerService.SearchDealersAsync(storeId: model.StoreId, active: true, pageSize: int.MaxValue);
        model.AvailableDealers = dealers
            .OrderBy(dealer => dealer.Name)
            .ThenBy(dealer => dealer.Id)
            .Select(dealer => new SelectListItem
            {
                Value = dealer.Id.ToString(),
                Text = $"{dealer.Name} (#{dealer.Id})"
            })
            .ToList();

        if (model.DealerId > 0 && dealers.All(dealer => dealer.Id != model.DealerId))
            model.DealerId = 0;

        if (model.DealerId <= 0 && dealers.Any())
            model.DealerId = dealers.First().Id;

        model.AvailableCustomers = new List<SelectListItem>
        {
            new() { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.None") }
        };

        if (model.DealerId > 0)
        {
            var customerIds = await _dealerService.GetCustomerIdsByDealerIdAsync(model.DealerId);
            if (customerIds.Any())
            {
                var customers = await _customerService.GetCustomersByIdsAsync(customerIds.ToArray());
                foreach (var customer in customers.OrderBy(customer => customer.Email).ThenBy(customer => customer.Id))
                {
                    var identity = !string.IsNullOrWhiteSpace(customer.Email) ? customer.Email : customer.Username;
                    model.AvailableCustomers.Add(new SelectListItem
                    {
                        Value = customer.Id.ToString(),
                        Text = $"{identity} (#{customer.Id})"
                    });
                }
            }
        }

        if (model.CustomerId > 0 && model.AvailableCustomers.All(customer => customer.Value != model.CustomerId.ToString()))
            model.CustomerId = 0;

        model.AvailableCollectionMethods = new List<SelectListItem>();
        foreach (var method in Enum.GetValues<DealerCollectionMethod>())
        {
            model.AvailableCollectionMethods.Add(new SelectListItem
            {
                Value = ((int)method).ToString(),
                Text = await GetDealerCollectionMethodTextAsync((int)method)
            });
        }

        if (model.CollectionMethodId <= 0)
            model.CollectionMethodId = (int)DealerCollectionMethod.BankTransfer;

        if (model.CollectionDateUtc == default)
            model.CollectionDateUtc = DateTime.UtcNow;
    }

    protected virtual async Task<DealerCollectionListModel> PrepareDealerCollectionListModelAsync(DealerCollectionListModel searchModel)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        searchModel ??= new DealerCollectionListModel();
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
        var dealers = await _dealerService.SearchDealersAsync(storeId: effectiveStoreId, pageSize: int.MaxValue);
        searchModel.AvailableDealers = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };

        foreach (var dealer in dealers.OrderBy(item => item.Name).ThenBy(item => item.Id))
        {
            searchModel.AvailableDealers.Add(new SelectListItem
            {
                Value = dealer.Id.ToString(),
                Text = $"{dealer.Name} (#{dealer.Id})"
            });
        }

        searchModel.AvailableCollectionMethods = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };
        foreach (var method in Enum.GetValues<DealerCollectionMethod>())
        {
            searchModel.AvailableCollectionMethods.Add(new SelectListItem
            {
                Value = ((int)method).ToString(),
                Text = await GetDealerCollectionMethodTextAsync((int)method)
            });
        }

        searchModel.AvailableCollectionStatuses = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };
        foreach (var status in Enum.GetValues<DealerCollectionStatus>())
        {
            searchModel.AvailableCollectionStatuses.Add(new SelectListItem
            {
                Value = ((int)status).ToString(),
                Text = await GetDealerCollectionStatusTextAsync((int)status)
            });
        }

        var collectionFromUtc = NormalizeDateFilterFrom(searchModel.SearchCollectionDateFromUtc);
        var collectionToUtc = NormalizeDateFilterTo(searchModel.SearchCollectionDateToUtc);
        var collections = await _dealerService.SearchDealerCollectionsAsync(
            dealerId: searchModel.SearchDealerId,
            storeId: effectiveStoreId,
            collectionMethodId: searchModel.SearchCollectionMethodId,
            collectionStatusId: searchModel.SearchCollectionStatusId,
            collectionFromUtc: collectionFromUtc,
            collectionToUtc: collectionToUtc,
            pageSize: int.MaxValue);

        var storesById = stores.ToDictionary(store => store.Id);
        var dealersById = dealers.ToDictionary(dealer => dealer.Id);
        var allCustomerIds = collections.Where(item => item.CustomerId.HasValue).Select(item => item.CustomerId!.Value).Distinct().ToArray();
        var customersById = allCustomerIds.Any()
            ? (await _customerService.GetCustomersByIdsAsync(allCustomerIds)).ToDictionary(customer => customer.Id)
            : new Dictionary<int, Customer>();

        var unavailableText = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Common.NotAvailable");
        searchModel.Collections = new List<DealerCollectionListItemModel>();
        foreach (var collection in collections)
        {
            dealersById.TryGetValue(collection.DealerId, out var dealer);
            customersById.TryGetValue(collection.CustomerId ?? 0, out var customer);

            searchModel.Collections.Add(new DealerCollectionListItemModel
            {
                Id = collection.Id,
                DealerId = collection.DealerId,
                DealerName = dealer?.Name ?? string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.DealerFallback"), collection.DealerId),
                StoreId = dealer?.StoreId ?? 0,
                StoreName = dealer is not null && storesById.TryGetValue(dealer.StoreId, out var store) ? store.Name : unavailableText,
                CustomerId = collection.CustomerId,
                CustomerName = customer is null ? string.Empty : (!string.IsNullOrWhiteSpace(customer.Email) ? customer.Email : customer.Username),
                CollectionMethod = await GetDealerCollectionMethodTextAsync(collection.CollectionMethodId),
                CollectionStatus = await GetDealerCollectionStatusTextAsync(collection.CollectionStatusId),
                Amount = collection.Amount,
                CollectionDateUtc = collection.CollectionDateUtc,
                ReferenceNo = collection.ReferenceNo,
                DocumentNo = collection.DocumentNo,
                DueDateUtc = collection.DueDateUtc,
                Note = collection.Note
            });
        }

        return searchModel;
    }

    protected virtual async Task<DealerCollectionDetailsModel> PrepareDealerCollectionDetailsModelAsync(DealerCollection collection, DealerInfo dealer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(dealer);

        var store = await _storeService.GetStoreByIdAsync(dealer.StoreId);
        var createdByCustomer = await _customerService.GetCustomerByIdAsync(collection.CreatedByCustomerId);
        var mappedCustomer = collection.CustomerId.HasValue ? await _customerService.GetCustomerByIdAsync(collection.CustomerId.Value) : null;
        var cancelledByCustomer = collection.CancelledByCustomerId.HasValue ? await _customerService.GetCustomerByIdAsync(collection.CancelledByCustomerId.Value) : null;
        var originalTransaction = collection.DealerTransactionId.HasValue
            ? await _dealerService.GetDealerTransactionByIdAsync(collection.DealerTransactionId.Value)
            : null;
        var financialInstrument = collection.DealerFinancialInstrumentId.HasValue
            ? await _dealerService.GetDealerFinancialInstrumentByIdAsync(collection.DealerFinancialInstrumentId.Value)
            : null;
        var cancelledTransaction = collection.CancelledDealerTransactionId.HasValue
            ? await _dealerService.GetDealerTransactionByIdAsync(collection.CancelledDealerTransactionId.Value)
            : null;
        var auditTrail = await PrepareDealerFinanceAuditLogItemsAsync(await _dealerService.SearchDealerFinanceAuditLogsAsync(
            dealerId: dealer.Id,
            dealerCollectionId: collection.Id,
            pageSize: int.MaxValue));

        return new DealerCollectionDetailsModel
        {
            Id = collection.Id,
            DealerId = dealer.Id,
            DealerName = dealer.Name,
            StoreId = dealer.StoreId,
            StoreName = store?.Name ?? "-",
            CustomerId = collection.CustomerId,
            CustomerName = GetCustomerDisplayText(mappedCustomer),
            CollectionMethod = await GetDealerCollectionMethodTextAsync(collection.CollectionMethodId),
            CollectionStatus = await GetDealerCollectionStatusTextAsync(collection.CollectionStatusId),
            Amount = collection.Amount,
            CollectionDateUtc = collection.CollectionDateUtc,
            ReferenceNo = collection.ReferenceNo,
            DocumentNo = collection.DocumentNo,
            IssueDateUtc = collection.IssueDateUtc,
            DueDateUtc = collection.DueDateUtc,
            Note = collection.Note,
            CreatedByCustomerName = GetCustomerDisplayText(createdByCustomer),
            CreatedOnUtc = collection.CreatedOnUtc,
            UpdatedOnUtc = collection.UpdatedOnUtc,
            DealerTransactionId = collection.DealerTransactionId,
            DealerTransactionCreatedOnUtc = originalTransaction?.CreatedOnUtc,
            DealerFinancialInstrumentId = collection.DealerFinancialInstrumentId,
            DealerFinancialInstrumentType = financialInstrument is null ? string.Empty : await GetDealerFinancialInstrumentTypeTextAsync(financialInstrument.InstrumentTypeId),
            DealerFinancialInstrumentStatus = financialInstrument is null ? string.Empty : await GetDealerFinancialInstrumentStatusTextAsync(financialInstrument.InstrumentStatusId),
            CancelledDealerTransactionId = collection.CancelledDealerTransactionId,
            CancelledDealerTransactionCreatedOnUtc = cancelledTransaction?.CreatedOnUtc,
            CancelledByCustomerName = GetCustomerDisplayText(cancelledByCustomer),
            CancelledOnUtc = collection.CancelledOnUtc,
            CanCancel = collection.CollectionStatusId == (int)DealerCollectionStatus.Posted,
            AuditTrail = auditTrail
        };
    }

    protected virtual async Task<DealerFinancialInstrumentDetailsModel> PrepareDealerFinancialInstrumentDetailsModelAsync(DealerFinancialInstrument instrument, DealerInfo dealer)
    {
        ArgumentNullException.ThrowIfNull(instrument);
        ArgumentNullException.ThrowIfNull(dealer);

        var store = await _storeService.GetStoreByIdAsync(dealer.StoreId);
        var mappedCustomer = instrument.CustomerId.HasValue ? await _customerService.GetCustomerByIdAsync(instrument.CustomerId.Value) : null;
        var createdByCustomer = await _customerService.GetCustomerByIdAsync(instrument.CreatedByCustomerId);
        var auditTrail = await PrepareDealerFinanceAuditLogItemsAsync(await _dealerService.SearchDealerFinanceAuditLogsAsync(
            dealerId: dealer.Id,
            dealerFinancialInstrumentId: instrument.Id,
            pageSize: int.MaxValue));

        return new DealerFinancialInstrumentDetailsModel
        {
            Id = instrument.Id,
            StoreId = dealer.StoreId,
            StoreName = store?.Name ?? "-",
            DealerId = dealer.Id,
            DealerName = dealer.Name,
            DealerCollectionId = instrument.DealerCollectionId,
            CustomerId = instrument.CustomerId,
            CustomerName = GetCustomerDisplayText(mappedCustomer),
            InstrumentType = await GetDealerFinancialInstrumentTypeTextAsync(instrument.InstrumentTypeId),
            InstrumentStatus = await GetDealerFinancialInstrumentStatusTextAsync(instrument.InstrumentStatusId),
            Amount = instrument.Amount,
            InstrumentNo = instrument.InstrumentNo,
            IssueDateUtc = instrument.IssueDateUtc,
            DueDateUtc = instrument.DueDateUtc,
            BankName = instrument.BankName,
            BranchName = instrument.BranchName,
            AccountNo = instrument.AccountNo,
            DrawerName = instrument.DrawerName,
            Note = instrument.Note,
            CreatedByCustomerName = GetCustomerDisplayText(createdByCustomer),
            CreatedOnUtc = instrument.CreatedOnUtc,
            UpdatedOnUtc = instrument.UpdatedOnUtc,
            CanMarkCollected = CanSetFinancialInstrumentStatus(instrument.InstrumentStatusId, DealerFinancialInstrumentStatus.Collected),
            CanMarkReturned = CanSetFinancialInstrumentStatus(instrument.InstrumentStatusId, DealerFinancialInstrumentStatus.Returned),
            CanMarkProtested = CanSetFinancialInstrumentStatus(instrument.InstrumentStatusId, DealerFinancialInstrumentStatus.Protested),
            AuditTrail = auditTrail
        };
    }

    protected virtual async Task<DealerFinancialInstrumentListModel> PrepareDealerFinancialInstrumentListModelAsync(DealerFinancialInstrumentListModel searchModel, int pageSize)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        searchModel ??= new DealerFinancialInstrumentListModel();
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
        var dealers = await _dealerService.SearchDealersAsync(storeId: effectiveStoreId, pageSize: int.MaxValue);
        searchModel.AvailableDealers = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };

        foreach (var dealer in dealers.OrderBy(item => item.Name).ThenBy(item => item.Id))
        {
            searchModel.AvailableDealers.Add(new SelectListItem
            {
                Value = dealer.Id.ToString(),
                Text = $"{dealer.Name} (#{dealer.Id})"
            });
        }

        searchModel.AvailableInstrumentTypes = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };
        foreach (var type in Enum.GetValues<DealerFinancialInstrumentType>())
        {
            searchModel.AvailableInstrumentTypes.Add(new SelectListItem
            {
                Value = ((int)type).ToString(),
                Text = await GetDealerFinancialInstrumentTypeTextAsync((int)type)
            });
        }

        searchModel.AvailableInstrumentStatuses = new List<SelectListItem>
        {
            new() { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" }
        };
        foreach (var status in Enum.GetValues<DealerFinancialInstrumentStatus>())
        {
            searchModel.AvailableInstrumentStatuses.Add(new SelectListItem
            {
                Value = ((int)status).ToString(),
                Text = await GetDealerFinancialInstrumentStatusTextAsync((int)status)
            });
        }

        var dueFromUtc = NormalizeDateFilterFrom(searchModel.SearchDueDateFromUtc);
        var dueToUtc = NormalizeDateFilterTo(searchModel.SearchDueDateToUtc);
        var instruments = await _dealerService.SearchDealerFinancialInstrumentsAsync(
            dealerId: searchModel.SearchDealerId,
            storeId: effectiveStoreId,
            instrumentTypeId: searchModel.SearchInstrumentTypeId,
            instrumentStatusId: searchModel.SearchInstrumentStatusId,
            dueFromUtc: dueFromUtc,
            dueToUtc: dueToUtc,
            pageSize: pageSize);

        var storesById = stores.ToDictionary(store => store.Id);
        var dealersById = dealers.ToDictionary(dealer => dealer.Id);
        var customerIds = instruments.Where(item => item.CustomerId.HasValue).Select(item => item.CustomerId!.Value).Distinct().ToArray();
        var customersById = customerIds.Any()
            ? (await _customerService.GetCustomersByIdsAsync(customerIds)).ToDictionary(customer => customer.Id)
            : new Dictionary<int, Customer>();

        var unavailableText = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Common.NotAvailable");
        searchModel.Instruments = new List<DealerFinancialInstrumentListItemModel>();
        foreach (var instrument in instruments)
        {
            dealersById.TryGetValue(instrument.DealerId, out var dealer);
            customersById.TryGetValue(instrument.CustomerId ?? 0, out var customer);

            searchModel.Instruments.Add(new DealerFinancialInstrumentListItemModel
            {
                Id = instrument.Id,
                DealerId = instrument.DealerId,
                DealerName = dealer?.Name ?? string.Format(await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions.DealerFallback"), instrument.DealerId),
                StoreId = dealer?.StoreId ?? 0,
                StoreName = dealer is not null && storesById.TryGetValue(dealer.StoreId, out var store) ? store.Name : unavailableText,
                CustomerId = instrument.CustomerId,
                CustomerName = GetCustomerDisplayText(customer),
                InstrumentType = await GetDealerFinancialInstrumentTypeTextAsync(instrument.InstrumentTypeId),
                InstrumentStatus = await GetDealerFinancialInstrumentStatusTextAsync(instrument.InstrumentStatusId),
                Amount = instrument.Amount,
                InstrumentNo = instrument.InstrumentNo,
                IssueDateUtc = instrument.IssueDateUtc,
                DueDateUtc = instrument.DueDateUtc,
                DealerCollectionId = instrument.DealerCollectionId
            });
        }

        return searchModel;
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

    protected virtual async Task ValidateDealerCollectionInputsAsync(DealerCollectionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.DealerId <= 0)
            ModelState.AddModelError(nameof(model.DealerId), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.DealerRequired"));

        if (model.CollectionMethodId <= 0 || !Enum.IsDefined(typeof(DealerCollectionMethod), model.CollectionMethodId))
            ModelState.AddModelError(nameof(model.CollectionMethodId), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.MethodRequired"));

        if (model.Amount <= 0 || model.Amount > MaxDealerCreditLimit)
            ModelState.AddModelError(nameof(model.Amount), string.Format(await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.AmountRange"), MaxDealerCreditLimit.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)));

        if (decimal.Round(model.Amount, 4) != model.Amount)
            ModelState.AddModelError(nameof(model.Amount), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.AmountScale"));

        var issueDateUtc = NormalizeDateValue(model.IssueDateUtc);
        var dueDateUtc = NormalizeDateValue(model.DueDateUtc);
        if (issueDateUtc.HasValue && dueDateUtc.HasValue && dueDateUtc.Value < issueDateUtc.Value)
            ModelState.AddModelError(nameof(model.DueDateUtc), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.DueDateBeforeIssueDate"));
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

    protected virtual DateTime? NormalizeDateValue(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
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
        var collectionsByTransactionId = new Dictionary<int, DealerCollection>();
        var instrumentsById = new Dictionary<int, DealerFinancialInstrument>();
        if (transactions.Any())
        {
            var collections = await _dealerService.SearchDealerCollectionsAsync(
                dealerId: searchModel.SearchDealerId,
                storeId: effectiveStoreId,
                pageSize: int.MaxValue);

            foreach (var collection in collections)
            {
                if (collection.DealerTransactionId.HasValue && !collectionsByTransactionId.ContainsKey(collection.DealerTransactionId.Value))
                    collectionsByTransactionId[collection.DealerTransactionId.Value] = collection;

                if (collection.CancelledDealerTransactionId.HasValue && !collectionsByTransactionId.ContainsKey(collection.CancelledDealerTransactionId.Value))
                    collectionsByTransactionId[collection.CancelledDealerTransactionId.Value] = collection;
            }

            var instruments = await _dealerService.SearchDealerFinancialInstrumentsAsync(
                dealerId: searchModel.SearchDealerId,
                storeId: effectiveStoreId,
                pageSize: int.MaxValue);
            instrumentsById = instruments.ToDictionary(instrument => instrument.Id);
        }

        var dealerById = filteredDealers.ToDictionary(dealer => dealer.Id);
        var storesById = stores.ToDictionary(store => store.Id);
        var customerIds = transactions
            .Where(transaction => transaction.CustomerId.HasValue)
            .Select(transaction => transaction.CustomerId!.Value)
            .Distinct()
            .ToArray();
        var customersById = customerIds.Any()
            ? (await _customerService.GetCustomersByIdsAsync(customerIds)).ToDictionary(customer => customer.Id)
            : new Dictionary<int, Customer>();

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
            collectionsByTransactionId.TryGetValue(transaction.Id, out var sourceCollection);
            customersById.TryGetValue(transaction.CustomerId ?? 0, out var customer);
            var linkedInstrument = sourceCollection?.DealerFinancialInstrumentId is int instrumentId && instrumentsById.TryGetValue(instrumentId, out var instrument)
                ? instrument
                : null;
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
                CustomerName = GetCustomerDisplayText(customer),
                TransactionType = await GetDealerTransactionTypeTextAsync(transaction.TransactionTypeId),
                Direction = await GetDealerTransactionDirectionTextAsync(transaction.DirectionId),
                Amount = transaction.Amount,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount,
                RunningBalance = searchModel.IsStatementMode ? runningBalance : null,
                SourceText = await GetDealerTransactionSourceTextAsync(transaction, sourceCollection),
                SourceUrl = GetDealerTransactionSourceUrl(transaction, sourceCollection),
                ReferenceNo = sourceCollection?.ReferenceNo ?? transaction.ReferenceNo,
                DocumentNo = sourceCollection?.DocumentNo,
                DueDateUtc = sourceCollection?.DueDateUtc,
                FinancialInstrumentText = linkedInstrument is null
                    ? string.Empty
                    : (!string.IsNullOrWhiteSpace(linkedInstrument.InstrumentNo)
                        ? linkedInstrument.InstrumentNo
                        : string.Format(await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Fields.FinancialInstrument.Link"), linkedInstrument.Id)),
                FinancialInstrumentUrl = linkedInstrument is null
                    ? string.Empty
                    : Url.Action(nameof(FinancialInstrumentDetails), "Dealer", new { id = linkedInstrument.Id }),
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

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> Collections(DealerCollectionListModel searchModel)
    {
        var model = await PrepareDealerCollectionListModelAsync(searchModel);
        return View(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> FinancialInstruments(DealerFinancialInstrumentListModel searchModel)
    {
        var model = await PrepareDealerFinancialInstrumentListModelAsync(searchModel, 1000);
        return View(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> CollectionDetails(int id)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        var collection = await _dealerService.GetDealerCollectionByIdAsync(id);
        if (collection is null)
            return RedirectToAction(nameof(Collections));

        var dealer = await _dealerService.GetDealerByIdAsync(collection.DealerId);
        if (dealer is null)
            return RedirectToAction(nameof(Collections));

        if (isStoreOwner && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        var model = await PrepareDealerCollectionDetailsModelAsync(collection, dealer);
        return View(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> FinancialInstrumentDetails(int id)
    {
        var (_, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        var instrument = await _dealerService.GetDealerFinancialInstrumentByIdAsync(id);
        if (instrument is null)
            return RedirectToAction(nameof(FinancialInstruments));

        var dealer = await _dealerService.GetDealerByIdAsync(instrument.DealerId);
        if (dealer is null)
            return RedirectToAction(nameof(FinancialInstruments));

        if (isStoreOwner && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        var model = await PrepareDealerFinancialInstrumentDetailsModelAsync(instrument, dealer);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> UpdateFinancialInstrumentStatus(int id, int statusId)
    {
        var (currentCustomer, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        var instrument = await _dealerService.GetDealerFinancialInstrumentByIdAsync(id);
        if (instrument is null)
            return RedirectToAction(nameof(FinancialInstruments));

        var dealer = await _dealerService.GetDealerByIdAsync(instrument.DealerId);
        if (dealer is null)
            return RedirectToAction(nameof(FinancialInstruments));

        if (isStoreOwner && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        if (!Enum.IsDefined(typeof(DealerFinancialInstrumentStatus), statusId))
            return RedirectToAction(nameof(FinancialInstrumentDetails), new { id = instrument.Id });

        if (!CanSetFinancialInstrumentStatus(instrument.InstrumentStatusId, (DealerFinancialInstrumentStatus)statusId))
            return RedirectToAction(nameof(FinancialInstrumentDetails), new { id = instrument.Id });

        var previousStatusId = instrument.InstrumentStatusId;
        instrument.InstrumentStatusId = statusId;
        instrument.UpdatedOnUtc = DateTime.UtcNow;
        await _dealerService.UpdateDealerFinancialInstrumentAsync(instrument);
        await _dealerService.InsertDealerFinanceAuditLogAsync(new DealerFinanceAuditLog
        {
            DealerId = instrument.DealerId,
            EntityTypeId = (int)DealerFinanceAuditEntityType.FinancialInstrument,
            DealerCollectionId = instrument.DealerCollectionId,
            DealerFinancialInstrumentId = instrument.Id,
            ActionTypeId = (int)DealerFinanceAuditActionType.FinancialInstrumentStatusChanged,
            StatusBeforeId = previousStatusId,
            StatusAfterId = instrument.InstrumentStatusId,
            PerformedByCustomerId = currentCustomer.Id,
            PerformedOnUtc = DateTime.UtcNow
        });

        return RedirectToAction(nameof(FinancialInstrumentDetails), new { id = instrument.Id });
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
        builder.AppendLine("Id,CreatedOnUtc,DealerId,DealerName,StoreName,Type,Source,ReferenceNo,DocumentNo,DueDate,Instrument,Direction,DebitAmount,CreditAmount,RunningBalance,OrderId,CustomerId,CustomerName,Note");

        foreach (var item in model.Transactions)
        {
            builder.Append(item.Id).Append(',');
            builder.Append(EscapeCsv(item.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))).Append(',');
            builder.Append(item.DealerId).Append(',');
            builder.Append(EscapeCsv(item.DealerName)).Append(',');
            builder.Append(EscapeCsv(item.StoreName)).Append(',');
            builder.Append(EscapeCsv(item.TransactionType)).Append(',');
            builder.Append(EscapeCsv(item.SourceText)).Append(',');
            builder.Append(EscapeCsv(item.ReferenceNo)).Append(',');
            builder.Append(EscapeCsv(item.DocumentNo)).Append(',');
            builder.Append(item.DueDateUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(EscapeCsv(item.FinancialInstrumentText)).Append(',');
            builder.Append(EscapeCsv(item.Direction)).Append(',');
            builder.Append(item.DebitAmount.ToString("0.####", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(item.CreditAmount.ToString("0.####", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(item.RunningBalance?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(item.OrderId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(item.CustomerId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            builder.Append(EscapeCsv(item.CustomerName)).Append(',');
            builder.Append(EscapeCsv(item.Note));
            builder.AppendLine();
        }

        var fileName = $"dealer_transactions_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{CommonHelper.GenerateRandomDigitCode(4)}.csv";
        return File(Encoding.UTF8.GetBytes(builder.ToString()), MimeTypes.TextCsv, fileName);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CreateCollection(int storeId = 0, int dealerId = 0, int customerId = 0)
    {
        var model = new DealerCollectionModel
        {
            StoreId = storeId,
            DealerId = dealerId,
            CustomerId = customerId
        };
        await PrepareDealerCollectionModelAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CreateCollection(DealerCollectionModel model)
    {
        var (currentCustomer, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();

        if (isStoreOwner)
            model.StoreId = managedStoreId;

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store is null)
            ModelState.AddModelError(nameof(model.StoreId), await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Validation.StoreRequired"));

        var dealer = await _dealerService.GetDealerByIdAsync(model.DealerId);
        if (dealer is null || !dealer.Active || (model.StoreId > 0 && dealer.StoreId != model.StoreId))
            ModelState.AddModelError(nameof(model.DealerId), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.DealerRequired"));

        if (isStoreOwner && dealer is not null && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        if (model.CustomerId > 0)
        {
            var isMapped = await _dealerService.IsCustomerMappedToDealerAsync(model.DealerId, model.CustomerId);
            if (!isMapped)
                ModelState.AddModelError(nameof(model.CustomerId), await _localizationService.GetResourceAsync("Admin.Customers.DealerCollections.Validation.CustomerInvalid"));
        }

        await ValidateDealerCollectionInputsAsync(model);

        if (!ModelState.IsValid)
        {
            await PrepareDealerCollectionModelAsync(model);
            return View(model);
        }

        var transaction = new DealerTransaction
        {
            DealerId = model.DealerId,
            CustomerId = model.CustomerId > 0 ? model.CustomerId : null,
            SourceTypeId = (int)DealerTransactionSourceType.Collection,
            ReferenceNo = string.IsNullOrWhiteSpace(model.ReferenceNo) ? null : model.ReferenceNo.Trim(),
            TransactionTypeId = (int)DealerTransactionType.OpenAccountCollection,
            DirectionId = (int)DealerTransactionDirection.Credit,
            Amount = model.Amount,
            Note = string.IsNullOrWhiteSpace(model.ReferenceNo)
                ? $"Manual collection entry. {model.Note}".Trim()
                : $"Manual collection entry. Ref: {model.ReferenceNo}. {model.Note}".Trim(),
            CreatedOnUtc = model.CollectionDateUtc
        };

        await _dealerService.InsertDealerTransactionAsync(transaction);

        var collection = new DealerCollection
        {
            DealerId = model.DealerId,
            CustomerId = model.CustomerId > 0 ? model.CustomerId : null,
            DealerTransactionId = transaction.Id,
            CollectionMethodId = model.CollectionMethodId,
            CollectionStatusId = (int)DealerCollectionStatus.Posted,
            Amount = model.Amount,
            CollectionDateUtc = model.CollectionDateUtc,
            ReferenceNo = string.IsNullOrWhiteSpace(model.ReferenceNo) ? null : model.ReferenceNo.Trim(),
            DocumentNo = string.IsNullOrWhiteSpace(model.DocumentNo) ? null : model.DocumentNo.Trim(),
            IssueDateUtc = NormalizeDateValue(model.IssueDateUtc),
            DueDateUtc = NormalizeDateValue(model.DueDateUtc),
            Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            CreatedByCustomerId = currentCustomer.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _dealerService.InsertDealerCollectionAsync(collection);
        await _dealerService.InsertDealerFinanceAuditLogAsync(new DealerFinanceAuditLog
        {
            DealerId = collection.DealerId,
            EntityTypeId = (int)DealerFinanceAuditEntityType.Collection,
            DealerCollectionId = collection.Id,
            ActionTypeId = (int)DealerFinanceAuditActionType.CollectionCreated,
            Note = collection.Note,
            PerformedByCustomerId = currentCustomer.Id,
            PerformedOnUtc = DateTime.UtcNow
        });

        if (RequiresFinancialInstrument(collection.CollectionMethodId))
        {
            var financialInstrument = new DealerFinancialInstrument
            {
                DealerId = collection.DealerId,
                DealerCollectionId = collection.Id,
                CustomerId = collection.CustomerId,
                InstrumentTypeId = collection.CollectionMethodId == (int)DealerCollectionMethod.Check
                    ? (int)DealerFinancialInstrumentType.Check
                    : (int)DealerFinancialInstrumentType.PromissoryNote,
                InstrumentStatusId = (int)DealerFinancialInstrumentStatus.Posted,
                Amount = collection.Amount,
                InstrumentNo = collection.DocumentNo,
                IssueDateUtc = collection.IssueDateUtc,
                DueDateUtc = collection.DueDateUtc,
                Note = collection.Note,
                CreatedByCustomerId = currentCustomer.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _dealerService.InsertDealerFinancialInstrumentAsync(financialInstrument);
            collection.DealerFinancialInstrumentId = financialInstrument.Id;
            await _dealerService.UpdateDealerCollectionAsync(collection);

            await _dealerService.InsertDealerFinanceAuditLogAsync(new DealerFinanceAuditLog
            {
                DealerId = collection.DealerId,
                EntityTypeId = (int)DealerFinanceAuditEntityType.FinancialInstrument,
                DealerCollectionId = collection.Id,
                DealerFinancialInstrumentId = financialInstrument.Id,
                ActionTypeId = (int)DealerFinanceAuditActionType.FinancialInstrumentCreated,
                StatusAfterId = financialInstrument.InstrumentStatusId,
                Note = financialInstrument.Note,
                PerformedByCustomerId = currentCustomer.Id,
                PerformedOnUtc = DateTime.UtcNow
            });
        }

        transaction.SourceId = collection.Id;
        await _dealerService.UpdateDealerTransactionAsync(transaction);

        return RedirectToAction(nameof(Collections), new { SearchStoreId = model.StoreId, SearchDealerId = model.DealerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CancelCollection(int id)
    {
        var (currentCustomer, isStoreOwner, managedStoreId, _) = await GetAccessContextAsync();
        var collection = await _dealerService.GetDealerCollectionByIdAsync(id);
        if (collection is null)
            return RedirectToAction(nameof(Collections));

        var dealer = await _dealerService.GetDealerByIdAsync(collection.DealerId);
        if (dealer is null)
            return RedirectToAction(nameof(Collections));

        if (isStoreOwner && managedStoreId > 0 && dealer.StoreId != managedStoreId)
            return AccessDeniedView();

        if (collection.CollectionStatusId == (int)DealerCollectionStatus.Cancelled)
            return RedirectToAction(nameof(CollectionDetails), new { id = collection.Id });

        if (collection.DealerFinancialInstrumentId.HasValue)
        {
            var financialInstrument = await _dealerService.GetDealerFinancialInstrumentByIdAsync(collection.DealerFinancialInstrumentId.Value);
            if (financialInstrument is not null && financialInstrument.InstrumentStatusId != (int)DealerFinancialInstrumentStatus.Cancelled)
            {
                var previousStatusId = financialInstrument.InstrumentStatusId;
                financialInstrument.InstrumentStatusId = (int)DealerFinancialInstrumentStatus.Cancelled;
                financialInstrument.UpdatedOnUtc = DateTime.UtcNow;
                await _dealerService.UpdateDealerFinancialInstrumentAsync(financialInstrument);

                await _dealerService.InsertDealerFinanceAuditLogAsync(new DealerFinanceAuditLog
                {
                    DealerId = collection.DealerId,
                    EntityTypeId = (int)DealerFinanceAuditEntityType.FinancialInstrument,
                    DealerCollectionId = collection.Id,
                    DealerFinancialInstrumentId = financialInstrument.Id,
                    ActionTypeId = (int)DealerFinanceAuditActionType.FinancialInstrumentStatusChanged,
                    StatusBeforeId = previousStatusId,
                    StatusAfterId = financialInstrument.InstrumentStatusId,
                    Note = $"Collection cancellation #{collection.Id}",
                    PerformedByCustomerId = currentCustomer.Id,
                    PerformedOnUtc = DateTime.UtcNow
                });
            }
        }

        var reversalTransaction = new DealerTransaction
        {
            DealerId = collection.DealerId,
            CustomerId = collection.CustomerId,
            SourceTypeId = (int)DealerTransactionSourceType.Collection,
            SourceId = collection.Id,
            ReferenceNo = collection.ReferenceNo,
            TransactionTypeId = (int)DealerTransactionType.OpenAccountCollection,
            DirectionId = (int)DealerTransactionDirection.Debit,
            Amount = collection.Amount,
            Note = string.IsNullOrWhiteSpace(collection.ReferenceNo)
                ? $"Collection cancellation for dealer collection #{collection.Id}"
                : $"Collection cancellation for dealer collection #{collection.Id}. Ref: {collection.ReferenceNo}",
            CreatedOnUtc = DateTime.UtcNow
        };

        await _dealerService.InsertDealerTransactionAsync(reversalTransaction);

        collection.CollectionStatusId = (int)DealerCollectionStatus.Cancelled;
        collection.CancelledDealerTransactionId = reversalTransaction.Id;
        collection.CancelledByCustomerId = currentCustomer.Id;
        collection.CancelledOnUtc = DateTime.UtcNow;

        await _dealerService.UpdateDealerCollectionAsync(collection);
        await _dealerService.InsertDealerFinanceAuditLogAsync(new DealerFinanceAuditLog
        {
            DealerId = collection.DealerId,
            EntityTypeId = (int)DealerFinanceAuditEntityType.Collection,
            DealerCollectionId = collection.Id,
            DealerFinancialInstrumentId = collection.DealerFinancialInstrumentId,
            ActionTypeId = (int)DealerFinanceAuditActionType.CollectionCancelled,
            Note = string.IsNullOrWhiteSpace(collection.ReferenceNo) ? null : collection.ReferenceNo,
            PerformedByCustomerId = currentCustomer.Id,
            PerformedOnUtc = DateTime.UtcNow
        });

        return RedirectToAction(nameof(CollectionDetails), new { id = collection.Id });
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
                SourceTypeId = (int)DealerTransactionSourceType.ManualAdjustment,
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
