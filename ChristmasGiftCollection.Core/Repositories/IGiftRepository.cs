using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// Repository interface for gift data access
/// </summary>
public interface IGiftRepository
{
    /// <summary>
    /// Get all gifts
    /// </summary>
    Task<IEnumerable<Gift>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get gifts for a specific member
    /// </summary>
    Task<IEnumerable<Gift>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a gift by ID
    /// </summary>
    Task<Gift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new gift
    /// </summary>
    Task<Gift> AddAsync(Gift gift, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing gift
    /// </summary>
    Task<Gift> UpdateAsync(Gift gift, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a gift
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a gift as taken by a member
    /// </summary>
    Task<Gift> TakeGiftAsync(Guid giftId, Guid takenByMemberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release a gift back to available status
    /// </summary>
    Task<Gift> ReleaseGiftAsync(Guid giftId, CancellationToken cancellationToken = default);
}
