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
    protected readonly IRepository<DealerCustomerMapping> _dealerCustomerMappingRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<DealerPaymentMethodMapping> _dealerPaymentMethodMappingRepository;

    protected const string OpenAccountPaymentMethodSystemName = "Payments.OpenAccount";

    #endregion

    #region Ctor

    public DealerService(IRepository<DealerInfo> dealerInfoRepository,
        IRepository<DealerFinancialProfile> dealerFinancialProfileRepository,
        IRepository<DealerTransaction> dealerTransactionRepository,
        IRepository<DealerCustomerMapping> dealerCustomerMappingRepository,
        IRepository<Order> orderRepository,
        IRepository<DealerPaymentMethodMapping> dealerPaymentMethodMappingRepository)
    {
        _dealerInfoRepository = dealerInfoRepository;
        _dealerFinancialProfileRepository = dealerFinancialProfileRepository;
        _dealerTransactionRepository = dealerTransactionRepository;
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

        if (openAccountTransactions.Any())
        {
            var balance = openAccountTransactions.Sum(transaction =>
                transaction.DirectionId == (int)DealerTransactionDirection.Debit
                    ? transaction.Amount
                    : -transaction.Amount);

            return balance > 0 ? balance : 0;
        }

        var customerIds = await GetCustomerIdsByDealerIdAsync(dealerId);
        if (!customerIds.Any())
            return 0;

        var debt = await _orderRepository.Table
            .Where(order => customerIds.Contains(order.CustomerId)
                            && !order.Deleted
                            && order.OrderStatusId != (int)OrderStatus.Cancelled
                            && order.PaymentMethodSystemName == OpenAccountPaymentMethodSystemName
                            && (order.PaymentStatusId == (int)PaymentStatus.Pending
                                || order.PaymentStatusId == (int)PaymentStatus.Authorized))
            .SumAsync(order => order.OrderTotal);

        return debt;
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
