using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Models;

public class MemberTests
{
    [Fact]
    public void Member_ShouldInitializeWithDefaultValues()
    {
        // Act
        var member = new Member();

        // Assert
        member.Id.Should().BeEmpty();
        member.Name.Should().BeEmpty();
        member.Gifts.Should().BeEmpty();
        member.Relationships.Should().BeEmpty();
    }

    [Fact]
    public void Member_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var email = "john@example.com";
        var notes = "Test notes";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var member = new Member
        {
            Id = id,
            Name = name,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        member.Id.Should().Be(id);
        member.Name.Should().Be(name);
        member.CreatedAt.Should().Be(createdAt);
        member.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Member_ShouldAllowGiftsCollection()
    {
        // Arrange
        var member = new Member();
        var gift = new Gift
        {
            Id = Guid.NewGuid(),
            Name = "Test Gift",
            MemberId = member.Id
        };

        // Act
        member.Gifts.Add(gift);

        // Assert
        member.Gifts.Should().HaveCount(1);
        member.Gifts.First().Should().Be(gift);
    }

    [Fact]
    public void Member_ShouldAllowRelationshipsCollection()
    {
        // Arrange
        var member = new Member { Id = Guid.NewGuid() };
        var relationship = new Relationship
        {
            Id = Guid.NewGuid(),
            FromMemberId = member.Id,
            ToMemberId = Guid.NewGuid(),
            Type = RelationshipType.PartnerOf
        };

        // Act
        member.Relationships.Add(relationship);

        // Assert
        member.Relationships.Should().HaveCount(1);
        member.Relationships.First().Should().Be(relationship);
    }
}
