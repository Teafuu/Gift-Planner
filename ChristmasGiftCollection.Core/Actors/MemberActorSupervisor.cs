using Akka.Actor;
using Akka.Event;
using ChristmasGiftCollection.Core.Actors.Commands;
using Marten;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Supervisor actor that manages MemberActor instances
/// Routes commands to the appropriate member actor based on MemberId
/// </summary>
public class MemberActorSupervisor : ReceiveActor
{
    private readonly IDocumentStore _documentStore;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<Guid, IActorRef> _memberActors = new();

    public MemberActorSupervisor(IDocumentStore documentStore)
    {
        _documentStore = documentStore;

        // Route all member commands to the appropriate actor
        Receive<MemberCommand>(cmd =>
        {
            var memberActor = GetOrCreateMemberActor(cmd.MemberId);
            memberActor.Forward(cmd);
        });

        // Handle requests to get all member IDs
        ReceiveAsync<GetAllMemberIds>(async _ =>
        {
            await using var session = _documentStore.LightweightSession();

            // Query all streams that start with member events
            var streamIds = await session.Events.QueryAllRawEvents()
                .Select(e => e.StreamId)
                .Distinct()
                .ToListAsync();

            Sender.Tell(streamIds);
        });
    }

    private IActorRef GetOrCreateMemberActor(Guid memberId)
    {
        if (_memberActors.TryGetValue(memberId, out var actor))
        {
            return actor;
        }

        // Pass DocumentStore instead of session - actor will create sessions per operation
        var newActor = Context.ActorOf(
            MemberActor.Props(memberId, _documentStore),
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

    public static Props Props(IDocumentStore documentStore) =>
        Akka.Actor.Props.Create(() => new MemberActorSupervisor(documentStore));
}

/// <summary>
/// Query to get all member IDs
/// </summary>
public record GetAllMemberIds;
