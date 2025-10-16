using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using ChristmasGiftCollection.Core.Repositories;
using Marten;

namespace ChristmasGiftCollection.Core.Services;

/// <summary>
/// Service for managing family members with event sourcing
/// </summary>
public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly IDocumentSession _session;

    public MemberService(
        IMemberRepository memberRepository,
        IRelationshipRepository relationshipRepository,
        IDocumentSession session)
    {
        _memberRepository = memberRepository;
        _relationshipRepository = relationshipRepository;
        _session = session;
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _memberRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Member?> GetMemberByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _memberRepository.GetByIdAsync(id, cancellationToken);
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

        // Raise event for event sourcing
        var memberAddedEvent = new MemberAdded
        {
            MemberId = memberId,
            Name = name,
            Email = email,
            Type = type,
            DateOfBirth = dateOfBirth,
            Notes = notes
        };

        _session.Events.StartStream<Member>(memberId, memberAddedEvent);
        await _session.SaveChangesAsync(cancellationToken);

        // Create and persist the member entity
        var member = new Member
        {
            Id = memberId,
            Name = name,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _memberRepository.AddAsync(member, cancellationToken);
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
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        if (member == null)
        {
            throw new InvalidOperationException($"Member with ID {id} not found.");
        }

        // Raise event for event sourcing
        var memberUpdatedEvent = new MemberUpdated
        {
            MemberId = id,
            Name = name,
            Email = email,
            Type = type,
            DateOfBirth = dateOfBirth,
            Notes = notes
        };

        _session.Events.Append(id, memberUpdatedEvent);
        await _session.SaveChangesAsync(cancellationToken);

        // Update the member entity
        if (name != null) member.Name = name;
        if (type.HasValue) member.Type = type.Value;
        member.UpdatedAt = DateTime.UtcNow;

        return await _memberRepository.UpdateAsync(member, cancellationToken);
    }

    public async Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        if (member == null)
        {
            throw new InvalidOperationException($"Member with ID {id} not found.");
        }

        // Raise event for event sourcing
        var memberRemovedEvent = new MemberRemoved
        {
            MemberId = id,
            Reason = "User deleted"
        };

        _session.Events.Append(id, memberRemovedEvent);
        await _session.SaveChangesAsync(cancellationToken);

        // Delete the member entity
        await _memberRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<object> GetMembersForGraphAsync(CancellationToken cancellationToken = default)
    {
        var members = await _memberRepository.GetAllAsync(cancellationToken);
        var relationships = await _relationshipRepository.GetAllAsync(cancellationToken);

        // Transform members into graph nodes
        var nodes = members.Select(m => new
        {
            id = m.Id,
            label = m.Name,
            group = m.Type.ToString().ToLower(),
            title = $"{m.Name} ({m.Type})"
        }).ToList();

        // Transform relationships into graph edges
        var edges = relationships.Select(r => new
        {
            from = r.FromMemberId,
            to = r.ToMemberId,
            label = r.Type.ToString().Replace("Of", " of "),
            arrows = "to"
        }).ToList();

        return new
        {
            nodes,
            edges
        };
    }
}
