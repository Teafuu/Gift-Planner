using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Events;

/// <summary>
/// Base event for all relationship-related events
/// </summary>
public abstract class RelationshipEvent
{
    public Guid RelationshipId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a new relationship is created between members
/// </summary>
public class RelationshipAdded : RelationshipEvent
{
    public Guid FromMemberId { get; set; }
    public Guid ToMemberId { get; set; }
    public RelationshipType Type { get; set; }
}

/// <summary>
/// Event raised when a relationship type is updated
/// </summary>
public class RelationshipUpdated : RelationshipEvent
{
    public RelationshipType NewType { get; set; }
}

/// <summary>
/// Event raised when a relationship is removed
/// </summary>
public class RelationshipRemoved : RelationshipEvent
{
    public Guid FromMemberId { get; set; }
    public Guid ToMemberId { get; set; }
}
