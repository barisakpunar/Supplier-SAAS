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

    private static readonly HashSet<string> _allowedMenuItems = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Home",
        "Dashboard",
        "Catalog",
        "Products",
        "Categories",
        "Manufacturers",
        "Product reviews",
        "Sales",
        "Orders",
        "Shipments",
        "Return requests",
        "Customers",
        "Customers list",
        "Dealers",
        "DealerTransactions",
        "Promotions",
        "Discounts"
    };

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

        static bool prune(AdminMenuItem node)
        {
            if (node is null)
                return false;

            node.ChildNodes = node.ChildNodes.Where(prune).ToList();
            return _allowedMenuItems.Contains(node.SystemName) || node.ChildNodes.Any();
        }

        eventMessage.RootMenuItem.ChildNodes = eventMessage.RootMenuItem.ChildNodes.Where(prune).ToList();

        var dashboard = eventMessage.RootMenuItem.GetItemBySystemName("Dashboard");
        if (dashboard != null)
        {
            dashboard.PermissionNames = new List<string> { StandardPermission.Security.ACCESS_STORE_OWNER_PANEL };
            return;
        }

        eventMessage.RootMenuItem.ChildNodes.Insert(0, new AdminMenuItem
        {
            Visible = true,
            SystemName = "Dashboard",
            Title = "Dashboard",
            Url = eventMessage.GetMenuItemUrl("Home", "Index"),
            IconClass = "fas fa-desktop",
            PermissionNames = new List<string> { StandardPermission.Security.ACCESS_STORE_OWNER_PANEL }
        });
    }

    #endregion
}
