using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

public partial class StoreOwnerController : BaseAdminController
{
    #region Fields

    protected readonly IStoreService _storeService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public StoreOwnerController(IStoreService storeService, IWorkContext workContext)
    {
        _storeService = storeService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Security.ACCESS_STORE_OWNER_PANEL)]
    public virtual async Task<IActionResult> Index()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeService.GetStoreByIdAsync(customer.RegisteredInStoreId);

        ViewBag.StoreOwnerEmail = customer.Email;
        ViewBag.StoreId = customer.RegisteredInStoreId;
        ViewBag.StoreName = store?.Name ?? "-";

        return View();
    }

    #endregion
}
