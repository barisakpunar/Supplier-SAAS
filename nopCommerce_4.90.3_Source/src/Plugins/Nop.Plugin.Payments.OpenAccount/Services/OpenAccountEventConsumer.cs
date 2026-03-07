using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Plugin.Payments.OpenAccount.Services;

/// <summary>
/// Handles open account dealer transaction postings
/// </summary>
public class OpenAccountEventConsumer :
    IConsumer<OrderPlacedEvent>
{
    #region Fields

    protected readonly IDealerService _dealerService;

    #endregion

    #region Ctor

    public OpenAccountEventConsumer(IDealerService dealerService)
    {
        _dealerService = dealerService;
    }

    #endregion

    #region Utilities

    protected virtual bool IsOpenAccountOrder(Order order)
    {
        return order != null
               && !order.Deleted
               && !string.IsNullOrWhiteSpace(order.PaymentMethodSystemName)
               && order.PaymentMethodSystemName.Equals(OpenAccountPaymentDefaults.SystemName, StringComparison.InvariantCultureIgnoreCase);
    }

    protected virtual async Task<int> GetDealerIdAsync(Order order)
    {
        if (order?.CustomerId <= 0)
            return 0;

        return await _dealerService.GetDealerIdByCustomerIdAsync(order.CustomerId);
    }

    protected virtual async Task InsertOrderDebitTransactionAsync(Order order, int dealerId)
    {
        if (await _dealerService.DealerTransactionExistsAsync(dealerId, order.Id, (int)DealerTransactionType.OpenAccountOrder))
            return;

        await _dealerService.InsertDealerTransactionAsync(new DealerTransaction
        {
            DealerId = dealerId,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            SourceTypeId = (int)DealerTransactionSourceType.Order,
            SourceId = order.Id,
            ReferenceNo = order.CustomOrderNumber,
            TransactionTypeId = (int)DealerTransactionType.OpenAccountOrder,
            DirectionId = (int)DealerTransactionDirection.Debit,
            Amount = order.OrderTotal,
            Note = $"Open account order posted. Order #{order.CustomOrderNumber}",
            CreatedOnUtc = DateTime.UtcNow
        });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles order placed event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage?.Order;
        if (!IsOpenAccountOrder(order) || order.OrderTotal <= 0)
            return;

        var dealerId = await GetDealerIdAsync(order);
        if (dealerId <= 0)
            return;

        await InsertOrderDebitTransactionAsync(order, dealerId);
    }

    #endregion
}
