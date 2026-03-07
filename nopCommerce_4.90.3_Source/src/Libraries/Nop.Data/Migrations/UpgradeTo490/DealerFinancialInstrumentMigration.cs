using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 04:00:00", "Create DealerFinancialInstrument table")]
public class DealerFinancialInstrumentMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerCollection)).Exists())
            Create.TableFor<DealerCollection>();

        if (!Schema.Table(nameof(DealerFinancialInstrument)).Exists())
            Create.TableFor<DealerFinancialInstrument>();
    }
}
