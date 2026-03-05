using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Web.Areas.Admin.Infrastructure;

/// <summary>
/// Represents dealer admin menu consumer
/// </summary>
public partial class DealerAdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public DealerAdminMenuEventConsumer(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
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
        var customersRoot = eventMessage.RootMenuItem.GetItemBySystemName("Customers");
        if (customersRoot is null)
            return;

        if (!customersRoot.ContainsSystemName("Dealers"))
        {
            customersRoot.InsertAfter("Customers list", new AdminMenuItem
            {
                Visible = true,
                SystemName = "Dealers",
                Title = await _localizationService.GetResourceAsync("Admin.Customers.Dealers"),
                Url = eventMessage.GetMenuItemUrl("Dealer", "List"),
                IconClass = "far fa-dot-circle",
                PermissionNames = new List<string> { StandardPermission.Customers.CUSTOMERS_VIEW }
            });
        }

        if (customersRoot.ContainsSystemName("DealerTransactions"))
            return;

        customersRoot.InsertAfter("Dealers", new AdminMenuItem
        {
            Visible = true,
            SystemName = "DealerTransactions",
            Title = await _localizationService.GetResourceAsync("Admin.Customers.Dealers.Transactions"),
            Url = eventMessage.GetMenuItemUrl("Dealer", "Transactions"),
            IconClass = "far fa-dot-circle",
            PermissionNames = new List<string> { StandardPermission.Customers.CUSTOMERS_VIEW }
        });
    }

    #endregion
}
