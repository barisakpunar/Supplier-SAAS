using FluentMigrator;
using Nop.Core.Domain.Customers;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 03:00:00", "Add document metadata to DealerCollection")]
public class DealerCollectionDocumentMetadataMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        var tableName = nameof(DealerCollection);
        if (!Schema.Table(tableName).Exists())
            return;

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.DocumentNo)).Exists())
            Alter.Table(tableName).AddColumn(nameof(DealerCollection.DocumentNo)).AsString(400).Nullable();

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.IssueDateUtc)).Exists())
            Alter.Table(tableName).AddColumn(nameof(DealerCollection.IssueDateUtc)).AsDateTime2().Nullable().Indexed();

        if (!Schema.Table(tableName).Column(nameof(DealerCollection.DueDateUtc)).Exists())
            Alter.Table(tableName).AddColumn(nameof(DealerCollection.DueDateUtc)).AsDateTime2().Nullable().Indexed();
    }
}
