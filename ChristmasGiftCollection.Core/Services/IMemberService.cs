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
    Task<Member> CreateMemberAsync(string name, MemberType type, string? email = null,
        DateTime? dateOfBirth = null, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing member
    /// </summary>
    Task<Member> UpdateMemberAsync(Guid id, string? name = null, MemberType? type = null,
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
}
