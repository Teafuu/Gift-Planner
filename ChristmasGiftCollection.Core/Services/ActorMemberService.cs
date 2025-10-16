using Akka.Actor;
using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Services;

/// <summary>
/// Actor-based implementation of IMemberService using event sourcing
/// </summary>
public class ActorMemberService(ActorSystem actorSystem) : IMemberService
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    private IActorRef GetSupervisor() =>
        actorSystem.ActorSelection("/user/member-supervisor").ResolveOne(_timeout).Result;

    public async Task<IEnumerable<Member>> GetAllMembersAsync(CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        // Get all member IDs
        var memberIds = await supervisor.Ask<List<Guid>>(new GetAllMemberIds(), _timeout);

        // Query each member's state
        var members = new List<Member>();
        foreach (var memberId in memberIds)
        {
            var member = await GetMemberByIdAsync(memberId, cancellationToken);
            if (member != null && !member.Name.IsNullOrEmpty())
            {
                members.Add(member);
            }
        }

        return members;
    }

    public async Task<Member?> GetMemberByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        try
        {
            var member = await supervisor.Ask<Member?>(new GetMemberState(id), _timeout);
            return member;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Member> CreateMemberAsync(
        string name,
        MemberType type,
        string? email = null,
        DateTime? dateOfBirth = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var memberId = Guid.NewGuid();
        var supervisor = GetSupervisor();

        var command = new CreateMember(memberId, name, type, email, dateOfBirth, notes);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }

        // Return the newly created member
        var member = await GetMemberByIdAsync(memberId, cancellationToken);
        return member ?? throw new InvalidOperationException("Failed to create member");
    }

    public async Task<Member> UpdateMemberAsync(
        Guid id,
        string? name = null,
        MemberType? type = null,
        string? email = null,
        DateTime? dateOfBirth = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new UpdateMember(id, name, type, email, dateOfBirth, notes);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }

        // Return the updated member
        var member = await GetMemberByIdAsync(id, cancellationToken);
        return member ?? throw new InvalidOperationException($"Member with ID {id} not found");
    }

    public async Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new DeleteMember(id);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task<object> GetMembersForGraphAsync(CancellationToken cancellationToken = default)
    {
        var members = await GetAllMembersAsync(cancellationToken);

        // Transform members into graph nodes
        var nodes = members.Select(m => new
        {
            id = m.Id,
            label = m.Name,
            group = m.Type.ToString().ToLower(),
            title = $"{m.Name} ({m.Type})"
        }).ToList();

        // Transform relationships into graph edges
        var edges = members.SelectMany(m => m.Relationships.Select(r => new
        {
            from = r.FromMemberId,
            to = r.ToMemberId,
            label = r.Type.ToString().Replace("Of", " of "),
            arrows = "to"
        })).ToList();

        return new
        {
            nodes,
            edges
        };
    }
}

// Extension method for null/empty string checking
internal static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}
