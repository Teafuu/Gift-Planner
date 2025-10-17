namespace ChristmasGiftCollection.Core.Models;

/// <summary>
/// Represents a family member in the gift collection system
/// </summary>
public class Member
{
    /// <summary>
    /// Unique identifier for the member
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Full name of the family member
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth of the family member
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// 4-digit PIN code for member authentication (plain text)
    /// </summary>
    public string? PinCode { get; set; }

    /// <summary>
    /// Whether this member has administrative privileges
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// List of gift ideas associated with this member
    /// </summary>
    public List<Gift> Gifts { get; set; } = new();

    /// <summary>
    /// Relationships where this member is the source (e.g., parent of, spouse of)
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new();

    /// <summary>
    /// When this member was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this member was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
