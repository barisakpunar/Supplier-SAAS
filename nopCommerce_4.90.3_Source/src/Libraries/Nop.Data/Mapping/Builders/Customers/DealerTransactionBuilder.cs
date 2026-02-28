using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer transaction entity builder
/// </summary>
public partial class DealerTransactionBuilder : NopEntityBuilder<DealerTransaction>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerTransaction.DealerId)).AsInt32().NotNullable().ForeignKey<DealerInfo>(onDelete: Rule.Cascade).Indexed()
            .WithColumn(nameof(DealerTransaction.OrderId)).AsInt32().Nullable().ForeignKey<Order>(onDelete: Rule.None).Indexed()
            .WithColumn(nameof(DealerTransaction.CustomerId)).AsInt32().Nullable().ForeignKey<Customer>(onDelete: Rule.None).Indexed()
            .WithColumn(nameof(DealerTransaction.TransactionTypeId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerTransaction.DirectionId)).AsInt32().NotNullable()
            .WithColumn(nameof(DealerTransaction.Amount)).AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerTransaction.Note)).AsString(1000).Nullable()
            .WithColumn(nameof(DealerTransaction.CreatedOnUtc)).AsDateTime2().NotNullable().Indexed();
    }

    #endregion
}
