using FluentMigrator;
using LinqToDB;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopUpdateMigration("2026-02-22 00:00:00", "4.90", UpdateMigrationType.Data)]
public class StoreOwnerPermissionMappingMigration : Migration
{
    private readonly INopDataProvider _dataProvider;

    public StoreOwnerPermissionMappingMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        var storeOwnerRole = _dataProvider.GetTable<CustomerRole>()
            .FirstOrDefault(role => string.Compare(role.SystemName, NopCustomerDefaults.StoreOwnersRoleName, StringComparison.InvariantCultureIgnoreCase) == 0);

        if (storeOwnerRole == null)
            return;

        var targetPermissionSystemNames = new[]
        {
            "Security.AccessAdminPanel",
            "Security.AccessStoreOwnerPanel",
            "Customers.CustomersView",
            "Customers.CustomersCreateEditDelete",
            "Orders.OrdersView",
            "Orders.OrdersCreateEditDelete",
            "Orders.ShipmentsView",
            "Orders.ShipmentsCreateEditDelete",
            "Orders.ReturnRequestsView",
            "Orders.ReturnRequestsCreateEditDelete",
            "Catalog.ProductsView",
            "Catalog.ProductsCreateEditDelete",
            "Catalog.CategoriesView",
            "Catalog.CategoriesCreateEditDelete",
            "Catalog.ManufacturerView",
            "Catalog.ManufacturerCreateEditDelete",
            "Catalog.ProductReviewsView",
            "Catalog.ProductReviewsCreateEditDelete",
            "System.HtmlEditor.ManagePictures"
        };

        var permissionIds = _dataProvider.GetTable<PermissionRecord>()
            .Where(permission => targetPermissionSystemNames.Contains(permission.SystemName))
            .Select(permission => permission.Id)
            .ToList();

        if (!permissionIds.Any())
            return;

        var existingPermissionIds = _dataProvider.GetTable<PermissionRecordCustomerRoleMapping>()
            .Where(mapping => mapping.CustomerRoleId == storeOwnerRole.Id)
            .Select(mapping => mapping.PermissionRecordId)
            .ToHashSet();

        foreach (var permissionId in permissionIds)
        {
            if (existingPermissionIds.Contains(permissionId))
                continue;

            _dataProvider.InsertEntity(new PermissionRecordCustomerRoleMapping
            {
                CustomerRoleId = storeOwnerRole.Id,
                PermissionRecordId = permissionId
            });
        }
    }

    public override void Down()
    {
    }
}
