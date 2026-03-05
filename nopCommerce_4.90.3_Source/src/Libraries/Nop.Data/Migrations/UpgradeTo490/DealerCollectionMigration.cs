using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-05 23:30:00", "Create DealerCollection table")]
public class DealerCollectionMigration : ForwardOnlyMigration
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

        if (!Schema.Table(nameof(DealerCollection)).Exists())
            Create.TableFor<DealerCollection>();
    }
}
