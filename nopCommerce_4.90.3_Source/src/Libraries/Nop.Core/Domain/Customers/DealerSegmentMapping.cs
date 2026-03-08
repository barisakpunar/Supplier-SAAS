namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer-segment mapping class
/// </summary>
public partial class DealerSegmentMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer identifier
    /// </summary>
    public int DealerId { get; set; }

    /// <summary>
    /// Gets or sets the dealer segment identifier
    /// </summary>
    public int DealerSegmentId { get; set; }
}
