using Akka.Actor;
using Akka.TestKit.Xunit2;
using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Models;
using ChristmasGiftCollection.Core.Repositories;
using FluentAssertions;
using Moq;

namespace ChristmasGiftCollection.Tests.Actors;

/// <summary>
/// Unit tests for MemberActor with mocked EventStore repository
/// </summary>
public class MemberActorTests : TestKit
{
    [Fact]
    public async Task CreateMember_ShouldPersistEventAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));

        // Act
        var command = new CreateMember(memberId, "John Doe", "john@example.com", null, "Test notes");
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<MemberAdded>();

        var memberAdded = (MemberAdded)capturedEvents[0];
        memberAdded.MemberId.Should().Be(memberId);
        memberAdded.Name.Should().Be("John Doe");
        memberAdded.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task CreateMember_WhenMemberAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();

        // Simulate existing member by returning an existing event
        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "Existing User"}
            });

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));

        // Act
        var command = new CreateMember(memberId, "New User");
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<CommandFailure>();
        var failure = (CommandFailure)result;
        failure.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateMember_ShouldPersistEventAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        // Simulate existing member
        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe" }
            });

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));

        // Wait for actor to load
        await Task.Delay(100);

        // Act
        var command = new UpdateMember(memberId, "John Smith", "john.smith@example.com");
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<MemberUpdated>();

        var memberUpdated = (MemberUpdated)capturedEvents[0];
        memberUpdated.MemberId.Should().Be(memberId);
        memberUpdated.Name.Should().Be("John Smith");
        memberUpdated.Email.Should().Be("john.smith@example.com");
    }

    [Fact]
    public async Task AddGift_ShouldPersistEventAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe" }
            });

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));
        await Task.Delay(100);

        // Act
        var command = new AddGift(memberId, giftId, "Xbox Series X", "Gaming console", null, null, GiftPriority.High);
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<GiftAdded>();

        var giftAdded = (GiftAdded)capturedEvents[0];
        giftAdded.GiftId.Should().Be(giftId);
        giftAdded.Name.Should().Be("Xbox Series X");
        giftAdded.Priority.Should().Be(GiftPriority.High);
    }

    [Fact]
    public async Task TakeGift_ShouldPersistEventAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var takenByMemberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe" },
                new GiftAdded { GiftId = giftId, MemberId = memberId, Name = "Xbox", Priority = GiftPriority.Medium }
            });

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));
        await Task.Delay(100);

        // Act
        var command = new TakeGift(memberId, giftId, takenByMemberId);
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<GiftTaken>();

        var giftTaken = (GiftTaken)capturedEvents[0];
        giftTaken.GiftId.Should().Be(giftId);
        giftTaken.TakenByMemberId.Should().Be(takenByMemberId);
    }

    [Fact]
    public async Task AddRelationship_ShouldPersistEventAndReturnSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe", }
            });

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));
        await Task.Delay(100);

        // Act
        var command = new AddRelationship(memberId, relationshipId, toMemberId, RelationshipType.PartnerOf);
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<RelationshipAdded>();

        var relationshipAdded = (RelationshipAdded)capturedEvents[0];
        relationshipAdded.FromMemberId.Should().Be(memberId);
        relationshipAdded.ToMemberId.Should().Be(toMemberId);
        relationshipAdded.Type.Should().Be(RelationshipType.PartnerOf);
    }

    [Fact]
    public async Task GetMemberState_ShouldReturnRebuiltState()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();
        var toMemberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();

        // Simulate event stream with multiple events
        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe", Email = "john@example.com" },
                new GiftAdded { GiftId = giftId, MemberId = memberId, Name = "Xbox", Priority = GiftPriority.High },
                new RelationshipAdded { RelationshipId = relationshipId, FromMemberId = memberId, ToMemberId = toMemberId, Type = RelationshipType.PartnerOf },
                new MemberUpdated { MemberId = memberId, Name = "John Smith" }
            });

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));
        await Task.Delay(100);

        // Act
        var result = await actor.Ask<Member?>(new GetMemberState(memberId), TimeSpan.FromSeconds(5));

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Smith"); // Updated name
        result.Gifts.Should().HaveCount(1);
        result.Gifts[0].Name.Should().Be("Xbox");
        result.Relationships.Should().HaveCount(1);
        result.Relationships[0].Type.Should().Be(RelationshipType.PartnerOf);
    }

    [Fact]
    public async Task DeleteMember_ShouldMarkAsDeleted()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var mockRepo = new Mock<IEventStoreRepository>();
        var capturedEvents = new List<object>();

        mockRepo.Setup(r => r.ReadStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>
            {
                new MemberAdded { MemberId = memberId, Name = "John Doe" }
            });

        mockRepo.Setup(r => r.AppendEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((stream, evt, ct) => capturedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var actor = Sys.ActorOf(MemberActor.Props(memberId, mockRepo.Object));
        await Task.Delay(100);

        // Act
        var command = new DeleteMember(memberId, "User requested deletion");
        var result = await actor.Ask<object>(command, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeOfType<Success>();
        capturedEvents.Should().HaveCount(1);
        capturedEvents[0].Should().BeOfType<MemberRemoved>();

        var memberRemoved = (MemberRemoved)capturedEvents[0];
        memberRemoved.MemberId.Should().Be(memberId);
        memberRemoved.Reason.Should().Be("User requested deletion");
    }
}
