using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Aggregate root for Member - built by replaying events
/// </summary>
public class MemberAggregate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public Dictionary<Guid, GiftAggregate> Gifts { get; private set; } = new();
    public Dictionary<Guid, RelationshipAggregate> Relationships { get; private set; } = new();

    /// <summary>
    /// Apply MemberAdded event
    /// </summary>
    public void Apply(MemberAdded evt)
    {
        Id = evt.MemberId;
        Name = evt.Name;
        Email = evt.Email;
        DateOfBirth = evt.DateOfBirth;
        Notes = evt.Notes;
        CreatedAt = evt.Timestamp;
        UpdatedAt = evt.Timestamp;
        IsDeleted = false;
    }

    /// <summary>
    /// Apply MemberUpdated event
    /// </summary>
    public void Apply(MemberUpdated evt)
    {
        if (evt.Name != null) Name = evt.Name;
        if (evt.Email != null) Email = evt.Email;
        if (evt.DateOfBirth.HasValue) DateOfBirth = evt.DateOfBirth;
        if (evt.Notes != null) Notes = evt.Notes;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply MemberRemoved event
    /// </summary>
    public void Apply(MemberRemoved evt)
    {
        IsDeleted = true;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply GiftAdded event
    /// </summary>
    public void Apply(GiftAdded evt)
    {
        var gift = new GiftAggregate();
        gift.Apply(evt);
        Gifts[evt.GiftId] = gift;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply GiftUpdated event
    /// </summary>
    public void Apply(GiftUpdated evt)
    {
        if (Gifts.TryGetValue(evt.GiftId, out var gift))
        {
            gift.Apply(evt);
            UpdatedAt = evt.Timestamp;
        }
    }

    /// <summary>
    /// Apply GiftTaken event
    /// </summary>
    public void Apply(GiftTaken evt)
    {
        if (Gifts.TryGetValue(evt.GiftId, out var gift))
        {
            gift.Apply(evt);
            UpdatedAt = evt.Timestamp;
        }
    }

    /// <summary>
    /// Apply GiftReleased event
    /// </summary>
    public void Apply(GiftReleased evt)
    {
        if (Gifts.TryGetValue(evt.GiftId, out var gift))
        {
            gift.Apply(evt);
            UpdatedAt = evt.Timestamp;
        }
    }

    /// <summary>
    /// Apply GiftStatusChanged event
    /// </summary>
    public void Apply(GiftStatusChanged evt)
    {
        if (Gifts.TryGetValue(evt.GiftId, out var gift))
        {
            gift.Apply(evt);
            UpdatedAt = evt.Timestamp;
        }
    }

    /// <summary>
    /// Apply GiftRemoved event
    /// </summary>
    public void Apply(GiftRemoved evt)
    {
        Gifts.Remove(evt.GiftId);
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply RelationshipAdded event
    /// </summary>
    public void Apply(RelationshipAdded evt)
    {
        var relationship = new RelationshipAggregate();
        relationship.Apply(evt);
        Relationships[evt.RelationshipId] = relationship;
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Apply RelationshipUpdated event
    /// </summary>
    public void Apply(RelationshipUpdated evt)
    {
        if (Relationships.TryGetValue(evt.RelationshipId, out var relationship))
        {
            relationship.Apply(evt);
            UpdatedAt = evt.Timestamp;
        }
    }

    /// <summary>
    /// Apply RelationshipRemoved event
    /// </summary>
    public void Apply(RelationshipRemoved evt)
    {
        Relationships.Remove(evt.RelationshipId);
        UpdatedAt = evt.Timestamp;
    }

    /// <summary>
    /// Convert aggregate to Member model
    /// </summary>
    public Member ToModel()
    {
        return new Member
        {
            Id = Id,
            Name = Name,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            Gifts = Gifts.Values.Select(g => g.ToModel(Id)).ToList(),
            Relationships = Relationships.Values.Select(r => r.ToModel(Id)).ToList()
        };
    }
}

/// <summary>
/// Gift aggregate - part of member stream
/// </summary>
public class GiftAggregate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal? Price { get; private set; }
    public string? Url { get; private set; }
    public GiftStatus Status { get; private set; }
    public Guid? TakenByMemberId { get; private set; }
    public DateTime? TakenAt { get; private set; }
    public GiftPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Apply(GiftAdded evt)
    {
        Id = evt.GiftId;
        Name = evt.Name;
        Description = evt.Description;
        Priority = evt.Priority;
        Status = GiftStatus.Available;
        CreatedAt = evt.Timestamp;
        UpdatedAt = evt.Timestamp;
    }

    public void Apply(GiftUpdated evt)
    {
        if (evt.Name != null) Name = evt.Name;
        if (evt.Description != null) Description = evt.Description;
        if (evt.Priority.HasValue) Priority = evt.Priority.Value;
        UpdatedAt = evt.Timestamp;
    }

    public void Apply(GiftTaken evt)
    {
        Status = GiftStatus.Taken;
        TakenByMemberId = evt.TakenByMemberId;
        TakenAt = evt.Timestamp;
        UpdatedAt = evt.Timestamp;
    }

    public void Apply(GiftReleased evt)
    {
        Status = GiftStatus.Available;
        TakenByMemberId = null;
        TakenAt = null;
        UpdatedAt = evt.Timestamp;
    }

    public void Apply(GiftStatusChanged evt)
    {
        Status = evt.NewStatus;
        UpdatedAt = evt.Timestamp;
    }

    public Gift ToModel(Guid memberId)
    {
        return new Gift
        {
            Id = Id,
            MemberId = memberId,
            Name = Name,
            Description = Description,
            Price = Price,
            Url = Url,
            Status = Status,
            TakenByMemberId = TakenByMemberId,
            TakenAt = TakenAt,
            Priority = Priority,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}

/// <summary>
/// Relationship aggregate - part of member stream
/// </summary>
public class RelationshipAggregate
{
    public Guid Id { get; private set; }
    public Guid ToMemberId { get; private set; }
    public RelationshipType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Apply(RelationshipAdded evt)
    {
        Id = evt.RelationshipId;
        ToMemberId = evt.ToMemberId;
        Type = evt.Type;
        CreatedAt = evt.Timestamp;
    }

    public void Apply(RelationshipUpdated evt)
    {
        Type = evt.NewType;
    }

    public Relationship ToModel(Guid fromMemberId)
    {
        return new Relationship
        {
            Id = Id,
            FromMemberId = fromMemberId,
            ToMemberId = ToMemberId,
            Type = Type,
            CreatedAt = CreatedAt
        };
    }
}
