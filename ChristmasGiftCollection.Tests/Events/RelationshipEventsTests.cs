using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Events;

public class RelationshipEventsTests
{
    [Fact]
    public void RelationshipAdded_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var fromMemberId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();
        var type = RelationshipType.ParentOf;

        // Act
        var relationshipAdded = new RelationshipAdded
        {
            RelationshipId = relationshipId,
            FromMemberId = fromMemberId,
            ToMemberId = toMemberId,
            Type = type
        };

        // Assert
        relationshipAdded.RelationshipId.Should().Be(relationshipId);
        relationshipAdded.FromMemberId.Should().Be(fromMemberId);
        relationshipAdded.ToMemberId.Should().Be(toMemberId);
        relationshipAdded.Type.Should().Be(type);
        relationshipAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(RelationshipType.ParentOf)]
    [InlineData(RelationshipType.ChildOf)]
    [InlineData(RelationshipType.SpouseOf)]
    [InlineData(RelationshipType.SiblingOf)]
    [InlineData(RelationshipType.GrandparentOf)]
    [InlineData(RelationshipType.GrandchildOf)]
    [InlineData(RelationshipType.Other)]
    public void RelationshipAdded_ShouldSupportAllRelationshipTypes(RelationshipType type)
    {
        // Arrange & Act
        var relationshipAdded = new RelationshipAdded
        {
            RelationshipId = Guid.NewGuid(),
            FromMemberId = Guid.NewGuid(),
            ToMemberId = Guid.NewGuid(),
            Type = type
        };

        // Assert
        relationshipAdded.Type.Should().Be(type);
    }

    [Fact]
    public void RelationshipUpdated_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var newType = RelationshipType.SpouseOf;

        // Act
        var relationshipUpdated = new RelationshipUpdated
        {
            RelationshipId = relationshipId,
            NewType = newType
        };

        // Assert
        relationshipUpdated.RelationshipId.Should().Be(relationshipId);
        relationshipUpdated.NewType.Should().Be(newType);
        relationshipUpdated.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RelationshipRemoved_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var fromMemberId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();

        // Act
        var relationshipRemoved = new RelationshipRemoved
        {
            RelationshipId = relationshipId,
            FromMemberId = fromMemberId,
            ToMemberId = toMemberId
        };

        // Assert
        relationshipRemoved.RelationshipId.Should().Be(relationshipId);
        relationshipRemoved.FromMemberId.Should().Be(fromMemberId);
        relationshipRemoved.ToMemberId.Should().Be(toMemberId);
        relationshipRemoved.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RelationshipEvents_ShouldInheritFromRelationshipEvent()
    {
        // Act & Assert
        var relationshipAdded = new RelationshipAdded();
        var relationshipUpdated = new RelationshipUpdated();
        var relationshipRemoved = new RelationshipRemoved();

        relationshipAdded.Should().BeAssignableTo<RelationshipEvent>();
        relationshipUpdated.Should().BeAssignableTo<RelationshipEvent>();
        relationshipRemoved.Should().BeAssignableTo<RelationshipEvent>();
    }

    [Fact]
    public void RelationshipEvent_ShouldHaveTimestamp()
    {
        // Arrange & Act
        var relationshipAdded = new RelationshipAdded
        {
            RelationshipId = Guid.NewGuid(),
            FromMemberId = Guid.NewGuid(),
            ToMemberId = Guid.NewGuid(),
            Type = RelationshipType.ParentOf
        };

        // Assert
        relationshipAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RelationshipAdded_ShouldSupportDifferentMemberIds()
    {
        // Arrange
        var fromMemberId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();

        // Act
        var relationshipAdded = new RelationshipAdded
        {
            RelationshipId = Guid.NewGuid(),
            FromMemberId = fromMemberId,
            ToMemberId = toMemberId,
            Type = RelationshipType.ParentOf
        };

        // Assert
        relationshipAdded.FromMemberId.Should().NotBe(relationshipAdded.ToMemberId);
        relationshipAdded.FromMemberId.Should().Be(fromMemberId);
        relationshipAdded.ToMemberId.Should().Be(toMemberId);
    }
}
