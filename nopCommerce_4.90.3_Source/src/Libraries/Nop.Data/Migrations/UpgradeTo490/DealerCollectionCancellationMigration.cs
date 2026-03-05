using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-06 00:30:00", "Add cancellation columns to DealerCollection")]
public class DealerCollectionCancellationMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        var tableName = nameof(DealerCollection);
        if (!Schema.Table(tableName).Exists())
            return;

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.CancelledDealerTransactionId)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(DealerCollection.CancelledDealerTransactionId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerTransaction)), nameof(DealerTransaction.Id));
        }

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.CancelledByCustomerId)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(DealerCollection.CancelledByCustomerId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(Customer)), nameof(Customer.Id));
        }

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.CancelledOnUtc)).Exists())
            Alter.Table(tableName).AddColumn(nameof(DealerCollection.CancelledOnUtc)).AsDateTime2().Nullable().Indexed();
    }
}
