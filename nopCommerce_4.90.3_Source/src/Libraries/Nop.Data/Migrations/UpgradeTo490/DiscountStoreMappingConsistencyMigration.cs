using FluentMigrator;
using LinqToDB;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Stores;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopUpdateMigration("2026-03-01 09:45:00", "4.90", UpdateMigrationType.Data)]
public class DiscountStoreMappingConsistencyMigration : Migration
{
    private readonly INopDataProvider _dataProvider;

    public DiscountStoreMappingConsistencyMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(Discount)).Exists()
            || !Schema.Table(nameof(StoreMapping)).Exists()
            || !Schema.Table(nameof(Discount)).Column(nameof(Discount.LimitedToStores)).Exists())
            return;

        var mappedDiscountIds = _dataProvider.GetTable<StoreMapping>()
            .Where(mapping => mapping.EntityName == nameof(Discount))
            .Select(mapping => mapping.EntityId)
            .Distinct()
            .ToList();

        if (mappedDiscountIds.Any())
        {
            _dataProvider.GetTable<Discount>()
                .Where(discount => mappedDiscountIds.Contains(discount.Id) && !discount.LimitedToStores)
                .Set(discount => discount.LimitedToStores, true)
                .Update();

            _dataProvider.GetTable<Discount>()
                .Where(discount => discount.LimitedToStores && !mappedDiscountIds.Contains(discount.Id))
                .Set(discount => discount.LimitedToStores, false)
                .Update();

            return;
        }

        _dataProvider.GetTable<Discount>()
            .Where(discount => discount.LimitedToStores)
            .Set(discount => discount.LimitedToStores, false)
            .Update();
    }

    /// <summary>
    /// Collect the DOWN migration expressions
    /// </summary>
    public override void Down()
    {
    }
}
