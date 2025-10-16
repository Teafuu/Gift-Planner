using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// Repository interface for relationship data access
/// </summary>
public interface IRelationshipRepository
{
    /// <summary>
    /// Get all relationships
    /// </summary>
    Task<IEnumerable<Relationship>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get relationships for a specific member (both from and to)
    /// </summary>
    Task<IEnumerable<Relationship>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a relationship by ID
    /// </summary>
    Task<Relationship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new relationship
    /// </summary>
    Task<Relationship> AddAsync(Relationship relationship, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing relationship
    /// </summary>
    Task<Relationship> UpdateAsync(Relationship relationship, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a relationship
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a relationship exists between two members
    /// </summary>
    Task<bool> ExistsAsync(Guid fromMemberId, Guid toMemberId, CancellationToken cancellationToken = default);
}
