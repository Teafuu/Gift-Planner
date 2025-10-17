using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Aggregate root for SecretSanta - built by replaying events
/// </summary>
public class SecretSantaAggregate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public List<Guid> ParticipantIds { get; private set; } = new();
    public decimal? Budget { get; private set; }
    public Guid CreatedByMemberId { get; private set; }
    public int Year { get; private set; }
    public bool IsExecuted { get; private set; }
    public Dictionary<Guid, Guid> Assignments { get; private set; } = new();
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Apply SecretSantaRaffleCreated event
    /// </summary>
    public void Apply(SecretSantaRaffleCreated evt)
    {
        Id = evt.RaffleId;
        Name = evt.Name;
        ParticipantIds = new List<Guid>(evt.ParticipantIds);
        Budget = evt.Budget;
        CreatedByMemberId = evt.CreatedByMemberId;
        Year = evt.Year;
        IsExecuted = false;
        IsCancelled = false;
        CreatedAt = evt.Timestamp;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply SecretSantaRaffleExecuted event
    /// </summary>
    public void Apply(SecretSantaRaffleExecuted evt)
    {
        IsExecuted = true;
        Assignments = new Dictionary<Guid, Guid>(evt.Assignments);
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply SecretSantaRaffleCancelled event
    /// </summary>
    public void Apply(SecretSantaRaffleCancelled evt)
    {
        IsCancelled = true;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Convert aggregate to read model
    /// </summary>
    public SecretSanta ToModel()
    {
        return new SecretSanta
        {
            Id = Id,
            Name = Name,
            ParticipantIds = new List<Guid>(ParticipantIds),
            Budget = Budget,
            CreatedByMemberId = CreatedByMemberId,
            Year = Year,
            IsExecuted = IsExecuted,
            Assignments = new Dictionary<Guid, Guid>(Assignments),
            IsCancelled = IsCancelled,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
