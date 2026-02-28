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
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains dealer transactions
    /// </returns>
    Task<IList<DealerTransaction>> GetDealerTransactionsAsync(int dealerId, int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a dealer transaction
    /// </summary>
    /// <param name="dealerTransaction">Dealer transaction</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertDealerTransactionAsync(DealerTransaction dealerTransaction);

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
