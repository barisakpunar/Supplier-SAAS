using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 08:30:00", "Create DealerFinanceAuditLog table")]
public class DealerFinanceAuditLogMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerFinanceAuditLog)).Exists())
            Create.TableFor<DealerFinanceAuditLog>();
    }
}
