using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Web.Areas.Admin.Infrastructure;

/// <summary>
/// Represents store owner admin menu consumer
/// </summary>
public partial class StoreOwnerAdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public StoreOwnerAdminMenuEventConsumer(ICustomerService customerService, IWorkContext workContext)
    {
        _customerService = customerService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle admin menu created event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsAdminAsync(customer))
            return;

        if (!await _customerService.IsInCustomerRoleAsync(customer, NopCustomerDefaults.StoreOwnersRoleName))
            return;

        eventMessage.RootMenuItem.ChildNodes = new List<AdminMenuItem>
        {
            new()
            {
                Visible = true,
                SystemName = "StoreOwnerPanel",
                Title = "Store Owner Panel",
                Url = eventMessage.GetMenuItemUrl("StoreOwner", "Index"),
                IconClass = "fas fa-store",
                PermissionNames = new List<string> { StandardPermission.Security.ACCESS_STORE_OWNER_PANEL }
            }
        };
    }

    #endregion
}
