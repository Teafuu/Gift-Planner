using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Services;

/// <summary>
/// Service for managing Secret Santa raffles
/// </summary>
public interface ISecretSantaService
{
    /// <summary>
    /// Get all Secret Santa raffles
    /// </summary>
    Task<IEnumerable<SecretSanta>> GetAllRafflesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific raffle by ID
    /// </summary>
    Task<SecretSanta?> GetRaffleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all raffles for a specific year
    /// </summary>
    Task<IEnumerable<SecretSanta>> GetRafflesByYearAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all raffles that a member is participating in
    /// </summary>
    Task<IEnumerable<SecretSanta>> GetRafflesForMemberAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get who a member should give a gift to in a specific raffle
    /// </summary>
    Task<Guid?> GetAssignmentForMemberAsync(Guid raffleId, Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new Secret Santa raffle
    /// </summary>
    Task<SecretSanta> CreateRaffleAsync(
        string name,
        List<Guid> participantIds,
        decimal? budget,
        Guid createdByMemberId,
        int year,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a raffle (perform the random assignment)
    /// </summary>
    Task ExecuteRaffleAsync(Guid raffleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a raffle
    /// </summary>
    Task CancelRaffleAsync(Guid raffleId, string reason, Guid cancelledByMemberId, CancellationToken cancellationToken = default);
}
