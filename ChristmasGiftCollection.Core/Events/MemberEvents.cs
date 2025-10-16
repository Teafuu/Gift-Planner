using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Events;

/// <summary>
/// Base event for all member-related events
/// </summary>
public abstract class MemberEvent
{
    public Guid MemberId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a new member is added
/// </summary>
public class MemberAdded : MemberEvent
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Event raised when a member's information is updated
/// </summary>
public class MemberUpdated : MemberEvent
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Event raised when a member is removed from the system
/// </summary>
public class MemberRemoved : MemberEvent
{
    public string Reason { get; set; } = string.Empty;
}
