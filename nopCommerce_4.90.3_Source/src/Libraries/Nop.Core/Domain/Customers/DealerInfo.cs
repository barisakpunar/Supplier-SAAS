namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents dealer account information
/// </summary>
public partial class DealerInfo : BaseEntity
{
    /// <summary>
    /// Gets or sets the dealer display name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the managed store identifier
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the dealer is active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated date and time in UTC
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }
}
