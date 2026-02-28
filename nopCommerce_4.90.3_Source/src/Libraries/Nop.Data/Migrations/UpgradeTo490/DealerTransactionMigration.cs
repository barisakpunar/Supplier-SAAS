using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-02-28 18:00:00", "Create DealerTransaction table")]
public class DealerTransactionMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerInfo)).Exists())
            Create.TableFor<DealerInfo>();

        if (!Schema.Table(nameof(DealerTransaction)).Exists())
            Create.TableFor<DealerTransaction>();
    }
}
