using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;

namespace Nop.Web.Framework;

/// <summary>
/// Store context for web application
/// </summary>
public partial class WebStoreContext : IStoreContext
{
    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IRepository<Customer> _customerRepository;
    protected readonly IRepository<Store> _storeRepository;
    protected readonly IStoreService _storeService;

    protected Store _cachedStore;
    protected Store _cachedStoreSync;
    protected int? _cachedActiveStoreScopeConfiguration;

    #endregion

    #region Ctor

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="genericAttributeService">Generic attribute service</param>
    /// <param name="httpContextAccessor">HTTP context accessor</param>
    /// <param name="storeRepository">Store repository</param>
    /// <param name="storeService">Store service</param>
    public WebStoreContext(IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        IRepository<Customer> customerRepository,
        IRepository<Store> storeRepository,
        IStoreService storeService)
    {
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
        _storeRepository = storeRepository;
        _storeService = storeService;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether current request belongs to the admin area
    /// </summary>
    /// <returns><c>true</c> if current request belongs to the admin area; otherwise <c>false</c></returns>
    protected virtual bool IsAdminAreaRequest()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return false;

        if (httpContext.GetRouteValue("area") is string area && area.Equals(AreaNames.ADMIN, StringComparison.InvariantCultureIgnoreCase))
            return true;

        var requestPath = httpContext.Request.Path.Value;
        return !string.IsNullOrEmpty(requestPath) && requestPath.StartsWith("/admin", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Gets a value indicating whether current request is a scheduled task execution request
    /// </summary>
    /// <returns><c>true</c> if current request is a scheduled task execution request; otherwise <c>false</c></returns>
    protected virtual bool IsScheduleTaskRequest()
    {
        return _httpContextAccessor.HttpContext?.Request?.Path
                   .Equals(new PathString($"/{NopTaskDefaults.ScheduleTaskPath}"), StringComparison.InvariantCultureIgnoreCase)
               ?? false;
    }

    /// <summary>
    /// Tries to resolve the current store from authenticated customer assignment
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the resolved store if any, otherwise <c>null</c>
    /// </returns>
    protected virtual async Task<Store> GetStoreByAuthenticatedCustomerAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true || IsScheduleTaskRequest())
            return null;

        //resolve lazily to avoid circular dependency in constructors
        var workContext = EngineContext.Current.Resolve<IWorkContext>();
        var currentCustomer = await workContext.GetCurrentCustomerAsync();
        if (currentCustomer?.RegisteredInStoreId <= 0)
            return null;

        if (!IsAdminAreaRequest())
            return await _storeService.GetStoreByIdAsync(currentCustomer.RegisteredInStoreId);

        var customerService = EngineContext.Current.Resolve<ICustomerService>();
        if (await customerService.IsAdminAsync(currentCustomer))
            return null;

        if (await customerService.IsInCustomerRoleAsync(currentCustomer, NopCustomerDefaults.StoreOwnersRoleName))
            return await _storeService.GetStoreByIdAsync(currentCustomer.RegisteredInStoreId);

        return null;
    }

    /// <summary>
    /// Gets the current store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<Store> GetCurrentStoreAsync()
    {
        if (_cachedStore != null)
            return _cachedStore;

        var store = await GetStoreByAuthenticatedCustomerAsync();

        if (store is null)
        {
            //fallback to default host-based resolution
            string host = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Host];
            var allStores = await _storeService.GetAllStoresAsync();
            store = allStores.FirstOrDefault(s => _storeService.ContainsHostValue(s, host)) ?? allStores.FirstOrDefault();
        }

        _cachedStore = store ?? throw new Exception("No store could be loaded");

        return _cachedStore;
    }

    /// <summary>
    /// Tries to resolve the current store from authenticated customer assignment (synchronous version).
    /// Uses claims from HttpContext.User to look up RegisteredInStoreId without async calls.
    /// IMPORTANT: This method must NOT resolve any services via EngineContext/DI to avoid circular
    /// dependencies (ICustomerService → ISettings → IStoreContext → this method).
    /// For admin area requests it returns null so the async path can handle role-based logic later.
    /// </summary>
    /// <param name="allStores">Pre-fetched list of all stores</param>
    /// <returns>The resolved store if any, otherwise <c>null</c></returns>
    protected virtual Store GetStoreByAuthenticatedCustomer(IList<Store> allStores)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true || IsScheduleTaskRequest())
            return null;

        //in admin area we cannot safely check roles without resolving services (circular dep),
        //so return null and let the async GetCurrentStoreAsync handle it with full role logic
        if (IsAdminAreaRequest())
            return null;

        //get email or username claim set during sign-in (see CookieAuthenticationService.SignInAsync)
        //we cannot inject CustomerSettings here because ISettings resolution itself depends on IStoreContext,
        //which would create a circular dependency. Instead, try email claim first then fallback to username.
        var emailClaim = httpContext.User.FindFirst(c =>
            c.Type == ClaimTypes.Email &&
            c.Issuer.Equals(NopAuthenticationDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase))?.Value;

        var usernameClaim = emailClaim is null
            ? httpContext.User.FindFirst(c =>
                c.Type == ClaimTypes.Name &&
                c.Issuer.Equals(NopAuthenticationDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase))?.Value
            : null;

        if (string.IsNullOrEmpty(emailClaim) && string.IsNullOrEmpty(usernameClaim))
            return null;

        //synchronous DB lookup – same pattern as _storeRepository.GetAll used below
        var registeredInStoreId = _customerRepository.GetAll(query =>
        {
            query = !string.IsNullOrEmpty(emailClaim)
                ? query.Where(c => c.Email == emailClaim)
                : query.Where(c => c.Username == usernameClaim);

            return query
                .Where(c => !c.Deleted && c.Active && c.RegisteredInStoreId > 0)
                .Select(c => new Customer { RegisteredInStoreId = c.RegisteredInStoreId });
        }, _ => default, includeDeleted: false)
        .FirstOrDefault()?.RegisteredInStoreId ?? 0;

        if (registeredInStoreId <= 0)
            return null;

        return allStores.FirstOrDefault(s => s.Id == registeredInStoreId);
    }

    /// <summary>
    /// Gets the current store (synchronous version).
    /// Uses a separate cache (_cachedStoreSync) so it does not interfere with the async path.
    /// </summary>
    public virtual Store GetCurrentStore()
    {
        if (_cachedStoreSync != null)
            return _cachedStoreSync;

        //we cannot call async methods here. otherwise, an application can hang. so it's a workaround to avoid that
        var allStores = _storeRepository.GetAll(query =>
        {
            return from s in query orderby s.DisplayOrder, s.Id select s;
        }, _ => default, includeDeleted: false);

        //try to resolve store from authenticated customer's RegisteredInStoreId
        var store = GetStoreByAuthenticatedCustomer(allStores);

        if (store is null)
        {
            //fallback to default host-based resolution
            string host = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Host];
            store = allStores.FirstOrDefault(s => _storeService.ContainsHostValue(s, host)) ?? allStores.FirstOrDefault();
        }

        _cachedStoreSync = store ?? throw new Exception("No store could be loaded");

        return _cachedStoreSync;
    }

    /// <summary>
    /// Gets active store scope configuration
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<int> GetActiveStoreScopeConfigurationAsync()
    {
        if (_cachedActiveStoreScopeConfiguration.HasValue)
            return _cachedActiveStoreScopeConfiguration.Value;

        //ensure that we have 2 (or more) stores
        if ((await _storeService.GetAllStoresAsync()).Count > 1)
        {
            //do not inject IWorkContext via constructor because it'll cause circular references
            var currentCustomer = await EngineContext.Current.Resolve<IWorkContext>().GetCurrentCustomerAsync();

            //try to get store identifier from attributes
            var storeId = await _genericAttributeService
                .GetAttributeAsync<int>(currentCustomer, NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute);

            _cachedActiveStoreScopeConfiguration = (await _storeService.GetStoreByIdAsync(storeId))?.Id ?? 0;
        }
        else
            _cachedActiveStoreScopeConfiguration = 0;

        return _cachedActiveStoreScopeConfiguration ?? 0;
    }

    #endregion
}
