namespace Nop.Core.Domain.Customers;

/// <summary>
/// Represents a dealer segment
/// </summary>
public partial class DealerSegment : BaseEntity
{
    /// <summary>
    /// Gets or sets the store identifier
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Gets or sets the segment name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the segment code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the optional description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the segment is active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the created date and time in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated date and time in UTC
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }
}
