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
