using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 09:30:00", "Create DealerTransactionAllocation table")]
public class DealerTransactionAllocationMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerTransactionAllocation)).Exists())
            Create.TableFor<DealerTransactionAllocation>();
    }
}
