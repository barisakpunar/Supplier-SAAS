using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer-segment mapping entity builder
/// </summary>
public partial class DealerSegmentMappingBuilder : NopEntityBuilder<DealerSegmentMapping>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerSegmentMapping), nameof(DealerSegmentMapping.DealerId)))
            .AsInt32().ForeignKey<DealerInfo>().PrimaryKey()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerSegmentMapping), nameof(DealerSegmentMapping.DealerSegmentId)))
            .AsInt32().ForeignKey<DealerSegment>().PrimaryKey();
    }

    #endregion
}
