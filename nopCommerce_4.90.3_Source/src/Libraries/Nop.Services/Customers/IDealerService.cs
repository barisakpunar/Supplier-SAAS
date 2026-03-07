using Nop.Core;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers;

/// <summary>
/// Dealer service interface
/// </summary>
public partial interface IDealerService
{
    /// <summary>
    /// Gets a dealer by identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer
    /// </returns>
    Task<DealerInfo> GetDealerByIdAsync(int dealerId);

    /// <summary>
    /// Gets a dealer by customer identifier
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer
    /// </returns>
    Task<DealerInfo> GetDealerByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets a dealer identifier by customer identifier
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer identifier
    /// </returns>
    Task<int> GetDealerIdByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets dealer financial profile by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer financial profile
    /// </returns>
    Task<DealerFinancialProfile> GetDealerFinancialProfileByDealerIdAsync(int dealerId);

    /// <summary>
    /// Gets dealer collection by identifier
    /// </summary>
    /// <param name="dealerCollectionId">Dealer collection identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer collection
    /// </returns>
    Task<DealerCollection> GetDealerCollectionByIdAsync(int dealerCollectionId);

    /// <summary>
    /// Gets dealer financial instrument by identifier
    /// </summary>
    /// <param name="dealerFinancialInstrumentId">Dealer financial instrument identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer financial instrument
    /// </returns>
    Task<DealerFinancialInstrument> GetDealerFinancialInstrumentByIdAsync(int dealerFinancialInstrumentId);

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
    Task<IList<DealerFinanceAuditLog>> SearchDealerFinanceAuditLogsAsync(int dealerId = 0, int dealerCollectionId = 0,
        int dealerFinancialInstrumentId = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets dealer transaction by identifier
    /// </summary>
    /// <param name="dealerTransactionId">Dealer transaction identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dealer transaction
    /// </returns>
    Task<DealerTransaction> GetDealerTransactionByIdAsync(int dealerTransactionId);

    /// <summary>
    /// Gets current open account debt for dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains current open account debt
    /// </returns>
    Task<decimal> GetOpenAccountCurrentDebtAsync(int dealerId);

    /// <summary>
    /// Gets available open account credit for dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains available credit
    /// </returns>
    Task<decimal> GetOpenAccountAvailableCreditAsync(int dealerId);

    /// <summary>
    /// Gets dealer transactions
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
    Task<IList<DealerTransaction>> SearchDealerTransactionsAsync(int dealerId = 0, int storeId = 0,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets dealer transactions
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer transactions
    /// </returns>
    Task<IList<DealerTransaction>> GetDealerTransactionsAsync(int dealerId, int pageSize = int.MaxValue);

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
    Task<IList<DealerCollection>> SearchDealerCollectionsAsync(int dealerId = 0, int storeId = 0, int customerId = 0,
        int collectionMethodId = 0, int collectionStatusId = 0, DateTime? collectionFromUtc = null,
        DateTime? collectionToUtc = null, int pageSize = int.MaxValue);

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
    Task<IList<DealerFinancialInstrument>> SearchDealerFinancialInstrumentsAsync(int dealerId = 0, int storeId = 0,
        int customerId = 0, int instrumentTypeId = 0, int instrumentStatusId = 0, DateTime? dueFromUtc = null,
        DateTime? dueToUtc = null, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets dealer collections by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer collections
    /// </returns>
    Task<IList<DealerCollection>> GetDealerCollectionsByDealerIdAsync(int dealerId, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets dealer financial instruments by dealer identifier
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer financial instruments
    /// </returns>
    Task<IList<DealerFinancialInstrument>> GetDealerFinancialInstrumentsByDealerIdAsync(int dealerId, int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a dealer transaction
    /// </summary>
    /// <param name="dealerTransaction">Dealer transaction</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerTransactionAsync(DealerTransaction dealerTransaction);

    /// <summary>
    /// Inserts a dealer collection
    /// </summary>
    /// <param name="dealerCollection">Dealer collection</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerCollectionAsync(DealerCollection dealerCollection);

    /// <summary>
    /// Inserts a dealer financial instrument
    /// </summary>
    /// <param name="dealerFinancialInstrument">Dealer financial instrument</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerFinancialInstrumentAsync(DealerFinancialInstrument dealerFinancialInstrument);

    /// <summary>
    /// Inserts a dealer finance audit log entry
    /// </summary>
    /// <param name="dealerFinanceAuditLog">Dealer finance audit log entry</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerFinanceAuditLogAsync(DealerFinanceAuditLog dealerFinanceAuditLog);

    /// <summary>
    /// Updates a dealer financial instrument
    /// </summary>
    /// <param name="dealerFinancialInstrument">Dealer financial instrument</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateDealerFinancialInstrumentAsync(DealerFinancialInstrument dealerFinancialInstrument);

    /// <summary>
    /// Updates a dealer transaction
    /// </summary>
    /// <param name="dealerTransaction">Dealer transaction</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateDealerTransactionAsync(DealerTransaction dealerTransaction);

    /// <summary>
    /// Updates a dealer collection
    /// </summary>
    /// <param name="dealerCollection">Dealer collection</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateDealerCollectionAsync(DealerCollection dealerCollection);

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
    Task<bool> DealerTransactionExistsAsync(int dealerId, int orderId, int transactionTypeId);

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
    Task<IPagedList<DealerInfo>> SearchDealersAsync(string name = "", int storeId = 0, bool? active = null,
        int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets dealer-customer mappings
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains mappings
    /// </returns>
    Task<IList<DealerCustomerMapping>> GetDealerCustomerMappingsAsync(int dealerId = 0, int customerId = 0);

    /// <summary>
    /// Gets customer identifiers by dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains customer identifiers
    /// </returns>
    Task<IList<int>> GetCustomerIdsByDealerIdAsync(int dealerId);

    /// <summary>
    /// Gets dealer-payment method mappings
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains mappings
    /// </returns>
    Task<IList<DealerPaymentMethodMapping>> GetDealerPaymentMethodMappingsAsync(int dealerId = 0);

    /// <summary>
    /// Gets allowed payment method system names by dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains payment method system names
    /// </returns>
    Task<IList<string>> GetAllowedPaymentMethodSystemNamesAsync(int dealerId);

    /// <summary>
    /// Indicates whether a payment method is allowed for the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="paymentMethodSystemName">Payment method system name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether payment method is allowed
    /// </returns>
    Task<bool> IsPaymentMethodAllowedForDealerAsync(int dealerId, string paymentMethodSystemName);

    /// <summary>
    /// Replaces allowed payment method system names for the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="paymentMethodSystemNames">Payment method system names</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SetAllowedPaymentMethodSystemNamesAsync(int dealerId, IList<string> paymentMethodSystemNames);

    /// <summary>
    /// Creates or updates dealer financial profile
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="openAccountEnabled">Open account enabled flag</param>
    /// <param name="creditLimit">Credit limit</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpsertDealerFinancialProfileAsync(int dealerId, bool openAccountEnabled, decimal creditLimit);

    /// <summary>
    /// Indicates whether a customer is mapped to the dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether customer is mapped
    /// </returns>
    Task<bool> IsCustomerMappedToDealerAsync(int dealerId, int customerId);

    /// <summary>
    /// Maps a customer to dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task MapCustomerToDealerAsync(int dealerId, int customerId);

    /// <summary>
    /// Unmaps customer from dealer
    /// </summary>
    /// <param name="dealerId">Dealer identifier</param>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UnmapCustomerFromDealerAsync(int dealerId, int customerId);

    /// <summary>
    /// Inserts a dealer
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerAsync(DealerInfo dealer);

    /// <summary>
    /// Updates a dealer
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateDealerAsync(DealerInfo dealer);

    /// <summary>
    /// Deletes a dealer and related mappings
    /// </summary>
    /// <param name="dealer">Dealer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteDealerAsync(DealerInfo dealer);
}
