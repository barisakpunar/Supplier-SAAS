using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-02-22 12:00:00", "Create DealerPaymentMethodMapping table")]
public class DealerPaymentMethodMappingMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerPaymentMethodMapping)).Exists())
            Create.TableFor<DealerPaymentMethodMapping>();
    }
}
