using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 14:30:00", "Add DealerFinancialInstrumentId to DealerCollection")]
public class DealerCollectionFinancialInstrumentLinkMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        var tableName = nameof(DealerCollection);
        if (!Schema.Table(tableName).Exists())
            return;

        if (Schema.Table(tableName).Column(nameof(DealerCollection.DealerFinancialInstrumentId)).Exists())
            return;

        Alter.Table(tableName)
            .AddColumn(nameof(DealerCollection.DealerFinancialInstrumentId)).AsInt32().Nullable()
            .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerFinancialInstrument)), nameof(DealerFinancialInstrument.Id));
    }
}
