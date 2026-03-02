using FluentMigrator;
using Nop.Core.Domain.Discounts;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-01 09:00:00", "Add discount store scope column")]
public class DiscountStoreScopeMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(Discount)).Exists())
            return;

        if (!Schema.Table(nameof(Discount)).Column(nameof(Discount.LimitedToStores)).Exists())
        {
            Alter.Table(nameof(Discount))
                .AddColumn(nameof(Discount.LimitedToStores))
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
        }
    }
}
