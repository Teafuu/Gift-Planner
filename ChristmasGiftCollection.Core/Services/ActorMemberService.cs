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
        string? email = null,
        DateTime? dateOfBirth = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var memberId = Guid.NewGuid();
        var supervisor = GetSupervisor();

        var command = new CreateMember(memberId, name, email, dateOfBirth, notes);
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
        string? email = null,
        DateTime? dateOfBirth = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new UpdateMember(id, name, email, dateOfBirth, notes);
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
            title = $"{m.Name}"
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

    public async Task AddGiftAsync(Guid memberId, string name, string? description, GiftPriority priority, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();
        var giftId = Guid.NewGuid();

        var command = new AddGift(memberId, giftId, name, description, null, null, priority);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task TakeGiftAsync(Guid memberId, Guid giftId, Guid takenByMemberId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new TakeGift(memberId, giftId, takenByMemberId);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task ReleaseGiftAsync(Guid memberId, Guid giftId, Guid releasedByMemberId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new ReleaseGift(memberId, giftId, releasedByMemberId);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task ReorderGiftsAsync(Guid memberId, Dictionary<Guid, int> giftOrders, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new ReorderGifts(memberId, giftOrders);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task RemoveGiftAsync(Guid memberId, Guid giftId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new RemoveGift(memberId, giftId, "Deleted by owner");
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task AddRelationshipAsync(Guid fromMemberId, Guid toMemberId, RelationshipType type, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();
        var relationshipId = Guid.NewGuid();

        var command = new AddRelationship(fromMemberId, relationshipId, toMemberId, type);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }

        // If PartnerOf, automatically create the reverse relationship so it's bidirectional
        if (type == RelationshipType.PartnerOf)
        {
            var reverseRelationshipId = Guid.NewGuid();
            var reverseCommand = new AddRelationship(toMemberId, reverseRelationshipId, fromMemberId, RelationshipType.PartnerOf);
            var reverseResult = await supervisor.Ask<object>(reverseCommand, _timeout);

            if (reverseResult is CommandFailure reverseFailure)
            {
                throw reverseFailure.Exception;
            }
        }
    }

    public async Task RemoveRelationshipAsync(Guid fromMemberId, Guid relationshipId, Guid toMemberId, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        // Get the relationship type before removing it
        var fromMember = await GetMemberByIdAsync(fromMemberId, cancellationToken);
        var relationship = fromMember?.Relationships.FirstOrDefault(r => r.Id == relationshipId);

        var command = new RemoveRelationship(fromMemberId, relationshipId, toMemberId);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }

        // If it was a PartnerOf relationship, also remove the reverse relationship
        if (relationship?.Type == RelationshipType.PartnerOf)
        {
            var toMember = await GetMemberByIdAsync(toMemberId, cancellationToken);
            var reverseRelationship = toMember?.Relationships.FirstOrDefault(r =>
                r.Type == RelationshipType.PartnerOf && r.ToMemberId == fromMemberId);

            if (reverseRelationship != null)
            {
                var reverseCommand = new RemoveRelationship(toMemberId, reverseRelationship.Id, fromMemberId);
                var reverseResult = await supervisor.Ask<object>(reverseCommand, _timeout);

                if (reverseResult is CommandFailure reverseFailure)
                {
                    throw reverseFailure.Exception;
                }
            }
        }
    }

    public async Task<bool> CanEditGiftsAsync(Guid currentMemberId, Guid targetMemberId, CancellationToken cancellationToken = default)
    {
        // Same member can always edit their own gifts
        if (currentMemberId == targetMemberId)
            return true;

        // Get current member to check if admin
        var currentMember = await GetMemberByIdAsync(currentMemberId, cancellationToken);
        if (currentMember?.IsAdmin == true)
            return true;

        // Check if target is a descendant (child, grandchild, etc.) using ONLY ParentOf relationships
        var descendants = await GetAllDescendantsAsync(currentMemberId, cancellationToken);
        return descendants.Contains(targetMemberId);
    }

    public async Task<List<Guid>> GetAllDescendantsAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        // Load all members once to avoid N database calls
        var allMembers = (await GetAllMembersAsync(cancellationToken)).ToList();
        var memberLookup = allMembers.ToDictionary(m => m.Id);

        var descendants = new HashSet<Guid>();
        var toProcess = new Queue<Guid>();
        toProcess.Enqueue(memberId);

        // Breadth-first search through ParentOf relationships
        while (toProcess.Count > 0)
        {
            var currentId = toProcess.Dequeue();

            // Look up member in memory instead of hitting database
            if (!memberLookup.TryGetValue(currentId, out var member))
                continue;

            // Only look at ParentOf relationships (ignore ChildOf to avoid duplication)
            foreach (var rel in member.Relationships.Where(r => r.Type == RelationshipType.ParentOf))
            {
                if (!descendants.Contains(rel.ToMemberId))
                {
                    descendants.Add(rel.ToMemberId);
                    toProcess.Enqueue(rel.ToMemberId); // Recursively check this child's children
                }
            }
        }

        return descendants.ToList();
    }

    public async Task SetPinCodeAsync(Guid memberId, string pinCode, CancellationToken cancellationToken = default)
    {
        var supervisor = GetSupervisor();

        var command = new SetPinCode(memberId, pinCode);
        var result = await supervisor.Ask<object>(command, _timeout);

        if (result is CommandFailure failure)
        {
            throw failure.Exception;
        }
    }

    public async Task<List<(Gift gift, Member owner)>> GetGiftsTakenByMemberAsync(Guid takenByMemberId, CancellationToken cancellationToken = default)
    {
        var allMembers = await GetAllMembersAsync(cancellationToken);
        var takenGifts = new List<(Gift gift, Member owner)>();

        foreach (var member in allMembers)
        {
            var giftsFromThisMember = member.Gifts
                .Where(g => g.TakenByMemberId == takenByMemberId && g.Status == GiftStatus.Taken)
                .Select(g => (gift: g, owner: member));

            takenGifts.AddRange(giftsFromThisMember);
        }

        return takenGifts;
    }
}

// Extension method for null/empty string checking
internal static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}
