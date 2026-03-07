using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer finance audit log entity builder
/// </summary>
public partial class DealerFinanceAuditLogBuilder : NopEntityBuilder<DealerFinanceAuditLog>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerFinanceAuditLog.DealerId)).AsInt32().NotNullable().ForeignKey<DealerInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(DealerFinanceAuditLog.EntityTypeId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerFinanceAuditLog.DealerCollectionId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerCollection)), nameof(DealerCollection.Id))
            .WithColumn(nameof(DealerFinanceAuditLog.DealerFinancialInstrumentId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerFinancialInstrument)), nameof(DealerFinancialInstrument.Id))
            .WithColumn(nameof(DealerFinanceAuditLog.ActionTypeId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerFinanceAuditLog.StatusBeforeId)).AsInt32().Nullable()
            .WithColumn(nameof(DealerFinanceAuditLog.StatusAfterId)).AsInt32().Nullable()
            .WithColumn(nameof(DealerFinanceAuditLog.Note)).AsString(1000).Nullable()
            .WithColumn(nameof(DealerFinanceAuditLog.PerformedByCustomerId)).AsInt32().Nullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerFinanceAuditLog.PerformedOnUtc)).AsDateTime2().NotNullable().Indexed();
    }
}
