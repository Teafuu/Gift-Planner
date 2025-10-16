namespace ChristmasGiftCollection.Core.Models;

/// <summary>
/// Represents a relationship between two family members
/// </summary>
public class Relationship
{
    /// <summary>
    /// Unique identifier for the relationship
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The source member (e.g., the parent in "parent of")
    /// </summary>
    public Guid FromMemberId { get; set; }

    /// <summary>
    /// Navigation property for the source member
    /// </summary>
    public Member? FromMember { get; set; }

    /// <summary>
    /// The target member (e.g., the child in "parent of")
    /// </summary>
    public Guid ToMemberId { get; set; }

    /// <summary>
    /// Navigation property for the target member
    /// </summary>
    public Member? ToMember { get; set; }

    /// <summary>
    /// The type of relationship
    /// </summary>
    public RelationshipType Type { get; set; }

    /// <summary>
    /// When this relationship was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Types of relationships between family members
/// </summary>
public enum RelationshipType
{
    ChildOf,
    PartnerOf
}
