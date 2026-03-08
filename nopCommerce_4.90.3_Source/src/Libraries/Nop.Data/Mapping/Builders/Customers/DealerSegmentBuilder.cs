using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer segment entity builder
/// </summary>
public partial class DealerSegmentBuilder : NopEntityBuilder<DealerSegment>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerSegment.StoreId)).AsInt32().ForeignKey<Store>(onDelete: Rule.None)
            .WithColumn(nameof(DealerSegment.Name)).AsString(400).NotNullable()
            .WithColumn(nameof(DealerSegment.Code)).AsString(100).NotNullable()
            .WithColumn(nameof(DealerSegment.Description)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(DealerSegment.Active)).AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn(nameof(DealerSegment.DisplayOrder)).AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerSegment.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(DealerSegment.UpdatedOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
