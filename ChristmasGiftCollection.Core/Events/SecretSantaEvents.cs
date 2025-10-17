namespace ChristmasGiftCollection.Core.Events;

/// <summary>
/// Base event for all Secret Santa raffle-related events
/// </summary>
public abstract class SecretSantaEvent
{
    public Guid RaffleId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a new Secret Santa raffle is created
/// </summary>
public class SecretSantaRaffleCreated : SecretSantaEvent
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new();
    public decimal? Budget { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Event raised when the Secret Santa raffle is executed (assignments made)
/// </summary>
public class SecretSantaRaffleExecuted : SecretSantaEvent
{
    public Dictionary<Guid, Guid> Assignments { get; set; } = new();
    // Key: Giver (MemberId), Value: Receiver (MemberId)
}

/// <summary>
/// Event raised when a Secret Santa raffle is cancelled
/// </summary>
public class SecretSantaRaffleCancelled : SecretSantaEvent
{
    public string Reason { get; set; } = string.Empty;
    public Guid CancelledByMemberId { get; set; }
}
