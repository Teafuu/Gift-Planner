namespace ChristmasGiftCollection.Core.Models;

/// <summary>
/// Represents a Secret Santa raffle
/// </summary>
public class SecretSanta
{
    /// <summary>
    /// Unique identifier for the raffle
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name/description of the raffle
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of member IDs participating in the raffle
    /// </summary>
    public List<Guid> ParticipantIds { get; set; } = new();

    /// <summary>
    /// Optional budget for gifts
    /// </summary>
    public decimal? Budget { get; set; }

    /// <summary>
    /// ID of the admin who created the raffle
    /// </summary>
    public Guid CreatedByMemberId { get; set; }

    /// <summary>
    /// Year of the raffle
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Whether the raffle has been executed (assignments made)
    /// </summary>
    public bool IsExecuted { get; set; }

    /// <summary>
    /// Assignments: Key = Giver (MemberId), Value = Receiver (MemberId)
    /// Each person gives to another person
    /// </summary>
    public Dictionary<Guid, Guid> Assignments { get; set; } = new();

    /// <summary>
    /// Whether the raffle has been cancelled
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// When this raffle was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this raffle was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
