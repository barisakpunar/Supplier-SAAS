using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Web.Framework;

namespace Nop.Web.Framework.Mvc.Filters;

/// <summary>
/// Represents a filter attribute that limits admin area access for store owners to store owner pages
/// </summary>
public sealed class ValidateStoreOwnerAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public ValidateStoreOwnerAttribute(bool ignore = false) : base(typeof(ValidateStoreOwnerFilter))
    {
        IgnoreFilter = ignore;
        Arguments = [ignore];
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to ignore the execution of filter actions
    /// </summary>
    public bool IgnoreFilter { get; }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter limiting admin area access for store owners
    /// </summary>
    private class ValidateStoreOwnerFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        protected readonly bool _ignoreFilter;
        protected readonly ICustomerService _customerService;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ValidateStoreOwnerFilter(bool ignoreFilter, ICustomerService customerService, IWorkContext workContext)
        {
            _ignoreFilter = ignoreFilter;
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task ValidateStoreOwnerAsync(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether this filter has been overridden for the Action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<ValidateStoreOwnerAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if the current customer is a store owner)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsAdminAsync(customer))
                return;

            if (!await _customerService.IsInCustomerRoleAsync(customer, NopCustomerDefaults.StoreOwnersRoleName))
                return;

            var controller = context.RouteData.Values.TryGetValue("controller", out var controllerValue)
                ? controllerValue?.ToString()
                : string.Empty;
            var action = context.RouteData.Values.TryGetValue("action", out var actionValue)
                ? actionValue?.ToString()
                : string.Empty;

            if (controller?.Equals("StoreOwner", StringComparison.InvariantCultureIgnoreCase) ?? false)
                return;

            if ((controller?.Equals("Security", StringComparison.InvariantCultureIgnoreCase) ?? false)
                && (action?.Equals("AccessDenied", StringComparison.InvariantCultureIgnoreCase) ?? false))
                return;

            context.Result = new RedirectToActionResult("Index", "StoreOwner", new { area = AreaNames.ADMIN });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await ValidateStoreOwnerAsync(context);
        }

        #endregion
    }

    #endregion
}
