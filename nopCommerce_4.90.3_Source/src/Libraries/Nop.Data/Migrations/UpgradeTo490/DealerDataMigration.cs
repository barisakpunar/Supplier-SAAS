using FluentMigrator;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopUpdateMigration("2026-02-22 12:40:00", "4.90", UpdateMigrationType.Data)]
public class DealerDataMigration : Migration
{
    private readonly INopDataProvider _dataProvider;

    public DealerDataMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerInfo)).Exists() || !Schema.Table(nameof(DealerCustomerMapping)).Exists())
            return;

        var fallbackStoreId = _dataProvider.GetTable<Store>()
            .OrderBy(store => store.Id)
            .Select(store => store.Id)
            .FirstOrDefault();

        if (fallbackStoreId <= 0)
            return;

        var dealerByCustomerId = _dataProvider.GetTable<DealerCustomerMapping>()
            .ToDictionary(mapping => mapping.CustomerId, mapping => mapping.DealerId);

        var storeOwnerRoleId = _dataProvider.GetTable<CustomerRole>()
            .Where(role => string.Compare(role.SystemName, NopCustomerDefaults.StoreOwnersRoleName, StringComparison.InvariantCultureIgnoreCase) == 0)
            .Select(role => role.Id)
            .FirstOrDefault();

        if (storeOwnerRoleId > 0)
        {
            var storeOwnerCustomerIds = _dataProvider.GetTable<CustomerCustomerRoleMapping>()
                .Where(mapping => mapping.CustomerRoleId == storeOwnerRoleId)
                .Select(mapping => mapping.CustomerId)
                .Distinct()
                .ToList();

            foreach (var customerId in storeOwnerCustomerIds)
                EnsureDealerForCustomer(customerId, fallbackStoreId, dealerByCustomerId);
        }

        if (!Schema.Table(nameof(DealerPaymentMethodMapping)).Exists()
            || !Schema.Table(nameof(DealerPaymentMethodMapping)).Column("CustomerId").Exists()
            || !Schema.Table(nameof(DealerPaymentMethodMapping)).Column(nameof(DealerPaymentMethodMapping.DealerId)).Exists())
            return;

        var paymentCustomerIds = _dataProvider.GetTable<LegacyDealerPaymentMethodMapping>()
            .Where(mapping => mapping.CustomerId > 0)
            .Select(mapping => mapping.CustomerId)
            .Distinct()
            .ToList();

        foreach (var customerId in paymentCustomerIds)
        {
            var dealerId = EnsureDealerForCustomer(customerId, fallbackStoreId, dealerByCustomerId);
            if (dealerId <= 0)
                continue;

            Execute.Sql(
                $"UPDATE {nameof(DealerPaymentMethodMapping)} " +
                $"SET {nameof(DealerPaymentMethodMapping.DealerId)} = {dealerId} " +
                $"WHERE CustomerId = {customerId} AND ({nameof(DealerPaymentMethodMapping.DealerId)} IS NULL OR {nameof(DealerPaymentMethodMapping.DealerId)} = 0)");
        }
    }

    /// <summary>
    /// Collect the DOWN migration expressions
    /// </summary>
    public override void Down()
    {
    }

    private int EnsureDealerForCustomer(int customerId, int fallbackStoreId, IDictionary<int, int> dealerByCustomerId)
    {
        if (customerId <= 0)
            return 0;

        if (dealerByCustomerId.TryGetValue(customerId, out var existingDealerId))
            return existingDealerId;

        var customer = _dataProvider.GetTable<Customer>()
            .FirstOrDefault(item => item.Id == customerId);

        if (customer == null)
            return 0;

        var dealerName = !string.IsNullOrWhiteSpace(customer.Company)
            ? customer.Company
            : !string.IsNullOrWhiteSpace(customer.Email)
                ? customer.Email
                : !string.IsNullOrWhiteSpace(customer.Username)
                    ? customer.Username
                    : $"Dealer #{customer.Id}";

        var dealer = _dataProvider.InsertEntity(new DealerInfo
        {
            Name = dealerName,
            StoreId = customer.RegisteredInStoreId > 0 ? customer.RegisteredInStoreId : fallbackStoreId,
            Active = true,
            CreatedOnUtc = DateTime.UtcNow
        });

        _dataProvider.InsertEntity(new DealerCustomerMapping
        {
            DealerId = dealer.Id,
            CustomerId = customer.Id
        });

        dealerByCustomerId[customer.Id] = dealer.Id;
        return dealer.Id;
    }

    private sealed class LegacyDealerPaymentMethodMapping : BaseEntity
    {
        public int CustomerId { get; set; }
    }
}
