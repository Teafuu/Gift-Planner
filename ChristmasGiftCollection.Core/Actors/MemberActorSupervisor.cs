using Akka.Actor;
using Akka.Event;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Repositories;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Supervisor actor that manages MemberActor instances
/// Routes commands to the appropriate member actor based on MemberId
/// </summary>
public class MemberActorSupervisor : ReceiveActor
{
    private readonly IEventStoreRepository _eventStore;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<Guid, IActorRef> _memberActors = new();

    public MemberActorSupervisor(IEventStoreRepository eventStore)
    {
        _eventStore = eventStore;

        // Route all member commands to the appropriate actor
        Receive<MemberCommand>(cmd =>
        {
            var memberActor = GetOrCreateMemberActor(cmd.MemberId);
            memberActor.Forward(cmd);
        });

        // Handle requests to get all member IDs
        ReceiveAsync<GetAllMemberIds>(async _ =>
        {
            // Get all stream names from EventStore
            var streamNames = await _eventStore.GetAllStreamNamesAsync();

            // Extract member IDs from stream names (format: "member-{guid}")
            var memberIds = streamNames
                .Where(name => !name.Contains("&&") && name.Contains("member-"))
                .Select(name => Guid.Parse(name.Split("member-")[1]))
                .ToList();

            Sender.Tell(memberIds);
        });
    }

    private IActorRef GetOrCreateMemberActor(Guid memberId)
    {
        if (_memberActors.TryGetValue(memberId, out var actor))
        {
            return actor;
        }

        // Create new actor with EventStore repository
        var newActor = Context.ActorOf(
            MemberActor.Props(memberId, _eventStore),
            $"member-{memberId}");

        _memberActors[memberId] = newActor;

        _log.Info($"Created new MemberActor for {memberId}");

        return newActor;
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 10,
            withinTimeRange: TimeSpan.FromMinutes(1),
            localOnlyDecider: ex =>
            {
                _log.Error(ex, "MemberActor failed");

                return ex switch
                {
                    InvalidOperationException => Directive.Resume,
                    ArgumentException => Directive.Resume,
                    _ => Directive.Restart
                };
            });
    }

    public static Props Props(IEventStoreRepository eventStore) =>
        Akka.Actor.Props.Create(() => new MemberActorSupervisor(eventStore));
}

/// <summary>
/// Query to get all member IDs
/// </summary>
public record GetAllMemberIds;
