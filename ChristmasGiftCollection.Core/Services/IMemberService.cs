using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Services;

/// <summary>
/// Service interface for member management operations
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// Get all members
    /// </summary>
    Task<IEnumerable<Member>> GetAllMembersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a member by ID
    /// </summary>
    Task<Member?> GetMemberByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new member
    /// </summary>
    Task<Member> CreateMemberAsync(string name, string? email = null,
        DateTime? dateOfBirth = null, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing member
    /// </summary>
    Task<Member> UpdateMemberAsync(Guid id, string? name = null,
        string? email = null, DateTime? dateOfBirth = null, string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a member
    /// </summary>
    Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get members for graph visualization
    /// </summary>
    Task<object> GetMembersForGraphAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a gift to a member
    /// </summary>
    Task AddGiftAsync(Guid memberId, string name, string? description, GiftPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Take a gift (mark as taken by someone)
    /// </summary>
    Task TakeGiftAsync(Guid memberId, Guid giftId, Guid takenByMemberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release a gift (mark as available again)
    /// </summary>
    Task ReleaseGiftAsync(Guid memberId, Guid giftId, Guid releasedByMemberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorder gifts for a member
    /// </summary>
    Task ReorderGiftsAsync(Guid memberId, Dictionary<Guid, int> giftOrders, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a gift from a member's wishlist
    /// </summary>
    Task RemoveGiftAsync(Guid memberId, Guid giftId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a relationship between members
    /// </summary>
    Task AddRelationshipAsync(Guid fromMemberId, Guid toMemberId, RelationshipType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a relationship between members
    /// </summary>
    Task RemoveRelationshipAsync(Guid fromMemberId, Guid relationshipId, Guid toMemberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set or update a member's PIN code
    /// </summary>
    Task SetPinCodeAsync(Guid memberId, string pinCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if currentMember can edit targetMember's gifts (is admin, is self, or is ancestor)
    /// </summary>
    Task<bool> CanEditGiftsAsync(Guid currentMemberId, Guid targetMemberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all descendants (children, grandchildren, etc.) of a member
    /// </summary>
    Task<List<Guid>> GetAllDescendantsAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all gifts taken by a specific member across all wishlists
    /// Returns tuples of (gift, owner) for easy display
    /// </summary>
    Task<List<(Gift gift, Member owner)>> GetGiftsTakenByMemberAsync(Guid takenByMemberId, CancellationToken cancellationToken = default);
}
