using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-02-27 12:00:00", "Create DealerFinancialProfile table")]
public class DealerFinancialProfileMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerInfo)).Exists())
            Create.TableFor<DealerInfo>();

        if (!Schema.Table(nameof(DealerFinancialProfile)).Exists())
            Create.TableFor<DealerFinancialProfile>();
    }
}
