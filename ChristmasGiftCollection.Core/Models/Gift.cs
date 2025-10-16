namespace ChristmasGiftCollection.Core.Models;

/// <summary>
/// Represents a gift idea for a family member
/// </summary>
public class Gift
{
    /// <summary>
    /// Unique identifier for the gift
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The member this gift is for
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Navigation property for the member
    /// </summary>
    public Member? Member { get; set; }

    /// <summary>
    /// Name or title of the gift
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the gift
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estimated or actual price
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Optional URL to purchase or view the gift
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Current status of the gift
    /// </summary>
    public GiftStatus Status { get; set; }

    /// <summary>
    /// Who claimed/purchased this gift (if taken)
    /// </summary>
    public Guid? TakenByMemberId { get; set; }

    /// <summary>
    /// Navigation property for who took the gift
    /// </summary>
    public Member? TakenByMember { get; set; }

    /// <summary>
    /// When the gift was claimed/taken
    /// </summary>
    public DateTime? TakenAt { get; set; }

    /// <summary>
    /// Priority level for this gift
    /// </summary>
    public GiftPriority Priority { get; set; }

    /// <summary>
    /// When this gift was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this gift was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Status of a gift in the collection
/// </summary>
public enum GiftStatus
{
    Available,
    Taken
}

/// <summary>
/// Priority level for gift ideas
/// </summary>
public enum GiftPriority
{
    Low,
    Medium,
    High
}
