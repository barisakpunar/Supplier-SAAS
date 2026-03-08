using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-08 21:30:00", "Create DealerSegment tables")]
public class DealerSegmentMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(DealerSegment)).Exists())
            Create.TableFor<DealerSegment>();

        if (!Schema.Table(nameof(DealerSegmentMapping)).Exists())
            Create.TableFor<DealerSegmentMapping>();
    }
}
