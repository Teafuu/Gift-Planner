namespace ChristmasGiftCollection.Core.Actors.Commands;

/// <summary>
/// Base class for all Secret Santa commands
/// </summary>
public record SecretSantaCommand(Guid RaffleId);

/// <summary>
/// Command to create a new Secret Santa raffle
/// </summary>
public record CreateSecretSantaRaffle(
    Guid RaffleId,
    string Name,
    List<Guid> ParticipantIds,
    decimal? Budget,
    Guid CreatedByMemberId,
    int Year
) : SecretSantaCommand(RaffleId);

/// <summary>
/// Command to execute a Secret Santa raffle (perform the random assignment)
/// </summary>
public record ExecuteSecretSantaRaffle(
    Guid RaffleId
) : SecretSantaCommand(RaffleId);

/// <summary>
/// Command to cancel a Secret Santa raffle
/// </summary>
public record CancelSecretSantaRaffle(
    Guid RaffleId,
    string Reason,
    Guid CancelledByMemberId
) : SecretSantaCommand(RaffleId);

/// <summary>
/// Query to get the state of a Secret Santa raffle
/// </summary>
public record GetSecretSantaState(Guid RaffleId) : SecretSantaCommand(RaffleId);

/// <summary>
/// Query to get all Secret Santa raffle IDs
/// </summary>
public record GetAllSecretSantaIds;
