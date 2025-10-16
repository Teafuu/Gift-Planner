using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Events;

public class GiftEventsTests
{
    [Fact]
    public void GiftAdded_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var name = "PlayStation 5";
        var description = "Gaming console";
        var price = 499.99m;
        var url = "https://example.com/ps5";
        var priority = GiftPriority.High;

        // Act
        var giftAdded = new GiftAdded
        {
            GiftId = giftId,
            MemberId = memberId,
            Name = name,
            Description = description,
            Priority = priority
        };

        // Assert
        giftAdded.GiftId.Should().Be(giftId);
        giftAdded.MemberId.Should().Be(memberId);
        giftAdded.Name.Should().Be(name);
        giftAdded.Description.Should().Be(description);
        giftAdded.Priority.Should().Be(priority);
        giftAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GiftAdded_ShouldAllowOptionalFields()
    {
        // Arrange & Act
        var giftAdded = new GiftAdded
        {
            GiftId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Name = "Simple Gift",
            Priority = GiftPriority.Medium
        };

        // Assert
        giftAdded.Description.Should().BeNull();
    }

    [Fact]
    public void GiftUpdated_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var name = "Updated Gift Name";
        var description = "Updated description";
        var price = 599.99m;
        var url = "https://example.com/updated";
        var priority = GiftPriority.High;

        // Act
        var giftUpdated = new GiftUpdated
        {
            GiftId = giftId,
            Name = name,
            Description = description,
            Priority = priority
        };

        // Assert
        giftUpdated.GiftId.Should().Be(giftId);
        giftUpdated.Name.Should().Be(name);
        giftUpdated.Description.Should().Be(description);
        giftUpdated.Priority.Should().Be(priority);
        giftUpdated.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GiftUpdated_ShouldAllowPartialUpdates()
    {
        // Arrange & Act
        var giftUpdated = new GiftUpdated
        {
            GiftId = Guid.NewGuid(),
            Name = "Only Name Updated"
        };

        // Assert
        giftUpdated.Name.Should().Be("Only Name Updated");
        giftUpdated.Description.Should().BeNull();
        giftUpdated.Priority.Should().BeNull();
    }

    [Fact]
    public void GiftTaken_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var takenByMemberId = Guid.NewGuid();

        // Act
        var giftTaken = new GiftTaken
        {
            GiftId = giftId,
            TakenByMemberId = takenByMemberId
        };

        // Assert
        giftTaken.GiftId.Should().Be(giftId);
        giftTaken.TakenByMemberId.Should().Be(takenByMemberId);
        giftTaken.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GiftReleased_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var releasedByMemberId = Guid.NewGuid();

        // Act
        var giftReleased = new GiftReleased
        {
            GiftId = giftId,
            ReleasedByMemberId = releasedByMemberId
        };

        // Assert
        giftReleased.GiftId.Should().Be(giftId);
        giftReleased.ReleasedByMemberId.Should().Be(releasedByMemberId);
        giftReleased.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GiftStatusChanged_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var oldStatus = GiftStatus.Available;
        var newStatus = GiftStatus.Taken;

        // Act
        var giftStatusChanged = new GiftStatusChanged
        {
            GiftId = giftId,
            OldStatus = oldStatus,
            NewStatus = newStatus
        };

        // Assert
        giftStatusChanged.GiftId.Should().Be(giftId);
        giftStatusChanged.OldStatus.Should().Be(oldStatus);
        giftStatusChanged.NewStatus.Should().Be(newStatus);
        giftStatusChanged.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(GiftStatus.Available, GiftStatus.Taken)]
    [InlineData(GiftStatus.Taken, GiftStatus.Available)]
    public void GiftStatusChanged_ShouldSupportAllStatusTransitions(GiftStatus oldStatus, GiftStatus newStatus)
    {
        // Arrange & Act
        var giftStatusChanged = new GiftStatusChanged
        {
            GiftId = Guid.NewGuid(),
            OldStatus = oldStatus,
            NewStatus = newStatus
        };

        // Assert
        giftStatusChanged.OldStatus.Should().Be(oldStatus);
        giftStatusChanged.NewStatus.Should().Be(newStatus);
    }

    [Fact]
    public void GiftRemoved_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var giftId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var reason = "No longer needed";

        // Act
        var giftRemoved = new GiftRemoved
        {
            GiftId = giftId,
            MemberId = memberId,
            Reason = reason
        };

        // Assert
        giftRemoved.GiftId.Should().Be(giftId);
        giftRemoved.MemberId.Should().Be(memberId);
        giftRemoved.Reason.Should().Be(reason);
        giftRemoved.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GiftEvents_ShouldInheritFromGiftEvent()
    {
        // Act & Assert
        var giftAdded = new GiftAdded();
        var giftUpdated = new GiftUpdated();
        var giftTaken = new GiftTaken();
        var giftReleased = new GiftReleased();
        var giftStatusChanged = new GiftStatusChanged();
        var giftRemoved = new GiftRemoved();

        giftAdded.Should().BeAssignableTo<GiftEvent>();
        giftUpdated.Should().BeAssignableTo<GiftEvent>();
        giftTaken.Should().BeAssignableTo<GiftEvent>();
        giftReleased.Should().BeAssignableTo<GiftEvent>();
        giftStatusChanged.Should().BeAssignableTo<GiftEvent>();
        giftRemoved.Should().BeAssignableTo<GiftEvent>();
    }

    [Fact]
    public void GiftEvent_ShouldHaveTimestamp()
    {
        // Arrange & Act
        var giftAdded = new GiftAdded
        {
            GiftId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Name = "Test Gift",
            Priority = GiftPriority.Low
        };

        // Assert
        giftAdded.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
