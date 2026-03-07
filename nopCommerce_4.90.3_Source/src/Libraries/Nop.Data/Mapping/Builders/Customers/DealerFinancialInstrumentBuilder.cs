using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer financial instrument entity builder
/// </summary>
public partial class DealerFinancialInstrumentBuilder : NopEntityBuilder<DealerFinancialInstrument>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerFinancialInstrument.DealerId)).AsInt32().NotNullable().ForeignKey<DealerInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(DealerFinancialInstrument.DealerCollectionId)).AsInt32().Nullable()
                .ForeignKey(NameCompatibilityManager.GetTableName(typeof(DealerCollection)), nameof(DealerCollection.Id))
            .WithColumn(nameof(DealerFinancialInstrument.CustomerId)).AsInt32().Nullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerFinancialInstrument.InstrumentTypeId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerFinancialInstrument.InstrumentStatusId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerFinancialInstrument.Amount)).AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerFinancialInstrument.InstrumentNo)).AsString(400).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.IssueDateUtc)).AsDateTime2().Nullable().Indexed()
            .WithColumn(nameof(DealerFinancialInstrument.DueDateUtc)).AsDateTime2().Nullable().Indexed()
            .WithColumn(nameof(DealerFinancialInstrument.BankName)).AsString(400).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.BranchName)).AsString(400).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.AccountNo)).AsString(400).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.DrawerName)).AsString(400).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.Note)).AsString(1000).Nullable()
            .WithColumn(nameof(DealerFinancialInstrument.CreatedByCustomerId)).AsInt32().NotNullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerFinancialInstrument.CreatedOnUtc)).AsDateTime2().NotNullable().Indexed()
            .WithColumn(nameof(DealerFinancialInstrument.UpdatedOnUtc)).AsDateTime2().Nullable();
    }
}
