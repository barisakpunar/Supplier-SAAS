using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer info entity builder
/// </summary>
public partial class DealerInfoBuilder : NopEntityBuilder<DealerInfo>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerInfo.Name)).AsString(400).NotNullable()
            .WithColumn(nameof(DealerInfo.StoreId)).AsInt32().ForeignKey<Store>(onDelete: Rule.None)
            .WithColumn(nameof(DealerInfo.Active)).AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn(nameof(DealerInfo.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(DealerInfo.UpdatedOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
