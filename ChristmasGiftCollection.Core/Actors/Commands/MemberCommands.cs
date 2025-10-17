using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Actors.Commands;

/// <summary>
/// Base class for all member commands
/// </summary>
public record MemberCommand(Guid MemberId);

/// <summary>
/// Command to create a new member
/// </summary>
public record CreateMember(
    Guid MemberId,
    string Name,
    string? Email = null,
    DateTime? DateOfBirth = null,
    string? Notes = null
)  : MemberCommand(MemberId);

/// <summary>
/// Command to update member information
/// </summary>
public record UpdateMember(
    Guid MemberId,
    string? Name = null,
    string? Email = null,
    DateTime? DateOfBirth = null,
    string? Notes = null
)  : MemberCommand(MemberId);

/// <summary>
/// Command to delete a member
/// </summary>
public record DeleteMember(
    Guid MemberId,
    string Reason = "User deleted"
)  : MemberCommand(MemberId);

/// <summary>
/// Command to add a gift to a member
/// </summary>
public record AddGift(
    Guid MemberId,
    Guid GiftId,
    string Name,
    string? Description = null,
    decimal? Price = null,
    string? Url = null,
    GiftPriority Priority = GiftPriority.Medium
)  : MemberCommand(MemberId);

/// <summary>
/// Command to update a gift
/// </summary>
public record UpdateGift(
    Guid MemberId,
    Guid GiftId,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    string? Url = null,
    GiftPriority? Priority = null
)  : MemberCommand(MemberId);

/// <summary>
/// Command to take a gift
/// </summary>
public record TakeGift(
    Guid MemberId,
    Guid GiftId,
    Guid TakenByMemberId
)  : MemberCommand(MemberId);

/// <summary>
/// Command to release a gift
/// </summary>
public record ReleaseGift(
    Guid MemberId,
    Guid GiftId,
    Guid ReleasedByMemberId
)  : MemberCommand(MemberId);

/// <summary>
/// Command to remove a gift
/// </summary>
public record RemoveGift(
    Guid MemberId,
    Guid GiftId,
    string Reason = "User deleted"
)  : MemberCommand(MemberId);

/// <summary>
/// Command to add a relationship
/// </summary>
public record AddRelationship(
    Guid MemberId,
    Guid RelationshipId,
    Guid ToMemberId,
    RelationshipType Type
)  : MemberCommand(MemberId);

/// <summary>
/// Command to update a relationship
/// </summary>
public record UpdateRelationship(
    Guid MemberId,
    Guid RelationshipId,
    RelationshipType NewType
)  : MemberCommand(MemberId);

/// <summary>
/// Command to remove a relationship
/// </summary>
public record RemoveRelationship(
    Guid MemberId,
    Guid RelationshipId,
    Guid ToMemberId
)  : MemberCommand(MemberId);

/// <summary>
/// Command to set or change a member's PIN code
/// </summary>
public record SetPinCode(
    Guid MemberId,
    string PinCode
)  : MemberCommand(MemberId);

/// <summary>
/// Command to reorder gifts
/// </summary>
public record ReorderGifts(
    Guid MemberId,
    Dictionary<Guid, int> GiftOrders
)  : MemberCommand(MemberId);

/// <summary>
/// Query to get member state from events
/// </summary>
public record GetMemberState(Guid MemberId)  : MemberCommand(MemberId);

