using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer financial profile entity builder
/// </summary>
public partial class DealerFinancialProfileBuilder : NopEntityBuilder<DealerFinancialProfile>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DealerFinancialProfile.DealerId)).AsInt32().NotNullable().Unique().ForeignKey<DealerInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(DealerFinancialProfile.OpenAccountEnabled)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(DealerFinancialProfile.CreditLimit)).AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(DealerFinancialProfile.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(DealerFinancialProfile.UpdatedOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
