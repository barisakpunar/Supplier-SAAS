using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer collection entity builder
/// </summary>
public partial class DealerCollectionBuilder : NopEntityBuilder<DealerCollection>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerCollection.DealerId)).AsInt32().NotNullable().ForeignKey<DealerInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(DealerCollection.CustomerId)).AsInt32().Nullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerCollection.DealerTransactionId)).AsInt32().Nullable().ForeignKey<DealerTransaction>(onDelete: Rule.None)
            .WithColumn(nameof(DealerCollection.CollectionMethodId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerCollection.CollectionStatusId)).AsInt32().NotNullable().Indexed()
            .WithColumn(nameof(DealerCollection.Amount)).AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerCollection.CollectionDateUtc)).AsDateTime2().NotNullable().Indexed()
            .WithColumn(nameof(DealerCollection.ReferenceNo)).AsString(400).Nullable()
            .WithColumn(nameof(DealerCollection.Note)).AsString(1000).Nullable()
            .WithColumn(nameof(DealerCollection.CreatedByCustomerId)).AsInt32().NotNullable().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(DealerCollection.CreatedOnUtc)).AsDateTime2().NotNullable().Indexed()
            .WithColumn(nameof(DealerCollection.UpdatedOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
