using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// Repository interface for member data access
/// </summary>
public interface IMemberRepository
{
    /// <summary>
    /// Get all members
    /// </summary>
    Task<IEnumerable<Member>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a member by ID
    /// </summary>
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new member
    /// </summary>
    Task<Member> AddAsync(Member member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing member
    /// </summary>
    Task<Member> UpdateAsync(Member member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a member
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a member exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
