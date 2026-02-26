namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents allowed payment methods for a dealer customer
/// </summary>
public partial class DealerPaymentMethodMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the payment method system name
    /// </summary>
    public string PaymentMethodSystemName { get; set; }
}
