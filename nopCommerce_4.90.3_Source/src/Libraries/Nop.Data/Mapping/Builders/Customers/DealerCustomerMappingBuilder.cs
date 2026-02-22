using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer-customer mapping entity builder
/// </summary>
public partial class DealerCustomerMappingBuilder : NopEntityBuilder<DealerCustomerMapping>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerCustomerMapping), nameof(DealerCustomerMapping.DealerId)))
            .AsInt32().ForeignKey<DealerInfo>().PrimaryKey()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerCustomerMapping), nameof(DealerCustomerMapping.CustomerId)))
            .AsInt32().ForeignKey<Customer>().PrimaryKey();
    }

    #endregion
}
