using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Actors;

public class MemberAggregateTests
{
    [Fact]
    public void MemberAggregate_ShouldApplyMemberAddedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var evt = new MemberAdded
        {
            MemberId = memberId,
            Name = "John Doe",
            Email = "john@example.com",
            Type = MemberType.Parent,
            DateOfBirth = new DateTime(1990, 1, 1),
            Notes = "Test member",
            Timestamp = DateTime.UtcNow
        };

        // Act
        aggregate.Apply(evt);

        // Assert
        aggregate.Id.Should().Be(memberId);
        aggregate.Name.Should().Be("John Doe");
        aggregate.Email.Should().Be("john@example.com");
        aggregate.Type.Should().Be(MemberType.Parent);
        aggregate.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        aggregate.Notes.Should().Be("Test member");
        aggregate.IsDeleted.Should().BeFalse();
        aggregate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        aggregate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MemberAggregate_ShouldApplyMemberUpdatedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();

        // First add the member
        aggregate.Apply(new MemberAdded
        {
            MemberId = memberId,
            Name = "John Doe",
            Email = "old@example.com",
            Type = MemberType.Parent
        });

        // Act - Update member
        aggregate.Apply(new MemberUpdated
        {
            MemberId = memberId,
            Name = "Jane Smith",
            Email = "new@example.com",
            Type = MemberType.Child
        });

        // Assert
        aggregate.Name.Should().Be("Jane Smith");
        aggregate.Email.Should().Be("new@example.com");
        aggregate.Type.Should().Be(MemberType.Child);
    }

    [Fact]
    public void MemberAggregate_ShouldApplyMemberRemovedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded
        {
            MemberId = memberId,
            Name = "John Doe",
            Type = MemberType.Parent
        });

        // Act
        aggregate.Apply(new MemberRemoved
        {
            MemberId = memberId,
            Reason = "Test deletion"
        });

        // Assert
        aggregate.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void MemberAggregate_ShouldApplyGiftAddedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });

        // Act
        aggregate.Apply(new GiftAdded
        {
            GiftId = giftId,
            MemberId = memberId,
            Name = "PlayStation 5",
            Description = "Gaming console",
            Priority = GiftPriority.High,
        });

        // Assert
        aggregate.Gifts.Should().ContainKey(giftId);
        aggregate.Gifts[giftId].Name.Should().Be("PlayStation 5");
        aggregate.Gifts[giftId].Status.Should().Be(GiftStatus.Available);
    }

    [Fact]
    public void MemberAggregate_ShouldApplyGiftTakenEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var takenByMemberId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new GiftAdded
        {
            GiftId = giftId,
            MemberId = memberId,
            Name = "Gift",
            Priority = GiftPriority.Medium
        });

        // Act
        aggregate.Apply(new GiftTaken
        {
            GiftId = giftId,
            TakenByMemberId = takenByMemberId
        });

        // Assert
        aggregate.Gifts[giftId].Status.Should().Be(GiftStatus.Taken);
        aggregate.Gifts[giftId].TakenByMemberId.Should().Be(takenByMemberId);
        aggregate.Gifts[giftId].TakenAt.Should().NotBeNull();
    }

    [Fact]
    public void MemberAggregate_ShouldApplyGiftReleasedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new GiftAdded { GiftId = giftId, MemberId = memberId, Name = "Gift", Priority = GiftPriority.Medium });
        aggregate.Apply(new GiftTaken { GiftId = giftId, TakenByMemberId = Guid.NewGuid() });

        // Act
        aggregate.Apply(new GiftReleased
        {
            GiftId = giftId,
            ReleasedByMemberId = Guid.NewGuid()
        });

        // Assert
        aggregate.Gifts[giftId].Status.Should().Be(GiftStatus.Available);
        aggregate.Gifts[giftId].TakenByMemberId.Should().BeNull();
        aggregate.Gifts[giftId].TakenAt.Should().BeNull();
    }

    [Fact]
    public void MemberAggregate_ShouldApplyGiftRemovedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new GiftAdded { GiftId = giftId, MemberId = memberId, Name = "Gift", Priority = GiftPriority.Medium });

        // Act
        aggregate.Apply(new GiftRemoved
        {
            GiftId = giftId,
            MemberId = memberId,
            Reason = "No longer needed"
        });

        // Assert
        aggregate.Gifts.Should().NotContainKey(giftId);
    }

    [Fact]
    public void MemberAggregate_ShouldApplyRelationshipAddedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });

        // Act
        aggregate.Apply(new RelationshipAdded
        {
            RelationshipId = relationshipId,
            FromMemberId = memberId,
            ToMemberId = toMemberId,
            Type = RelationshipType.ParentOf
        });

        // Assert
        aggregate.Relationships.Should().ContainKey(relationshipId);
        aggregate.Relationships[relationshipId].ToMemberId.Should().Be(toMemberId);
        aggregate.Relationships[relationshipId].Type.Should().Be(RelationshipType.ParentOf);
    }

    [Fact]
    public void MemberAggregate_ShouldApplyRelationshipUpdatedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new RelationshipAdded
        {
            RelationshipId = relationshipId,
            FromMemberId = memberId,
            ToMemberId = Guid.NewGuid(),
            Type = RelationshipType.ParentOf
        });

        // Act
        aggregate.Apply(new RelationshipUpdated
        {
            RelationshipId = relationshipId,
            NewType = RelationshipType.SpouseOf
        });

        // Assert
        aggregate.Relationships[relationshipId].Type.Should().Be(RelationshipType.SpouseOf);
    }

    [Fact]
    public void MemberAggregate_ShouldApplyRelationshipRemovedEvent()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new RelationshipAdded
        {
            RelationshipId = relationshipId,
            FromMemberId = memberId,
            ToMemberId = Guid.NewGuid(),
            Type = RelationshipType.ParentOf
        });

        // Act
        aggregate.Apply(new RelationshipRemoved
        {
            RelationshipId = relationshipId,
            FromMemberId = memberId,
            ToMemberId = Guid.NewGuid()
        });

        // Assert
        aggregate.Relationships.Should().NotContainKey(relationshipId);
    }

    [Fact]
    public void MemberAggregate_ShouldConvertToModel()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();

        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John Doe", Email = "john@example.com", Type = MemberType.Parent });
        aggregate.Apply(new GiftAdded { GiftId = giftId, MemberId = memberId, Name = "Gift", Priority = GiftPriority.High });
        aggregate.Apply(new RelationshipAdded { RelationshipId = relationshipId, FromMemberId = memberId, ToMemberId = toMemberId, Type = RelationshipType.ParentOf });

        // Act
        var model = aggregate.ToModel();

        // Assert
        model.Id.Should().Be(memberId);
        model.Name.Should().Be("John Doe");
        model.Type.Should().Be(MemberType.Parent);
        model.Gifts.Should().HaveCount(1);
        model.Gifts.First().Name.Should().Be("Gift");
        model.Relationships.Should().HaveCount(1);
        model.Relationships.First().Type.Should().Be(RelationshipType.ParentOf);
    }

    [Fact]
    public void MemberAggregate_ShouldReplayMultipleEvents()
    {
        // Arrange
        var aggregate = new MemberAggregate();
        var memberId = Guid.NewGuid();

        // Act - Replay event history
        aggregate.Apply(new MemberAdded { MemberId = memberId, Name = "John", Type = MemberType.Parent });
        aggregate.Apply(new MemberUpdated { MemberId = memberId, Name = "John Doe" });
        aggregate.Apply(new GiftAdded { GiftId = Guid.NewGuid(), MemberId = memberId, Name = "Gift 1", Priority = GiftPriority.Low });
        aggregate.Apply(new GiftAdded { GiftId = Guid.NewGuid(), MemberId = memberId, Name = "Gift 2", Priority = GiftPriority.Medium });
        aggregate.Apply(new RelationshipAdded { RelationshipId = Guid.NewGuid(), FromMemberId = memberId, ToMemberId = Guid.NewGuid(), Type = RelationshipType.ParentOf });

        // Assert
        aggregate.Name.Should().Be("John Doe");
        aggregate.Gifts.Should().HaveCount(2);
        aggregate.Relationships.Should().HaveCount(1);
    }
}
