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
    /// Add a relationship between members
    /// </summary>
    Task AddRelationshipAsync(Guid fromMemberId, Guid toMemberId, RelationshipType type, CancellationToken cancellationToken = default);
}
