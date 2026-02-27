namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents dealer financial settings profile
/// </summary>
public partial class DealerFinancialProfile : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether open account payment method is enabled
    /// </summary>
    public bool OpenAccountEnabled { get; set; }

    /// <summary>
    /// Gets or sets dealer credit limit for open account
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated date and time in UTC
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }
}
