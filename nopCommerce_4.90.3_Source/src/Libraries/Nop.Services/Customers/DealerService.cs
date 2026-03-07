using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;

namespace Nop.Services.Customers;

/// <summary>
/// Dealer service
/// </summary>
public partial class DealerService : IDealerService
{
    #region Fields

    protected readonly IRepository<DealerInfo> _dealerInfoRepository;
    protected readonly IRepository<DealerFinancialProfile> _dealerFinancialProfileRepository;
    protected readonly IRepository<DealerTransaction> _dealerTransactionRepository;
    protected readonly IRepository<DealerTransactionAllocation> _dealerTransactionAllocationRepository;
    protected readonly IRepository<DealerCollection> _dealerCollectionRepository;
    protected readonly IRepository<DealerFinancialInstrument> _dealerFinancialInstrumentRepository;
    protected readonly IRepository<DealerFinanceAuditLog> _dealerFinanceAuditLogRepository;
    protected readonly IRepository<DealerCustomerMapping> _dealerCustomerMappingRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<DealerPaymentMethodMapping> _dealerPaymentMethodMappingRepository;

    protected const string OpenAccountPaymentMethodSystemName = "Payments.OpenAccount";

    #endregion

    #region Ctor

    public DealerService(IRepository<DealerInfo> dealerInfoRepository,
        IRepository<DealerFinancialProfile> dealerFinancialProfileRepository,
        IRepository<DealerTransaction> dealerTransactionRepository,
        IRepository<DealerTransactionAllocation> dealerTransactionAllocationRepository,
        IRepository<DealerCollection> dealerCollectionRepository,
        IRepository<DealerFinancialInstrument> dealerFinancialInstrumentRepository,
        IRepository<DealerFinanceAuditLog> dealerFinanceAuditLogRepository,
        IRepository<DealerCustomerMapping> dealerCustomerMappingRepository,
        IRepository<Order> orderRepository,
        IRepository<DealerPaymentMethodMapping> dealerPaymentMethodMappingRepository)
    {
        _dealerInfoRepository = dealerInfoRepository;
        _dealerFinancialProfileRepository = dealerFinancialProfileRepository;
        _dealerTransactionRepository = dealerTransactionRepository;
        _dealerTransactionAllocationRepository = dealerTransactionAllocationRepository;
        _dealerCollectionRepository = dealerCollectionRepository;
        _dealerFinancialInstrumentRepository = dealerFinancialInstrumentRepository;
        _dealerFinanceAuditLogRepository = dealerFinanceAuditLogRepository;
        _dealerCustomerMappingRepository = dealerCustomerMappingRepository;
        _orderRepository = orderRepository;
        _dealerPaymentMethodMappingRepository = dealerPaymentMethodMappingRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a dealer by identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer
    /// </returns>
    public virtual async Task<DealerInfo> GetDealerByIdAsync(int dealerId)
    {
        if (dealerId <= 0)
            return null;

        return await _dealerInfoRepository.GetByIdAsync(dealerId, cache => default);
    }

    /// <summary>
    /// Gets a dealer by customer identifier
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer
    /// </returns>
    public virtual async Task<DealerInfo> GetDealerByCustomerIdAsync(int customerId)
    {
        var dealerId = await GetDealerIdByCustomerIdAsync(customerId);
        if (dealerId <= 0)
            return null;

        return await GetDealerByIdAsync(dealerId);
    }

    /// <summary>
    /// Gets a dealer identifier by customer identifier
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer identifier
    /// </returns>
    public virtual async Task<int> GetDealerIdByCustomerIdAsync(int customerId)
    {
        if (customerId <= 0)
            return 0;

        var dealerId = await _dealerCustomerMappingRepository.Table
            .Where(mapping => mapping.CustomerId == customerId)
            .Select(mapping => mapping.DealerId)
            .FirstOrDefaultAsync();

        return dealerId;
    }

    /// <summary>
    /// Gets dealer financial profile by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer financial profile
    /// </returns>
    public virtual async Task<DealerFinancialProfile> GetDealerFinancialProfileByDealerIdAsync(int dealerId)
    {
        if (dealerId <= 0)
            return null;

        return await _dealerFinancialProfileRepository.Table
            .FirstOrDefaultAsync(profile => profile.DealerId == dealerId);
    }

    /// <summary>
    /// Gets dealer collection by identifier
    /// </summary>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer collection
    /// </returns>
    public virtual async Task<DealerCollection> GetDealerCollectionByIdAsync(int dealerCollectionId)
    {
        if (dealerCollectionId <= 0)
            return null;

        return await _dealerCollectionRepository.GetByIdAsync(dealerCollectionId, cache => default);
    }

    /// <summary>
    /// Gets dealer financial instrument by identifier
    /// </summary>
    /// <param name="dealerFinancialInstrumentId">Dealer financial instrument identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer financial instrument
    /// </returns>
    public virtual async Task<DealerFinancialInstrument> GetDealerFinancialInstrumentByIdAsync(int dealerFinancialInstrumentId)
    {
        if (dealerFinancialInstrumentId <= 0)
            return null;

        return await _dealerFinancialInstrumentRepository.GetByIdAsync(dealerFinancialInstrumentId, cache => default);
    }

    /// <summary>
    /// Searches dealer finance audit logs
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <param name="dealerFinancialInstrumentId">Dealer financial instrument identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer finance audit logs
    /// </returns>
    public virtual async Task<IList<DealerFinanceAuditLog>> SearchDealerFinanceAuditLogsAsync(int dealerId = 0, int dealerCollectionId = 0,
        int dealerFinancialInstrumentId = 0, int pageSize = int.MaxValue)
    {
        if (pageSize <= 0)
            return new List<DealerFinanceAuditLog>();

        var query = _dealerFinanceAuditLogRepository.Table;

        if (dealerId > 0)
            query = query.Where(item => item.DealerId == dealerId);

        if (dealerCollectionId > 0)
            query = query.Where(item => item.DealerCollectionId == dealerCollectionId);

        if (dealerFinancialInstrumentId > 0)
            query = query.Where(item => item.DealerFinancialInstrumentId == dealerFinancialInstrumentId);

        return await query
            .OrderByDescending(item => item.PerformedOnUtc)
            .ThenByDescending(item => item.Id)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Searches dealer transaction allocations
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <param name="creditDealerTransactionId">Credit transaction identifier</param>
    /// <param name="debitDealerTransactionId">Debit transaction identifier</param>
    /// <param name="activeOnly">Active records only</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer transaction allocations
    /// </returns>
    public virtual async Task<IList<DealerTransactionAllocation>> SearchDealerTransactionAllocationsAsync(int dealerId = 0, int dealerCollectionId = 0,
        int creditDealerTransactionId = 0, int debitDealerTransactionId = 0, bool activeOnly = false, int pageSize = int.MaxValue)
    {
        if (pageSize <= 0)
            return new List<DealerTransactionAllocation>();

        var query = _dealerTransactionAllocationRepository.Table;

        if (dealerId > 0)
            query = query.Where(item => item.DealerId == dealerId);

        if (dealerCollectionId > 0)
            query = query.Where(item => item.DealerCollectionId == dealerCollectionId);

        if (creditDealerTransactionId > 0)
            query = query.Where(item => item.CreditDealerTransactionId == creditDealerTransactionId);

        if (debitDealerTransactionId > 0)
            query = query.Where(item => item.DebitDealerTransactionId == debitDealerTransactionId);

        if (activeOnly)
            query = query.Where(item => !item.CancelledOnUtc.HasValue);

        return await query
            .OrderBy(item => item.CreatedOnUtc)
            .ThenBy(item => item.Id)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets dealer transaction by identifier
    /// </summary>
    /// <param name="dealerTransactionId">Dealer transaction identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer transaction
    /// </returns>
    public virtual async Task<DealerTransaction> GetDealerTransactionByIdAsync(int dealerTransactionId)
    {
        if (dealerTransactionId <= 0)
            return null;

        return await _dealerTransactionRepository.GetByIdAsync(dealerTransactionId, cache => default);
    }

    /// <summary>
    /// Gets current open account debt for dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains current open account debt
    /// </returns>
    public virtual async Task<decimal> GetOpenAccountCurrentDebtAsync(int dealerId)
    {
        if (dealerId <= 0)
            return 0;

        var openAccountTransactions = await _dealerTransactionRepository.Table
            .Where(transaction => transaction.DealerId == dealerId
                                  && (transaction.TransactionTypeId == (int)DealerTransactionType.OpenAccountOrder
                                      || transaction.TransactionTypeId == (int)DealerTransactionType.OpenAccountCollection
                                      || transaction.TransactionTypeId == (int)DealerTransactionType.ManualDebitAdjustment
                                      || transaction.TransactionTypeId == (int)DealerTransactionType.ManualCreditAdjustment))
            .Select(transaction => new
            {
                transaction.DirectionId,
                transaction.Amount
            })
            .ToListAsync();

        var transactionBalance = openAccountTransactions.Sum(transaction =>
            transaction.DirectionId == (int)DealerTransactionDirection.Debit
                ? transaction.Amount
                : -transaction.Amount);

        var customerIds = await GetCustomerIdsByDealerIdAsync(dealerId);
        if (!customerIds.Any())
            return transactionBalance > 0 ? transactionBalance : 0;

        // Keep compatibility with legacy open-account orders created before ledger postings existed.
        var unmappedOpenAccountOrdersDebt = await _orderRepository.Table
            .Where(order => customerIds.Contains(order.CustomerId)
                            && !order.Deleted
                            && order.OrderStatusId != (int)OrderStatus.Cancelled
                            && order.PaymentMethodSystemName == OpenAccountPaymentMethodSystemName
                            && (order.PaymentStatusId == (int)PaymentStatus.Pending
                                || order.PaymentStatusId == (int)PaymentStatus.Authorized)
                            && !_dealerTransactionRepository.Table.Any(transaction =>
                                transaction.DealerId == dealerId
                                && transaction.OrderId == order.Id
                                && transaction.TransactionTypeId == (int)DealerTransactionType.OpenAccountOrder))
            .SumAsync(order => order.OrderTotal);

        var debt = transactionBalance + unmappedOpenAccountOrdersDebt;
        return debt > 0 ? debt : 0;
    }

    /// <summary>
    /// Gets available open account credit for dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains available credit
    /// </returns>
    public virtual async Task<decimal> GetOpenAccountAvailableCreditAsync(int dealerId)
    {
        if (dealerId <= 0)
            return 0;

        var profile = await GetDealerFinancialProfileByDealerIdAsync(dealerId);
        if (profile == null || !profile.OpenAccountEnabled)
            return 0;

        var currentDebt = await GetOpenAccountCurrentDebtAsync(dealerId);
        var availableCredit = profile.CreditLimit - currentDebt;
        return availableCredit > 0 ? availableCredit : 0;
    }

    /// <summary>
    /// Search dealer transactions
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="createdFromUtc">Created date from (UTC)</param>
    /// <param name="createdToUtc">Created date to (UTC)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer transactions
    /// </returns>
    public virtual async Task<IList<DealerTransaction>> SearchDealerTransactionsAsync(int dealerId = 0, int storeId = 0,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageSize = int.MaxValue)
    {
        if (pageSize <= 0)
            return new List<DealerTransaction>();

        var query = from transaction in _dealerTransactionRepository.Table
                    join dealer in _dealerInfoRepository.Table on transaction.DealerId equals dealer.Id
                    select new
                    {
                        Transaction = transaction,
                        dealer.StoreId
                    };

        if (dealerId > 0)
            query = query.Where(item => item.Transaction.DealerId == dealerId);

        if (storeId > 0)
            query = query.Where(item => item.StoreId == storeId);

        if (createdFromUtc.HasValue)
            query = query.Where(item => item.Transaction.CreatedOnUtc >= createdFromUtc.Value);

        if (createdToUtc.HasValue)
            query = query.Where(item => item.Transaction.CreatedOnUtc <= createdToUtc.Value);

        return await query
            .OrderByDescending(item => item.Transaction.CreatedOnUtc)
            .ThenByDescending(item => item.Transaction.Id)
            .Select(item => item.Transaction)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets dealer transactions
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer transactions
    /// </returns>
    public virtual async Task<IList<DealerTransaction>> GetDealerTransactionsAsync(int dealerId, int pageSize = int.MaxValue)
    {
        if (dealerId <= 0 || pageSize <= 0)
            return new List<DealerTransaction>();

        return await SearchDealerTransactionsAsync(dealerId: dealerId, pageSize: pageSize);
    }

    /// <summary>
    /// Searches dealer collections
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="collectionMethodId">Collection method identifier</param>
    /// <param name="collectionStatusId">Collection status identifier</param>
    /// <param name="collectionFromUtc">Collection date from (UTC)</param>
    /// <param name="collectionToUtc">Collection date to (UTC)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer collections
    /// </returns>
    public virtual async Task<IList<DealerCollection>> SearchDealerCollectionsAsync(int dealerId = 0, int storeId = 0,
        int customerId = 0, int collectionMethodId = 0, int collectionStatusId = 0, DateTime? collectionFromUtc = null,
        DateTime? collectionToUtc = null, int pageSize = int.MaxValue)
    {
        if (pageSize <= 0)
            return new List<DealerCollection>();

        var query = from collection in _dealerCollectionRepository.Table
                    join dealer in _dealerInfoRepository.Table on collection.DealerId equals dealer.Id
                    select new
                    {
                        Collection = collection,
                        dealer.StoreId
                    };

        if (dealerId > 0)
            query = query.Where(item => item.Collection.DealerId == dealerId);

        if (storeId > 0)
            query = query.Where(item => item.StoreId == storeId);

        if (customerId > 0)
            query = query.Where(item => item.Collection.CustomerId == customerId);

        if (collectionMethodId > 0)
            query = query.Where(item => item.Collection.CollectionMethodId == collectionMethodId);

        if (collectionStatusId > 0)
            query = query.Where(item => item.Collection.CollectionStatusId == collectionStatusId);

        if (collectionFromUtc.HasValue)
            query = query.Where(item => item.Collection.CollectionDateUtc >= collectionFromUtc.Value);

        if (collectionToUtc.HasValue)
            query = query.Where(item => item.Collection.CollectionDateUtc <= collectionToUtc.Value);

        return await query
            .OrderByDescending(item => item.Collection.CollectionDateUtc)
            .ThenByDescending(item => item.Collection.Id)
            .Select(item => item.Collection)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets dealer collections by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer collections
    /// </returns>
    public virtual async Task<IList<DealerCollection>> GetDealerCollectionsByDealerIdAsync(int dealerId, int pageSize = int.MaxValue)
    {
        if (dealerId <= 0 || pageSize <= 0)
            return new List<DealerCollection>();

        return await SearchDealerCollectionsAsync(dealerId: dealerId, pageSize: pageSize);
    }

    /// <summary>
    /// Searches dealer financial instruments
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="instrumentTypeId">Instrument type identifier</param>
    /// <param name="instrumentStatusId">Instrument status identifier</param>
    /// <param name="dueFromUtc">Due date from (UTC)</param>
    /// <param name="dueToUtc">Due date to (UTC)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer financial instruments
    /// </returns>
    public virtual async Task<IList<DealerFinancialInstrument>> SearchDealerFinancialInstrumentsAsync(int dealerId = 0, int storeId = 0,
        int customerId = 0, int instrumentTypeId = 0, int instrumentStatusId = 0, DateTime? dueFromUtc = null,
        DateTime? dueToUtc = null, int pageSize = int.MaxValue)
    {
        if (pageSize <= 0)
            return new List<DealerFinancialInstrument>();

        var query = from instrument in _dealerFinancialInstrumentRepository.Table
                    join dealer in _dealerInfoRepository.Table on instrument.DealerId equals dealer.Id
                    select new
                    {
                        Instrument = instrument,
                        dealer.StoreId
                    };

        if (dealerId > 0)
            query = query.Where(item => item.Instrument.DealerId == dealerId);

        if (storeId > 0)
            query = query.Where(item => item.StoreId == storeId);

        if (customerId > 0)
            query = query.Where(item => item.Instrument.CustomerId == customerId);

        if (instrumentTypeId > 0)
            query = query.Where(item => item.Instrument.InstrumentTypeId == instrumentTypeId);

        if (instrumentStatusId > 0)
            query = query.Where(item => item.Instrument.InstrumentStatusId == instrumentStatusId);

        if (dueFromUtc.HasValue)
            query = query.Where(item => item.Instrument.DueDateUtc.HasValue && item.Instrument.DueDateUtc.Value >= dueFromUtc.Value);

        if (dueToUtc.HasValue)
            query = query.Where(item => item.Instrument.DueDateUtc.HasValue && item.Instrument.DueDateUtc.Value <= dueToUtc.Value);

        return await query
            .OrderByDescending(item => item.Instrument.DueDateUtc.HasValue)
            .ThenBy(item => item.Instrument.DueDateUtc)
            .ThenByDescending(item => item.Instrument.Id)
            .Select(item => item.Instrument)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets dealer financial instruments by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer financial instruments
    /// </returns>
    public virtual async Task<IList<DealerFinancialInstrument>> GetDealerFinancialInstrumentsByDealerIdAsync(int dealerId, int pageSize = int.MaxValue)
    {
        if (dealerId <= 0 || pageSize <= 0)
            return new List<DealerFinancialInstrument>();

        return await SearchDealerFinancialInstrumentsAsync(dealerId: dealerId, pageSize: pageSize);
    }

    /// <summary>
    /// Inserts a dealer transaction
    /// </summary>
    /// <param name="dealerTransaction">Dealer transaction</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerTransactionAsync(DealerTransaction dealerTransaction)
    {
        ArgumentNullException.ThrowIfNull(dealerTransaction);

        if (dealerTransaction.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransaction.DealerId));

        if (dealerTransaction.Amount < decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerTransaction.Amount));

        if (dealerTransaction.CreatedOnUtc == default)
            dealerTransaction.CreatedOnUtc = DateTime.UtcNow;

        await _dealerTransactionRepository.InsertAsync(dealerTransaction);
    }

    /// <summary>
    /// Inserts a dealer collection
    /// </summary>
    /// <param name="dealerCollection">Dealer collection</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerCollectionAsync(DealerCollection dealerCollection)
    {
        ArgumentNullException.ThrowIfNull(dealerCollection);

        if (dealerCollection.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerCollection.DealerId));

        if (dealerCollection.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerCollection.Amount));

        if (dealerCollection.CreatedByCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerCollection.CreatedByCustomerId));

        if (dealerCollection.CreatedOnUtc == default)
            dealerCollection.CreatedOnUtc = DateTime.UtcNow;

        if (dealerCollection.CollectionDateUtc == default)
            dealerCollection.CollectionDateUtc = DateTime.UtcNow;

        await _dealerCollectionRepository.InsertAsync(dealerCollection);
    }

    /// <summary>
    /// Inserts a dealer financial instrument
    /// </summary>
    /// <param name="dealerFinancialInstrument">Dealer financial instrument</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerFinancialInstrumentAsync(DealerFinancialInstrument dealerFinancialInstrument)
    {
        ArgumentNullException.ThrowIfNull(dealerFinancialInstrument);

        if (dealerFinancialInstrument.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.DealerId));

        if (dealerFinancialInstrument.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.Amount));

        if (dealerFinancialInstrument.CreatedByCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.CreatedByCustomerId));

        if (dealerFinancialInstrument.CreatedOnUtc == default)
            dealerFinancialInstrument.CreatedOnUtc = DateTime.UtcNow;

        await _dealerFinancialInstrumentRepository.InsertAsync(dealerFinancialInstrument);
    }

    /// <summary>
    /// Inserts a dealer finance audit log entry
    /// </summary>
    /// <param name="dealerFinanceAuditLog">Dealer finance audit log entry</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerFinanceAuditLogAsync(DealerFinanceAuditLog dealerFinanceAuditLog)
    {
        ArgumentNullException.ThrowIfNull(dealerFinanceAuditLog);

        if (dealerFinanceAuditLog.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerFinanceAuditLog.DealerId));

        if (dealerFinanceAuditLog.PerformedOnUtc == default)
            dealerFinanceAuditLog.PerformedOnUtc = DateTime.UtcNow;

        await _dealerFinanceAuditLogRepository.InsertAsync(dealerFinanceAuditLog);
    }

    /// <summary>
    /// Inserts a dealer transaction allocation
    /// </summary>
    /// <param name="dealerTransactionAllocation">Dealer transaction allocation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerTransactionAllocationAsync(DealerTransactionAllocation dealerTransactionAllocation)
    {
        ArgumentNullException.ThrowIfNull(dealerTransactionAllocation);

        if (dealerTransactionAllocation.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.DealerId));

        if (dealerTransactionAllocation.CreditDealerTransactionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.CreditDealerTransactionId));

        if (dealerTransactionAllocation.DebitDealerTransactionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.DebitDealerTransactionId));

        if (dealerTransactionAllocation.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.Amount));

        if (dealerTransactionAllocation.CreatedByCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.CreatedByCustomerId));

        if (dealerTransactionAllocation.CreatedOnUtc == default)
            dealerTransactionAllocation.CreatedOnUtc = DateTime.UtcNow;

        await _dealerTransactionAllocationRepository.InsertAsync(dealerTransactionAllocation);
    }

    /// <summary>
    /// Updates a dealer financial instrument
    /// </summary>
    /// <param name="dealerFinancialInstrument">Dealer financial instrument</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateDealerFinancialInstrumentAsync(DealerFinancialInstrument dealerFinancialInstrument)
    {
        ArgumentNullException.ThrowIfNull(dealerFinancialInstrument);

        if (dealerFinancialInstrument.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.DealerId));

        if (dealerFinancialInstrument.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.Amount));

        if (dealerFinancialInstrument.CreatedByCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerFinancialInstrument.CreatedByCustomerId));

        await _dealerFinancialInstrumentRepository.UpdateAsync(dealerFinancialInstrument);
    }

    /// <summary>
    /// Updates a dealer transaction allocation
    /// </summary>
    /// <param name="dealerTransactionAllocation">Dealer transaction allocation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateDealerTransactionAllocationAsync(DealerTransactionAllocation dealerTransactionAllocation)
    {
        ArgumentNullException.ThrowIfNull(dealerTransactionAllocation);

        if (dealerTransactionAllocation.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.DealerId));

        if (dealerTransactionAllocation.CreditDealerTransactionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.CreditDealerTransactionId));

        if (dealerTransactionAllocation.DebitDealerTransactionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.DebitDealerTransactionId));

        if (dealerTransactionAllocation.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.Amount));

        if (dealerTransactionAllocation.CreatedByCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransactionAllocation.CreatedByCustomerId));

        await _dealerTransactionAllocationRepository.UpdateAsync(dealerTransactionAllocation);
    }

    /// <summary>
    /// Updates a dealer transaction
    /// </summary>
    /// <param name="dealerTransaction">Dealer transaction</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateDealerTransactionAsync(DealerTransaction dealerTransaction)
    {
        ArgumentNullException.ThrowIfNull(dealerTransaction);

        if (dealerTransaction.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerTransaction.DealerId));

        if (dealerTransaction.Amount < decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerTransaction.Amount));

        if (dealerTransaction.CreatedOnUtc == default)
            throw new ArgumentOutOfRangeException(nameof(dealerTransaction.CreatedOnUtc));

        await _dealerTransactionRepository.UpdateAsync(dealerTransaction);
    }

    /// <summary>
    /// Updates a dealer collection
    /// </summary>
    /// <param name="dealerCollection">Dealer collection</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateDealerCollectionAsync(DealerCollection dealerCollection)
    {
        ArgumentNullException.ThrowIfNull(dealerCollection);

        if (dealerCollection.DealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerCollection.DealerId));

        if (dealerCollection.Amount <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(dealerCollection.Amount));

        dealerCollection.UpdatedOnUtc = DateTime.UtcNow;
        await _dealerCollectionRepository.UpdateAsync(dealerCollection);
    }

    /// <summary>
    /// Creates automatic allocations for a credit transaction
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="creditDealerTransactionId">Credit transaction identifier</param>
    /// <param name="creditAmount">Credit amount</param>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <param name="createdByCustomerId">Created by customer identifier</param>
    /// <param name="createdOnUtc">Created date in UTC</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains created allocations
    /// </returns>
    public virtual async Task<IList<DealerTransactionAllocation>> CreateAutomaticAllocationsAsync(int dealerId, int creditDealerTransactionId,
        decimal creditAmount, int? dealerCollectionId, int createdByCustomerId, DateTime createdOnUtc)
    {
        if (dealerId <= 0 || creditDealerTransactionId <= 0 || creditAmount <= decimal.Zero || createdByCustomerId <= 0)
            return new List<DealerTransactionAllocation>();

        var debitTransactions = await _dealerTransactionRepository.Table
            .Where(transaction => transaction.DealerId == dealerId
                                  && transaction.DirectionId == (int)DealerTransactionDirection.Debit
                                  && (transaction.TransactionTypeId == (int)DealerTransactionType.OpenAccountOrder
                                      || transaction.TransactionTypeId == (int)DealerTransactionType.ManualDebitAdjustment))
            .OrderBy(transaction => transaction.CreatedOnUtc)
            .ThenBy(transaction => transaction.Id)
            .ToListAsync();

        if (!debitTransactions.Any())
            return new List<DealerTransactionAllocation>();

        var debitTransactionIds = debitTransactions.Select(transaction => transaction.Id).ToArray();
        var activeAllocations = await _dealerTransactionAllocationRepository.Table
            .Where(item => item.DealerId == dealerId
                           && !item.CancelledOnUtc.HasValue
                           && debitTransactionIds.Contains(item.DebitDealerTransactionId))
            .GroupBy(item => item.DebitDealerTransactionId)
            .Select(group => new
            {
                DebitDealerTransactionId = group.Key,
                Amount = group.Sum(item => item.Amount)
            })
            .ToListAsync();

        var allocatedByDebitTransactionId = activeAllocations.ToDictionary(item => item.DebitDealerTransactionId, item => item.Amount);
        var remainingCredit = creditAmount;
        var result = new List<DealerTransactionAllocation>();

        foreach (var debitTransaction in debitTransactions)
        {
            if (remainingCredit <= decimal.Zero)
                break;

            var allocatedAmount = allocatedByDebitTransactionId.TryGetValue(debitTransaction.Id, out var existingAllocatedAmount)
                ? existingAllocatedAmount
                : decimal.Zero;
            var remainingDebit = debitTransaction.Amount - allocatedAmount;
            if (remainingDebit <= decimal.Zero)
                continue;

            var amountToAllocate = Math.Min(remainingCredit, remainingDebit);
            if (amountToAllocate <= decimal.Zero)
                continue;

            var allocation = new DealerTransactionAllocation
            {
                DealerId = dealerId,
                DealerCollectionId = dealerCollectionId,
                CreditDealerTransactionId = creditDealerTransactionId,
                DebitDealerTransactionId = debitTransaction.Id,
                Amount = amountToAllocate,
                CreatedByCustomerId = createdByCustomerId,
                CreatedOnUtc = createdOnUtc
            };

            await InsertDealerTransactionAllocationAsync(allocation);
            result.Add(allocation);
            remainingCredit -= amountToAllocate;
        }

        return result;
    }

    /// <summary>
    /// Cancels allocations by collection identifier
    /// </summary>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <param name="cancelledByCustomerId">Cancelled by customer identifier</param>
    /// <param name="cancelledOnUtc">Cancelled date in UTC</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task CancelDealerTransactionAllocationsByCollectionAsync(int dealerCollectionId, int cancelledByCustomerId, DateTime cancelledOnUtc)
    {
        if (dealerCollectionId <= 0 || cancelledByCustomerId <= 0)
            return;

        var allocations = await SearchDealerTransactionAllocationsAsync(dealerCollectionId: dealerCollectionId, activeOnly: true, pageSize: int.MaxValue);
        foreach (var allocation in allocations)
        {
            allocation.CancelledByCustomerId = cancelledByCustomerId;
            allocation.CancelledOnUtc = cancelledOnUtc == default ? DateTime.UtcNow : cancelledOnUtc;
            await UpdateDealerTransactionAllocationAsync(allocation);
        }
    }

    /// <summary>
    /// Indicates whether a dealer transaction exists for the order and transaction type
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="orderId">Order identifier</param>
    /// <param name="transactionTypeId">Transaction type identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether transaction exists
    /// </returns>
    public virtual async Task<bool> DealerTransactionExistsAsync(int dealerId, int orderId, int transactionTypeId)
    {
        if (dealerId <= 0 || orderId <= 0 || transactionTypeId <= 0)
            return false;

        return await _dealerTransactionRepository.Table
            .AnyAsync(transaction => transaction.DealerId == dealerId
                                     && transaction.OrderId == orderId
                                     && transaction.TransactionTypeId == transactionTypeId);
    }

    /// <summary>
    /// Search dealers
    /// </summary>
    /// <param name="name">Dealer name</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="active">Dealer active flag</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealers
    /// </returns>
    public virtual async Task<IPagedList<DealerInfo>> SearchDealersAsync(string name = "", int storeId = 0,
        bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _dealerInfoRepository.Table;

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(dealer => dealer.Name.Contains(name));

        if (storeId > 0)
            query = query.Where(dealer => dealer.StoreId == storeId);

        if (active.HasValue)
            query = query.Where(dealer => dealer.Active == active.Value);

        query = query.OrderBy(dealer => dealer.Name).ThenBy(dealer => dealer.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets dealer-customer mappings
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains mappings
    /// </returns>
    public virtual async Task<IList<DealerCustomerMapping>> GetDealerCustomerMappingsAsync(int dealerId = 0, int customerId = 0)
    {
        var query = _dealerCustomerMappingRepository.Table;

        if (dealerId > 0)
            query = query.Where(mapping => mapping.DealerId == dealerId);

        if (customerId > 0)
            query = query.Where(mapping => mapping.CustomerId == customerId);

        query = query.OrderBy(mapping => mapping.DealerId).ThenBy(mapping => mapping.CustomerId);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets customer identifiers by dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains customer identifiers
    /// </returns>
    public virtual async Task<IList<int>> GetCustomerIdsByDealerIdAsync(int dealerId)
    {
        if (dealerId <= 0)
            return new List<int>();

        return await _dealerCustomerMappingRepository.Table
            .Where(mapping => mapping.DealerId == dealerId)
            .Select(mapping => mapping.CustomerId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets dealer-payment method mappings
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains mappings
    /// </returns>
    public virtual async Task<IList<DealerPaymentMethodMapping>> GetDealerPaymentMethodMappingsAsync(int dealerId = 0)
    {
        var query = _dealerPaymentMethodMappingRepository.Table;

        if (dealerId > 0)
            query = query.Where(mapping => mapping.DealerId == dealerId);

        query = query.OrderBy(mapping => mapping.DealerId).ThenBy(mapping => mapping.PaymentMethodSystemName);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets allowed payment method system names by dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains payment method system names
    /// </returns>
    public virtual async Task<IList<string>> GetAllowedPaymentMethodSystemNamesAsync(int dealerId)
    {
        if (dealerId <= 0)
            return new List<string>();

        return await _dealerPaymentMethodMappingRepository.Table
            .Where(mapping => mapping.DealerId == dealerId)
            .Select(mapping => mapping.PaymentMethodSystemName)
            .OrderBy(systemName => systemName)
            .ToListAsync();
    }

    /// <summary>
    /// Indicates whether a payment method is allowed for the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="paymentMethodSystemName">Payment method system name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether payment method is allowed
    /// </returns>
    public virtual async Task<bool> IsPaymentMethodAllowedForDealerAsync(int dealerId, string paymentMethodSystemName)
    {
        if (dealerId <= 0 || string.IsNullOrWhiteSpace(paymentMethodSystemName))
            return false;

        var normalizedSystemName = paymentMethodSystemName.Trim();
        var allowedPaymentMethodSystemNames = await GetAllowedPaymentMethodSystemNamesAsync(dealerId);
        return allowedPaymentMethodSystemNames
            .Any(systemName => systemName.Equals(normalizedSystemName, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Replaces allowed payment method system names for the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="paymentMethodSystemNames">Payment method system names</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SetAllowedPaymentMethodSystemNamesAsync(int dealerId, IList<string> paymentMethodSystemNames)
    {
        if (dealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerId));

        var dealer = await GetDealerByIdAsync(dealerId);
        if (dealer == null)
            throw new ArgumentException("Dealer not found.", nameof(dealerId));

        var normalizedSystemNames = (paymentMethodSystemNames ?? [])
            .Where(systemName => !string.IsNullOrWhiteSpace(systemName))
            .Select(systemName => systemName.Trim())
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToList();

        await _dealerPaymentMethodMappingRepository.DeleteAsync(mapping => mapping.DealerId == dealerId);

        if (!normalizedSystemNames.Any())
            return;

        var mappings = normalizedSystemNames.Select(systemName => new DealerPaymentMethodMapping
        {
            DealerId = dealerId,
            PaymentMethodSystemName = systemName
        }).ToList();

        await _dealerPaymentMethodMappingRepository.InsertAsync(mappings);
    }

    /// <summary>
    /// Creates or updates dealer financial profile
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="openAccountEnabled">Open account enabled flag</param>
    /// <param name="creditLimit">Credit limit</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpsertDealerFinancialProfileAsync(int dealerId, bool openAccountEnabled, decimal creditLimit)
    {
        if (dealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerId));

        if (creditLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(creditLimit));

        var dealer = await GetDealerByIdAsync(dealerId);
        if (dealer == null)
            throw new ArgumentException("Dealer not found.", nameof(dealerId));

        var profile = await GetDealerFinancialProfileByDealerIdAsync(dealerId);
        if (profile == null)
        {
            profile = new DealerFinancialProfile
            {
                DealerId = dealerId,
                OpenAccountEnabled = openAccountEnabled,
                CreditLimit = creditLimit,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _dealerFinancialProfileRepository.InsertAsync(profile);
            return;
        }

        profile.OpenAccountEnabled = openAccountEnabled;
        profile.CreditLimit = creditLimit;
        profile.UpdatedOnUtc = DateTime.UtcNow;
        await _dealerFinancialProfileRepository.UpdateAsync(profile);
    }

    /// <summary>
    /// Indicates whether a customer is mapped to the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether customer is mapped
    /// </returns>
    public virtual async Task<bool> IsCustomerMappedToDealerAsync(int dealerId, int customerId)
    {
        if (dealerId <= 0 || customerId <= 0)
            return false;

        return await _dealerCustomerMappingRepository.Table
            .AnyAsync(mapping => mapping.DealerId == dealerId && mapping.CustomerId == customerId);
    }

    /// <summary>
    /// Maps a customer to dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task MapCustomerToDealerAsync(int dealerId, int customerId)
    {
        if (dealerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(dealerId));

        if (customerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(customerId));

        var dealer = await GetDealerByIdAsync(dealerId);
        if (dealer == null)
            throw new ArgumentException("Dealer not found.", nameof(dealerId));

        var mappings = await _dealerCustomerMappingRepository.Table
            .Where(mapping => mapping.CustomerId == customerId)
            .ToListAsync();

        if (mappings.Any(mapping => mapping.DealerId == dealerId))
            return;

        if (mappings.Any())
            await _dealerCustomerMappingRepository.DeleteAsync(mappings);

        await _dealerCustomerMappingRepository.InsertAsync(new DealerCustomerMapping
        {
            DealerId = dealerId,
            CustomerId = customerId
        });
    }

    /// <summary>
    /// Unmaps customer from dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UnmapCustomerFromDealerAsync(int dealerId, int customerId)
    {
        if (dealerId <= 0 || customerId <= 0)
            return;

        var mapping = await _dealerCustomerMappingRepository.Table
            .SingleOrDefaultAsync(item => item.DealerId == dealerId && item.CustomerId == customerId);

        if (mapping != null)
            await _dealerCustomerMappingRepository.DeleteAsync(mapping);
    }

    /// <summary>
    /// Inserts a dealer
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertDealerAsync(DealerInfo dealer)
    {
        ArgumentNullException.ThrowIfNull(dealer);

        if (dealer.CreatedOnUtc == default)
            dealer.CreatedOnUtc = DateTime.UtcNow;

        dealer.UpdatedOnUtc = null;
        await _dealerInfoRepository.InsertAsync(dealer);
    }

    /// <summary>
    /// Updates a dealer
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateDealerAsync(DealerInfo dealer)
    {
        ArgumentNullException.ThrowIfNull(dealer);

        dealer.UpdatedOnUtc = DateTime.UtcNow;
        await _dealerInfoRepository.UpdateAsync(dealer);
    }

    /// <summary>
    /// Deletes a dealer and related mappings
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteDealerAsync(DealerInfo dealer)
    {
        ArgumentNullException.ThrowIfNull(dealer);

        await _dealerPaymentMethodMappingRepository.DeleteAsync(mapping => mapping.DealerId == dealer.Id);
        await _dealerCustomerMappingRepository.DeleteAsync(mapping => mapping.DealerId == dealer.Id);
        await _dealerFinancialProfileRepository.DeleteAsync(profile => profile.DealerId == dealer.Id);
        await _dealerTransactionRepository.DeleteAsync(transaction => transaction.DealerId == dealer.Id);
        await _dealerInfoRepository.DeleteAsync(dealer);
    }

    #endregion
}
