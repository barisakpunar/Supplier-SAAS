# StoreOwner SaaS Progress (nopCommerce)

## Current Branch
- `codex/storeowner-admin-foundation`

## Committed Milestones
1. `714c966` (main): initial project commit
2. `590e772` (`codex/tenant-store-context`): current store resolution based on authenticated customer store
3. `2c5b843` (`codex/storeowner-admin-foundation`): StoreOwner foundation (role/permission/filter/menu/controller base)

## In-Progress (Not Committed Yet)
- StoreOwner users see/admin-manage only their own store data on shared admin pages
- Major touched areas:
  - Store context in admin for StoreOwner:
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web.Framework/WebStoreContext.cs`
  - Authorization + request/data scope filter:
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web.Framework/Mvc/Filters/ValidateStoreOwnerAttribute.cs`
  - Menu pruning for StoreOwner:
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Infrastructure/StoreOwnerAdminMenuEventConsumer.cs`
  - Permission mappings:
    - `nopCommerce_4.90.3_Source/src/Libraries/Nop.Services/Security/DefaultPermissionConfigManager.cs`
  - Search/list scoping:
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Controllers/SearchCompleteController.cs`
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Factories/ProductModelFactory.cs`
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Factories/CategoryModelFactory.cs`
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Factories/ManufacturerModelFactory.cs`
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Factories/OrderModelFactory.cs`
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Factories/CustomerModelFactory.cs`
    - `nopCommerce_4.90.3_Source/src/Libraries/Nop.Services/Customers/ICustomerService.cs`
    - `nopCommerce_4.90.3_Source/src/Libraries/Nop.Services/Customers/CustomerService.cs`
  - Order access checks:
    - `nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Areas/Admin/Controllers/OrderController.cs`

## Environment/Build Note
- Build pipeline issue fixed for plugin cleanup host path:
  - `nopCommerce_4.90.3_Source/src/Build/ClearPluginAssemblies.proj`
- This fix avoids `dotnet: command not found` inside post-build `ClearPluginAssemblies`.

## Functional Target (V1)
- Administrator: unchanged, full panel access
- StoreOwner:
  - same admin infrastructure
  - restricted menu
  - list/search/action scope forced to own `RegisteredInStoreId`
  - entity-level access checks for key catalog/sales/customer paths

## Test Checklist (Manual)
1. Login as `Administrator`: all menus and data should remain global.
2. Login as `StoreOwner` (Store A):
   - Products/Categories/Manufacturers/Orders/Customers lists return only Store A records.
   - Direct URL to Store B item should be denied.
   - Search autocomplete should return only Store A products.
3. Repeat for a second StoreOwner (Store B) and verify isolation from Store A.

## Next Step
1. Validate all modified flows with manual tests.
2. Fix any compile/runtime issues from latest changes.
3. Commit in one focused commit for this V1 scope.
