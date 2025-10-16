using ChristmasGiftCollection.Core.Models;
using FluentAssertions;

namespace ChristmasGiftCollection.Tests.Models;

public class GiftTests
{
    [Fact]
    public void Gift_ShouldInitializeWithDefaultValues()
    {
        // Act
        var gift = new Gift();

        // Assert
        gift.Id.Should().BeEmpty();
        gift.MemberId.Should().BeEmpty();
        gift.Name.Should().BeEmpty();
        gift.Description.Should().BeNull();
        gift.Price.Should().BeNull();
        gift.Url.Should().BeNull();
        gift.Status.Should().Be(GiftStatus.Available);
        gift.TakenByMemberId.Should().BeNull();
        gift.TakenAt.Should().BeNull();
        gift.Priority.Should().Be(GiftPriority.Low);
    }

    [Fact]
    public void Gift_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var name = "PlayStation 5";
        var description = "Latest gaming console";
        var price = 499.99m;
        var url = "https://example.com/ps5";
        var status = GiftStatus.Taken;
        var takenByMemberId = Guid.NewGuid();
        var takenAt = DateTime.UtcNow;
        var priority = GiftPriority.High;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var gift = new Gift
        {
            Id = id,
            MemberId = memberId,
            Name = name,
            Description = description,
            Price = price,
            Url = url,
            Status = status,
            TakenByMemberId = takenByMemberId,
            TakenAt = takenAt,
            Priority = priority,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        gift.Id.Should().Be(id);
        gift.MemberId.Should().Be(memberId);
        gift.Name.Should().Be(name);
        gift.Description.Should().Be(description);
        gift.Price.Should().Be(price);
        gift.Url.Should().Be(url);
        gift.Status.Should().Be(status);
        gift.TakenByMemberId.Should().Be(takenByMemberId);
        gift.TakenAt.Should().Be(takenAt);
        gift.Priority.Should().Be(priority);
        gift.CreatedAt.Should().Be(createdAt);
        gift.UpdatedAt.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData(GiftStatus.Available)]
    [InlineData(GiftStatus.Taken)]
    public void Gift_ShouldSupportAllStatuses(GiftStatus status)
    {
        // Arrange & Act
        var gift = new Gift { Status = status };

        // Assert
        gift.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(GiftPriority.Low)]
    [InlineData(GiftPriority.Medium)]
    [InlineData(GiftPriority.High)]
    public void Gift_ShouldSupportAllPriorities(GiftPriority priority)
    {
        // Arrange & Act
        var gift = new Gift { Priority = priority };

        // Assert
        gift.Priority.Should().Be(priority);
    }

    [Fact]
    public void Gift_ShouldSupportNavigationToMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, Name = "John Doe" };
        var gift = new Gift
        {
            MemberId = memberId,
            Member = member
        };

        // Act & Assert
        gift.Member.Should().NotBeNull();
        gift.Member.Should().Be(member);
        gift.Member!.Name.Should().Be("John Doe");
    }

    [Fact]
    public void Gift_ShouldSupportNavigationToTakenByMember()
    {
        // Arrange
        var takenByMemberId = Guid.NewGuid();
        var takenByMember = new Member { Id = takenByMemberId, Name = "Jane Doe" };
        var gift = new Gift
        {
            TakenByMemberId = takenByMemberId,
            TakenByMember = takenByMember,
            Status = GiftStatus.Taken
        };

        // Act & Assert
        gift.TakenByMember.Should().NotBeNull();
        gift.TakenByMember.Should().Be(takenByMember);
        gift.TakenByMember!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public void Gift_ShouldAllowPriceWithTwoDecimals()
    {
        // Arrange & Act
        var gift = new Gift { Price = 19.99m };

        // Assert
        gift.Price.Should().Be(19.99m);
    }

    [Fact]
    public void Gift_ShouldAllowExpensivePrices()
    {
        // Arrange & Act
        var gift = new Gift { Price = 9999.99m };

        // Assert
        gift.Price.Should().Be(9999.99m);
    }

    [Fact]
    public void Gift_ShouldAllowZeroPrice()
    {
        // Arrange & Act
        var gift = new Gift { Price = 0m };

        // Assert
        gift.Price.Should().Be(0m);
    }

    [Fact]
    public void Gift_ShouldAllowNullPrice()
    {
        // Arrange & Act
        var gift = new Gift { Price = null };

        // Assert
        gift.Price.Should().BeNull();
    }

    [Fact]
    public void Gift_AvailableStatus_ShouldNotHaveTakenByInformation()
    {
        // Arrange & Act
        var gift = new Gift
        {
            Name = "Test Gift",
            Status = GiftStatus.Available,
            TakenByMemberId = null,
            TakenAt = null
        };

        // Assert
        gift.Status.Should().Be(GiftStatus.Available);
        gift.TakenByMemberId.Should().BeNull();
        gift.TakenAt.Should().BeNull();
    }

    [Fact]
    public void Gift_TakenStatus_ShouldHaveTakenByInformation()
    {
        // Arrange
        var takenByMemberId = Guid.NewGuid();
        var takenAt = DateTime.UtcNow;

        // Act
        var gift = new Gift
        {
            Name = "Test Gift",
            Status = GiftStatus.Taken,
            TakenByMemberId = takenByMemberId,
            TakenAt = takenAt
        };

        // Assert
        gift.Status.Should().Be(GiftStatus.Taken);
        gift.TakenByMemberId.Should().Be(takenByMemberId);
        gift.TakenAt.Should().Be(takenAt);
    }
}
