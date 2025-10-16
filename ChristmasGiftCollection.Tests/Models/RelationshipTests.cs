using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Models;

public class RelationshipTests
{
    [Fact]
    public void Relationship_ShouldInitializeWithDefaultValues()
    {
        // Act
        var relationship = new Relationship();

        // Assert
        relationship.Id.Should().BeEmpty();
        relationship.FromMemberId.Should().BeEmpty();
        relationship.ToMemberId.Should().BeEmpty();
        relationship.Type.Should().Be(RelationshipType.ParentOf);
        relationship.FromMember.Should().BeNull();
        relationship.ToMember.Should().BeNull();
    }

    [Fact]
    public void Relationship_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fromMemberId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();
        var type = RelationshipType.SpouseOf;
        var createdAt = DateTime.UtcNow;

        // Act
        var relationship = new Relationship
        {
            Id = id,
            FromMemberId = fromMemberId,
            ToMemberId = toMemberId,
            Type = type,
            CreatedAt = createdAt
        };

        // Assert
        relationship.Id.Should().Be(id);
        relationship.FromMemberId.Should().Be(fromMemberId);
        relationship.ToMemberId.Should().Be(toMemberId);
        relationship.Type.Should().Be(type);
        relationship.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(RelationshipType.ParentOf)]
    [InlineData(RelationshipType.ChildOf)]
    [InlineData(RelationshipType.SpouseOf)]
    [InlineData(RelationshipType.SiblingOf)]
    [InlineData(RelationshipType.GrandparentOf)]
    [InlineData(RelationshipType.GrandchildOf)]
    [InlineData(RelationshipType.Other)]
    public void Relationship_ShouldSupportAllRelationshipTypes(RelationshipType type)
    {
        // Arrange & Act
        var relationship = new Relationship { Type = type };

        // Assert
        relationship.Type.Should().Be(type);
    }

    [Fact]
    public void Relationship_ShouldSupportParentChildRelationship()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        // Act
        var relationship = new Relationship
        {
            FromMemberId = parentId,
            ToMemberId = childId,
            Type = RelationshipType.ParentOf
        };

        // Assert
        relationship.FromMemberId.Should().Be(parentId);
        relationship.ToMemberId.Should().Be(childId);
        relationship.Type.Should().Be(RelationshipType.ParentOf);
    }

    [Fact]
    public void Relationship_ShouldSupportSpouseRelationship()
    {
        // Arrange
        var spouse1Id = Guid.NewGuid();
        var spouse2Id = Guid.NewGuid();

        // Act
        var relationship = new Relationship
        {
            FromMemberId = spouse1Id,
            ToMemberId = spouse2Id,
            Type = RelationshipType.SpouseOf
        };

        // Assert
        relationship.FromMemberId.Should().Be(spouse1Id);
        relationship.ToMemberId.Should().Be(spouse2Id);
        relationship.Type.Should().Be(RelationshipType.SpouseOf);
    }

    [Fact]
    public void Relationship_ShouldSupportSiblingRelationship()
    {
        // Arrange
        var sibling1Id = Guid.NewGuid();
        var sibling2Id = Guid.NewGuid();

        // Act
        var relationship = new Relationship
        {
            FromMemberId = sibling1Id,
            ToMemberId = sibling2Id,
            Type = RelationshipType.SiblingOf
        };

        // Assert
        relationship.FromMemberId.Should().Be(sibling1Id);
        relationship.ToMemberId.Should().Be(sibling2Id);
        relationship.Type.Should().Be(RelationshipType.SiblingOf);
    }

    [Fact]
    public void Relationship_ShouldSupportGrandparentGrandchildRelationship()
    {
        // Arrange
        var grandparentId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();

        // Act
        var relationship = new Relationship
        {
            FromMemberId = grandparentId,
            ToMemberId = grandchildId,
            Type = RelationshipType.GrandparentOf
        };

        // Assert
        relationship.FromMemberId.Should().Be(grandparentId);
        relationship.ToMemberId.Should().Be(grandchildId);
        relationship.Type.Should().Be(RelationshipType.GrandparentOf);
    }

    [Fact]
    public void Relationship_ShouldSupportNavigationToFromMember()
    {
        // Arrange
        var fromMemberId = Guid.NewGuid();
        var fromMember = new Member { Id = fromMemberId, Name = "Parent" };
        var relationship = new Relationship
        {
            FromMemberId = fromMemberId,
            FromMember = fromMember
        };

        // Act & Assert
        relationship.FromMember.Should().NotBeNull();
        relationship.FromMember.Should().Be(fromMember);
        relationship.FromMember!.Name.Should().Be("Parent");
    }

    [Fact]
    public void Relationship_ShouldSupportNavigationToToMember()
    {
        // Arrange
        var toMemberId = Guid.NewGuid();
        var toMember = new Member { Id = toMemberId, Name = "Child" };
        var relationship = new Relationship
        {
            ToMemberId = toMemberId,
            ToMember = toMember
        };

        // Act & Assert
        relationship.ToMember.Should().NotBeNull();
        relationship.ToMember.Should().Be(toMember);
        relationship.ToMember!.Name.Should().Be("Child");
    }

    [Fact]
    public void Relationship_ShouldAllowBidirectionalRelationships()
    {
        // Arrange
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        // Act
        var relationship1 = new Relationship
        {
            FromMemberId = member1Id,
            ToMemberId = member2Id,
            Type = RelationshipType.ParentOf
        };

        var relationship2 = new Relationship
        {
            FromMemberId = member2Id,
            ToMemberId = member1Id,
            Type = RelationshipType.ChildOf
        };

        // Assert
        relationship1.FromMemberId.Should().Be(member1Id);
        relationship1.ToMemberId.Should().Be(member2Id);
        relationship1.Type.Should().Be(RelationshipType.ParentOf);

        relationship2.FromMemberId.Should().Be(member2Id);
        relationship2.ToMemberId.Should().Be(member1Id);
        relationship2.Type.Should().Be(RelationshipType.ChildOf);
    }
}
