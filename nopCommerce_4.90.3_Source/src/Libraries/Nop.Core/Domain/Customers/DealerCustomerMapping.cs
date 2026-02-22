namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer-customer mapping class
/// </summary>
public partial class DealerCustomerMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }
}
