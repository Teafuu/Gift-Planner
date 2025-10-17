using Akka.Actor;
using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Services;

/// <summary>
/// Actor-based implementation of ISecretSantaService using event sourcing
/// </summary>
public class ActorSecretSantaService(ActorSystem actorSystem) : ISecretSantaService
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    private IActorRef GetSupervisor() =>
        actorSystem.ActorSelection("/user/secretsanta-supervisor").ResolveOne(_timeout).Result;

    public async Task<IEnumerable<SecretSanta>> GetAllRafflesAsync(CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        // Get all raffle IDs
        var raffleIds = await supervisor.Ask<List<Guid>>(new GetAllSecretSantaIds(), _timeout);

        // Query each raffle's state
        var raffles = new List<SecretSanta>();
        foreach (var raffleId in raffleIds)
        {
            var raffle = await GetRaffleByIdAsync(raffleId, cancellationToken);
            if (raffle != null)
            {
                raffles.Add(raffle);
            }
        }

        return raffles;
    }

    public async Task<SecretSanta?> GetRaffleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        try
        {
            var raffle = await supervisor.Ask<SecretSanta?>(new GetSecretSantaState(id), _timeout);
            return raffle;
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<SecretSanta>> GetRafflesByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var allRaffles = await GetAllRafflesAsync(cancellationToken);
        return allRaffles.Where(r => r.Year == year && !r.IsCancelled);
    }

    public async Task<IEnumerable<SecretSanta>> GetRafflesForMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var allRaffles = await GetAllRafflesAsync(cancellationToken);
        return allRaffles.Where(r => r.ParticipantIds.Contains(memberId) && !r.IsCancelled && r.IsExecuted);
    }

    public async Task<Guid?> GetAssignmentForMemberAsync(Guid raffleId, Guid memberId, CancellationToken cancellationToken = default)
    {
        var raffle = await GetRaffleByIdAsync(raffleId, cancellationToken);

        if (raffle == null || !raffle.IsExecuted || raffle.IsCancelled)
        {
            return null;
        }

        return raffle.Assignments.TryGetValue(memberId, out var receiverId) ? receiverId : null;
    }

    public async Task<SecretSanta> CreateRaffleAsync(
        string name,
        List<Guid> participantIds,
        decimal? budget,
        Guid createdByMemberId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var raffleId = Guid.NewGuid();
        var supervisor = GetSupervisor();

        var command = new CreateSecretSantaRaffle(
            raffleId,
            name,
            participantIds,
            budget,
            createdByMemberId,
            year);

        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }

        // Return the newly created raffle
        var raffle = await GetRaffleByIdAsync(raffleId, cancellationToken);
        return raffle ?? throw new InvalidOperationException("Failed to create raffle");
    }

    public async Task ExecuteRaffleAsync(Guid raffleId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new ExecuteSecretSantaRaffle(raffleId);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task CancelRaffleAsync(Guid raffleId, string reason, Guid cancelledByMemberId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new CancelSecretSantaRaffle(raffleId, reason, cancelledByMemberId);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }
}
