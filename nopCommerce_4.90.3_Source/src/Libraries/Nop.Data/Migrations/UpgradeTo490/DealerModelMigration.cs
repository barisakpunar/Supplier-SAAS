using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-02-22 12:30:00", "Dealer model tables")]
public class DealerModelMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerInfo)).Exists())
            Create.TableFor<DealerInfo>();

        if (!Schema.Table(nameof(DealerCustomerMapping)).Exists())
            Create.TableFor<DealerCustomerMapping>();
    }
}
