using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Events;

public class MemberEventsTests
{
    [Fact]
    public void MemberAdded_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var name = "John Doe";
        var email = "john@example.com";
        var type = MemberType.Parent;
        var dateOfBirth = new DateTime(1990, 1, 1);
        var notes = "Test notes";

        // Act
        var memberAdded = new MemberAdded
        {
            MemberId = memberId,
            Name = name,
            Email = email,
            Type = type,
            DateOfBirth = dateOfBirth,
            Notes = notes
        };

        // Assert
        memberAdded.MemberId.Should().Be(memberId);
        memberAdded.Name.Should().Be(name);
        memberAdded.Email.Should().Be(email);
        memberAdded.Type.Should().Be(type);
        memberAdded.DateOfBirth.Should().Be(dateOfBirth);
        memberAdded.Notes.Should().Be(notes);
        memberAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MemberAdded_ShouldAllowOptionalFields()
    {
        // Arrange & Act
        var memberAdded = new MemberAdded
        {
            MemberId = Guid.NewGuid(),
            Name = "Jane Doe",
            Type = MemberType.Child
        };

        // Assert
        memberAdded.Email.Should().BeNull();
        memberAdded.DateOfBirth.Should().BeNull();
        memberAdded.Notes.Should().BeNull();
    }

    [Fact]
    public void MemberUpdated_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var name = "Updated Name";
        var email = "updated@example.com";
        var type = MemberType.Child;
        var dateOfBirth = new DateTime(2010, 5, 15);
        var notes = "Updated notes";

        // Act
        var memberUpdated = new MemberUpdated
        {
            MemberId = memberId,
            Name = name,
            Email = email,
            Type = type,
            DateOfBirth = dateOfBirth,
            Notes = notes
        };

        // Assert
        memberUpdated.MemberId.Should().Be(memberId);
        memberUpdated.Name.Should().Be(name);
        memberUpdated.Email.Should().Be(email);
        memberUpdated.Type.Should().Be(type);
        memberUpdated.DateOfBirth.Should().Be(dateOfBirth);
        memberUpdated.Notes.Should().Be(notes);
        memberUpdated.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MemberUpdated_ShouldAllowPartialUpdates()
    {
        // Arrange & Act
        var memberUpdated = new MemberUpdated
        {
            MemberId = Guid.NewGuid(),
            Name = "Only Name Updated"
        };

        // Assert
        memberUpdated.Name.Should().Be("Only Name Updated");
        memberUpdated.Email.Should().BeNull();
        memberUpdated.Type.Should().BeNull();
        memberUpdated.DateOfBirth.Should().BeNull();
        memberUpdated.Notes.Should().BeNull();
    }

    [Fact]
    public void MemberRemoved_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var reason = "User requested deletion";

        // Act
        var memberRemoved = new MemberRemoved
        {
            MemberId = memberId,
            Reason = reason
        };

        // Assert
        memberRemoved.MemberId.Should().Be(memberId);
        memberRemoved.Reason.Should().Be(reason);
        memberRemoved.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MemberEvents_ShouldInheritFromMemberEvent()
    {
        // Act & Assert
        var memberAdded = new MemberAdded();
        var memberUpdated = new MemberUpdated();
        var memberRemoved = new MemberRemoved();

        memberAdded.Should().BeAssignableTo<MemberEvent>();
        memberUpdated.Should().BeAssignableTo<MemberEvent>();
        memberRemoved.Should().BeAssignableTo<MemberEvent>();
    }

    [Fact]
    public void MemberEvent_ShouldHaveTimestamp()
    {
        // Arrange & Act
        var memberAdded = new MemberAdded { MemberId = Guid.NewGuid(), Name = "Test" };

        // Assert
        memberAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
