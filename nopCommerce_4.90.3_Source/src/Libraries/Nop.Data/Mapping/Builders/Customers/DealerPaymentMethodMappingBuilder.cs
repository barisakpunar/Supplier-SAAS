using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Customers;

/// <summary>
/// Represents a dealer payment method mapping entity builder
/// </summary>
public partial class DealerPaymentMethodMappingBuilder : NopEntityBuilder<DealerPaymentMethodMapping>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerPaymentMethodMapping), nameof(DealerPaymentMethodMapping.CustomerId)))
            .AsInt32().ForeignKey<Customer>().PrimaryKey()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(DealerPaymentMethodMapping), nameof(DealerPaymentMethodMapping.PaymentMethodSystemName)))
            .AsString(400).NotNullable().PrimaryKey();
    }

    #endregion
}
