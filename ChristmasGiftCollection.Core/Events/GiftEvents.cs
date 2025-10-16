using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Events;

/// <summary>
/// Base event for all gift-related events
/// </summary>
public abstract class GiftEvent
{
    public Guid GiftId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a new gift is added for a member
/// </summary>
public class GiftAdded : GiftEvent
{
    public Guid MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GiftPriority Priority { get; set; }
}

/// <summary>
/// Event raised when gift information is updated
/// </summary>
public class GiftUpdated : GiftEvent
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public GiftPriority? Priority { get; set; }
}

/// <summary>
/// Event raised when someone claims/takes a gift
/// </summary>
public class GiftTaken : GiftEvent
{
    public Guid TakenByMemberId { get; set; }
}

/// <summary>
/// Event raised when a claimed gift is released back to available
/// </summary>
public class GiftReleased : GiftEvent
{
    public Guid ReleasedByMemberId { get; set; }
}

/// <summary>
/// Event raised when a gift status changes
/// </summary>
public class GiftStatusChanged : GiftEvent
{
    public GiftStatus NewStatus { get; set; }
    public GiftStatus OldStatus { get; set; }
}

/// <summary>
/// Event raised when a gift is removed
/// </summary>
public class GiftRemoved : GiftEvent
{
    public Guid MemberId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
