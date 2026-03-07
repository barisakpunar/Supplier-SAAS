using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer transaction allocation entity builder
/// </summary>
public partial class DealerTransactionAllocationBuilder : NopEntityBuilder<DealerTransactionAllocation>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerTransactionAllocation.DealerId)).AsInt32().NotNullable().ForeignKey<DealerInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(DealerTransactionAllocation.DealerCollectionId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerCollection)), nameof(DealerCollection.Id))
            .WithColumn(nameof(DealerTransactionAllocation.CreditDealerTransactionId)).AsInt32().NotNullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerTransaction)), nameof(DealerTransaction.Id))
            .WithColumn(nameof(DealerTransactionAllocation.DebitDealerTransactionId)).AsInt32().NotNullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerTransaction)), nameof(DealerTransaction.Id))
            .WithColumn(nameof(DealerTransactionAllocation.Amount)).AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerTransactionAllocation.CreatedByCustomerId)).AsInt32().NotNullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerTransactionAllocation.CreatedOnUtc)).AsDateTime2().NotNullable().Indexed()
            .WithColumn(nameof(DealerTransactionAllocation.CancelledByCustomerId)).AsInt32().Nullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerTransactionAllocation.CancelledOnUtc)).AsDateTime2().Nullable().Indexed();
    }
}
