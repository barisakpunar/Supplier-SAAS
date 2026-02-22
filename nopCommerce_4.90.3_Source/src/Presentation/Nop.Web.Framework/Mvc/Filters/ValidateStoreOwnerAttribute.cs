using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework;

namespace Nop.Web.Framework.Mvc.Filters;

/// <summary>
/// Represents a filter attribute that applies store owner restrictions in the admin area
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
    /// Represents a filter applying store owner restrictions in the admin area
    /// </summary>
    private class ValidateStoreOwnerFilter : IAsyncAuthorizationFilter, IAsyncActionFilter
    {
        #region Fields

        protected readonly bool _ignoreFilter;
        protected readonly ICategoryService _categoryService;
        protected readonly ICustomerService _customerService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IOrderService _orderService;
        protected readonly IProductReviewService _productReviewService;
        protected readonly IProductService _productService;
        protected readonly IReturnRequestService _returnRequestService;
        protected readonly IShipmentService _shipmentService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IWorkContext _workContext;

        private static readonly HashSet<string> _allowedControllers = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "Home",
            "Product",
            "Category",
            "Manufacturer",
            "ProductReview",
            "Order",
            "ReturnRequest",
            "Customer",
            "SearchComplete",
            "Picture",
            "Download",
            "Common",
            "RoxyFileman",
            "StoreOwner",
            "Security"
        };

        private static readonly HashSet<string> _deniedReturnRequestActions = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "ReturnRequestReasonList",
            "ReturnRequestReasonCreate",
            "ReturnRequestReasonEdit",
            "ReturnRequestReasonDelete",
            "ReturnRequestActionList",
            "ReturnRequestActionCreate",
            "ReturnRequestActionEdit",
            "ReturnRequestActionDelete"
        };

        private static readonly HashSet<string> _orderActionsWithIdAsOrderId = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "CancelOrder",
            "CaptureOrder",
            "MarkOrderAsPaid",
            "RefundOrder",
            "RefundOrderOffline",
            "VoidOrder",
            "VoidOrderOffline",
            "PartiallyRefundOrderPopup",
            "ChangeOrderStatus",
            "Edit",
            "Delete",
            "EditCreditCardInfo",
            "EditOrderTotals",
            "EditShippingMethod",
            "AddShipment"
        };

        private static readonly HashSet<string> _orderActionsWithIdAsShipmentId = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "ShipmentDetails",
            "DeleteShipment",
            "SetAsShipped",
            "SetAsReadyForPickup",
            "SetAsDelivered",
            "PdfPackagingSlip"
        };

        private static readonly HashSet<string> _orderActionsWithIdAsOrderItemId = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "EditOrderItem",
            "DeleteOrderItem",
            "ResetDownloadCount",
            "ActivateDownloadItem",
            "UploadLicenseFilePopup"
        };

        #endregion

        #region Ctor

        public ValidateStoreOwnerFilter(bool ignoreFilter,
            ICategoryService categoryService,
            ICustomerService customerService,
            IManufacturerService manufacturerService,
            IOrderService orderService,
            IProductReviewService productReviewService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IShipmentService shipmentService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext)
        {
            _ignoreFilter = ignoreFilter;
            _categoryService = categoryService;
            _customerService = customerService;
            _manufacturerService = manufacturerService;
            _orderService = orderService;
            _productReviewService = productReviewService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _shipmentService = shipmentService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        private static string GetRouteValue(ActionDescriptor actionDescriptor, string key)
        {
            return actionDescriptor.RouteValues.TryGetValue(key, out var routeValue)
                ? routeValue
                : string.Empty;
        }

        private static int ParsePositiveInt(object value)
        {
            return value switch
            {
                int id when id > 0 => id,
                long id when id > 0 => (int)id,
                string text when int.TryParse(text, out var id) && id > 0 => id,
                _ => 0
            };
        }

        private static int TryGetFromRoute(FilterContext context, params string[] keyNames)
        {
            foreach (var key in keyNames)
            {
                if (!context.RouteData.Values.TryGetValue(key, out var value))
                    continue;

                var parsed = ParsePositiveInt(value);
                if (parsed > 0)
                    return parsed;
            }

            return 0;
        }

        private static int TryGetFromArguments(IDictionary<string, object> actionArguments, params string[] keyNames)
        {
            foreach (var key in keyNames)
            {
                if (!actionArguments.TryGetValue(key, out var value))
                    continue;

                var parsed = ParsePositiveInt(value);
                if (parsed > 0)
                    return parsed;
            }

            return 0;
        }

        private static int TryGetFromArgumentModels(IDictionary<string, object> actionArguments, params string[] propertyNames)
        {
            foreach (var argument in actionArguments.Values.Where(v => v is not null))
            {
                var type = argument.GetType();
                foreach (var propertyName in propertyNames)
                {
                    var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (property is null)
                        continue;

                    var parsed = ParsePositiveInt(property.GetValue(argument));
                    if (parsed > 0)
                        return parsed;
                }
            }

            return 0;
        }

        private static int TryGetEntityId(ActionExecutingContext context, params string[] names)
        {
            var id = TryGetFromArguments(context.ActionArguments, names);
            if (id > 0)
                return id;

            id = TryGetFromRoute(context, names);
            if (id > 0)
                return id;

            return TryGetFromArgumentModels(context.ActionArguments, names);
        }

        private static PropertyInfo GetPropertyInfo(object target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        }

        private static void TrySetIntProperty(object target, string propertyName, int value)
        {
            var property = GetPropertyInfo(target, propertyName);
            if (property is null || !property.CanWrite)
                return;

            if (property.PropertyType == typeof(int))
                property.SetValue(target, value);
            else if (property.PropertyType == typeof(int?))
                property.SetValue(target, (int?)value);
        }

        private static void TrySetStoreCollectionProperty(object target, int storeId)
        {
            var property = GetPropertyInfo(target, "SelectedStoreIds");
            if (property is null || !property.CanWrite)
                return;

            if (property.PropertyType == typeof(int[]))
            {
                property.SetValue(target, new[] { storeId });
                return;
            }

            if (typeof(IList<int>).IsAssignableFrom(property.PropertyType))
                property.SetValue(target, new List<int> { storeId });
        }

        private static void ApplyStoreScopeToActionArguments(ActionExecutingContext context, int managedStoreId)
        {
            foreach (var (key, value) in context.ActionArguments.ToList())
            {
                if (value is not null)
                {
                    TrySetIntProperty(value, "SearchStoreId", managedStoreId);
                    TrySetIntProperty(value, "StoreId", managedStoreId);
                    TrySetStoreCollectionProperty(value, managedStoreId);
                }

                if (!key.Equals("SearchStoreId", StringComparison.InvariantCultureIgnoreCase) &&
                    !key.Equals("StoreId", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                context.ActionArguments[key] = managedStoreId;
            }
        }

        private static bool IsIgnoreFilter(ActionDescriptor actionDescriptor, bool ignoreFilter)
        {
            var actionFilter = actionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<ValidateStoreOwnerAttribute>()
                .FirstOrDefault();

            return actionFilter?.IgnoreFilter ?? ignoreFilter;
        }

        private static IActionResult AccessDeniedResult(FilterContext context, string controller, string action)
        {
            return new RedirectToActionResult("AccessDenied", "Security", new
            {
                area = AreaNames.ADMIN,
                pageUrl = context.HttpContext.Request.Path.Value ?? string.Empty,
                pageSystemNameKey = $"{controller}.{action}"
            });
        }

        private async Task<(bool IsStoreOwner, int ManagedStoreId)> GetStoreOwnerContextAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer is null || await _customerService.IsAdminAsync(customer))
                return (false, 0);

            var isStoreOwner = await _customerService.IsInCustomerRoleAsync(customer, NopCustomerDefaults.StoreOwnersRoleName);
            if (!isStoreOwner)
                return (false, 0);

            return (true, customer.RegisteredInStoreId);
        }

        private async Task<bool> HasOrderAccessAsync(int orderId, int managedStoreId)
        {
            if (orderId <= 0)
                return false;

            var order = await _orderService.GetOrderByIdAsync(orderId);
            return order != null && order.StoreId == managedStoreId;
        }

        private async Task<bool> HasProductAccessAsync(int productId, int managedStoreId)
        {
            if (productId <= 0)
                return false;

            var product = await _productService.GetProductByIdAsync(productId);
            return product != null
                   && product.LimitedToStores
                   && await _storeMappingService.AuthorizeAsync(product, managedStoreId);
        }

        private async Task<bool> HasStoreOwnerEntityAccessAsync(ActionExecutingContext context, int managedStoreId)
        {
            var controller = GetRouteValue(context.ActionDescriptor, "controller");
            var action = GetRouteValue(context.ActionDescriptor, "action");

            switch (controller)
            {
                case "Product":
                    var productId = TryGetEntityId(context, "productId");
                    if (productId <= 0 && (action.Equals("Edit", StringComparison.InvariantCultureIgnoreCase) || action.Equals("Delete", StringComparison.InvariantCultureIgnoreCase)))
                        productId = TryGetEntityId(context, "id");
                    return productId <= 0 || await HasProductAccessAsync(productId, managedStoreId);

                case "Category":
                    if (!action.Equals("Edit", StringComparison.InvariantCultureIgnoreCase)
                        && !action.Equals("Delete", StringComparison.InvariantCultureIgnoreCase)
                        && !action.Equals("PreTranslate", StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    var categoryId = TryGetEntityId(context, "id", "itemId", "categoryId");
                    if (categoryId <= 0)
                        return true;

                    var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                    return category != null
                           && category.LimitedToStores
                           && await _storeMappingService.AuthorizeAsync(category, managedStoreId);

                case "Manufacturer":
                    if (!action.Equals("Edit", StringComparison.InvariantCultureIgnoreCase)
                        && !action.Equals("Delete", StringComparison.InvariantCultureIgnoreCase)
                        && !action.Equals("PreTranslate", StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    var manufacturerId = TryGetEntityId(context, "id", "itemId", "manufacturerId");
                    if (manufacturerId <= 0)
                        return true;

                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
                    return manufacturer != null
                           && manufacturer.LimitedToStores
                           && await _storeMappingService.AuthorizeAsync(manufacturer, managedStoreId);

                case "ProductReview":
                    var reviewId = TryGetEntityId(context, "id", "productReviewId");
                    if (reviewId <= 0)
                        return true;

                    var review = await _productReviewService.GetProductReviewByIdAsync(reviewId);
                    return review != null && await HasProductAccessAsync(review.ProductId, managedStoreId);

                case "Order":
                    var orderId = TryGetEntityId(context, "orderId");
                    if (orderId > 0)
                        return await HasOrderAccessAsync(orderId, managedStoreId);

                    if (_orderActionsWithIdAsOrderId.Contains(action))
                    {
                        orderId = TryGetEntityId(context, "id");
                        return orderId <= 0 || await HasOrderAccessAsync(orderId, managedStoreId);
                    }

                    if (_orderActionsWithIdAsShipmentId.Contains(action))
                    {
                        var shipmentId = TryGetEntityId(context, "shipmentId", "id");
                        if (shipmentId <= 0)
                            return true;

                        var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
                        return shipment != null && await HasOrderAccessAsync(shipment.OrderId, managedStoreId);
                    }

                    if (_orderActionsWithIdAsOrderItemId.Contains(action))
                    {
                        var orderItemId = TryGetEntityId(context, "orderItemId", "id");
                        if (orderItemId <= 0)
                            return true;

                        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
                        return orderItem != null && await HasOrderAccessAsync(orderItem.OrderId, managedStoreId);
                    }

                    return true;

                case "ReturnRequest":
                    var returnRequestId = TryGetEntityId(context, "returnRequestId", "id");
                    if (returnRequestId <= 0)
                        return true;

                    var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(returnRequestId);
                    return returnRequest != null && returnRequest.StoreId == managedStoreId;

                case "Customer":
                    var customerId = TryGetEntityId(context, "customerId", "id");
                    if (customerId <= 0)
                        return true;

                    var customer = await _customerService.GetCustomerByIdAsync(customerId);
                    return customer != null && customer.RegisteredInStoreId == managedStoreId;

                default:
                    return true;
            }
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
            ArgumentNullException.ThrowIfNull(context);

            if (!DataSettingsManager.IsDatabaseInstalled() || IsIgnoreFilter(context.ActionDescriptor, _ignoreFilter))
                return;

            var (isStoreOwner, managedStoreId) = await GetStoreOwnerContextAsync();
            if (!isStoreOwner)
                return;

            var controller = GetRouteValue(context.ActionDescriptor, "controller");
            var action = GetRouteValue(context.ActionDescriptor, "action");

            if (managedStoreId <= 0)
            {
                context.Result = AccessDeniedResult(context, controller, action);
                return;
            }

            if (!controller.Equals("Security", StringComparison.InvariantCultureIgnoreCase)
                && !_allowedControllers.Contains(controller))
            {
                context.Result = AccessDeniedResult(context, controller, action);
                return;
            }

            if (controller.Equals("Security", StringComparison.InvariantCultureIgnoreCase)
                && !action.Equals("AccessDenied", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = AccessDeniedResult(context, controller, action);
                return;
            }

            if (controller.Equals("ReturnRequest", StringComparison.InvariantCultureIgnoreCase)
                && _deniedReturnRequestActions.Contains(action))
            {
                context.Result = AccessDeniedResult(context, controller, action);
            }
        }

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete
        /// </summary>
        /// <param name="context">Action executing context</param>
        /// <param name="next">Action execution delegate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            if (!DataSettingsManager.IsDatabaseInstalled() || IsIgnoreFilter(context.ActionDescriptor, _ignoreFilter))
            {
                await next();
                return;
            }

            var (isStoreOwner, managedStoreId) = await GetStoreOwnerContextAsync();
            if (!isStoreOwner)
            {
                await next();
                return;
            }

            var controller = GetRouteValue(context.ActionDescriptor, "controller");
            var action = GetRouteValue(context.ActionDescriptor, "action");
            if (managedStoreId <= 0)
            {
                context.Result = AccessDeniedResult(context, controller, action);
                return;
            }

            ApplyStoreScopeToActionArguments(context, managedStoreId);

            if (!await HasStoreOwnerEntityAccessAsync(context, managedStoreId))
            {
                context.Result = AccessDeniedResult(context, controller, action);
                return;
            }

            await next();
        }

        #endregion
    }

    #endregion
}
