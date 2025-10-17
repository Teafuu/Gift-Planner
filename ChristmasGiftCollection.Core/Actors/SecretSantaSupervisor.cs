using Akka.Actor;
using Akka.Event;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Repositories;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Supervisor actor that manages SecretSantaActor instances
/// Routes commands to the appropriate raffle actor based on RaffleId
/// </summary>
public class SecretSantaSupervisor : ReceiveActor
{
    private readonly IEventStoreRepository _eventStore;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<Guid, IActorRef> _raffleActors = new();

    public SecretSantaSupervisor(IEventStoreRepository eventStore)
    {
        _eventStore = eventStore;

        // Route all Secret Santa commands to the appropriate actor
        Receive<SecretSantaCommand>(cmd =>
        {
            var raffleActor = GetOrCreateRaffleActor(cmd.RaffleId);
            raffleActor.Forward(cmd);
        });

        // Handle requests to get all raffle IDs
        ReceiveAsync<GetAllSecretSantaIds>(async _ =>
        {
            // Get all stream names from EventStore
            var streamNames = await _eventStore.GetAllStreamNamesAsync();

            // Extract raffle IDs from stream names (format: "secretsanta-{guid}")
            var raffleIds = streamNames
                .Where(name => name.StartsWith("secretsanta-"))
                .Select(name => Guid.Parse(name.Split("secretsanta-")[1]))
                .ToList();

            Sender.Tell(raffleIds);
        });
    }

    private IActorRef GetOrCreateRaffleActor(Guid raffleId)
    {
        if (_raffleActors.TryGetValue(raffleId, out var actor))
        {
            return actor;
        }

        // Create new actor with EventStore repository
        var newActor = Context.ActorOf(
            SecretSantaActor.Props(raffleId, _eventStore),
            $"secretsanta-{raffleId}");

        _raffleActors[raffleId] = newActor;

        _log.Info($"Created new SecretSantaActor for raffle {raffleId}");

        return newActor;
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 10,
            withinTimeRange: TimeSpan.FromMinutes(1),
            localOnlyDecider: ex =>
            {
                _log.Error(ex, "SecretSantaActor failed");

                return ex switch
                {
                    InvalidOperationException => Directive.Resume,
                    ArgumentException => Directive.Resume,
                    _ => Directive.Restart
                };
            });
    }

    public static Props Props(IEventStoreRepository eventStore) =>
        Akka.Actor.Props.Create(() => new SecretSantaSupervisor(eventStore));
}
