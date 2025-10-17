using Akka.Actor;
using Akka.Event;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Repositories;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Actor that handles commands for a single member and maintains their aggregate state
/// Each member gets their own actor instance
/// </summary>
public class MemberActor : ReceiveActor, IWithStash
{
    private readonly IEventStoreRepository _eventStore;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Guid _memberId;
    private readonly string _streamName;
    private MemberAggregate _state = new();

    public IStash Stash { get; set; } = null!;

    public MemberActor(Guid memberId, IEventStoreRepository eventStore)
    {
        _memberId = memberId;
        _eventStore = eventStore;
        _streamName = $"member-{memberId}";

        // Load state from events on actor initialization
        Become(Loading);
    }

    private void Loading()
    {
        ReceiveAsync<LoadState>(async _ =>
        {
            try
            {
                _log.Info($"Loading state for member {_memberId}");

                // Load all events for this member from EventStore
                var events = await _eventStore.ReadStreamAsync(_streamName);

                // Replay events to build current state
                foreach (var evt in events)
                {
                    ApplyEvent(evt);
                }

                _log.Info($"Loaded {events.Count} events for member {_memberId}");

                // Switch to ready state
                Become(Ready);

                // Process any stashed messages
                Stash.UnstashAll();
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to load state for member {_memberId}");
                throw;
            }
        });

        // Stash all other messages until loading is complete
        ReceiveAny(_ => Stash.Stash());
    }

    private void Ready()
    {
        // Handle CreateMember command
        Receive<CreateMember>(cmd =>
        {
            if (_state.Id != Guid.Empty && !_state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} already exists") });
                return;
            }

            var evt = new MemberAdded
            {
                MemberId = cmd.MemberId,
                Name = cmd.Name,
                Email = cmd.Email,
                DateOfBirth = cmd.DateOfBirth,
                Notes = cmd.Notes
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Member created successfully" });
        });

        // Handle UpdateMember command
        Receive<UpdateMember>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new MemberUpdated
            {
                MemberId = cmd.MemberId,
                Name = cmd.Name,
                Email = cmd.Email,
                DateOfBirth = cmd.DateOfBirth,
                Notes = cmd.Notes
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Member updated successfully" });
        });

        // Handle DeleteMember command
        Receive<DeleteMember>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new MemberRemoved
            {
                MemberId = cmd.MemberId,
                Reason = cmd.Reason
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Member deleted successfully" });
        });

        // Handle AddGift command
        Receive<AddGift>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new GiftAdded
            {
                GiftId = cmd.GiftId,
                MemberId = cmd.MemberId,
                Name = cmd.Name,
                Description = cmd.Description,
                Priority = cmd.Priority
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gift added successfully" });
        });

        // Handle UpdateGift command
        Receive<UpdateGift>(cmd =>
        {
            if (!_state.Gifts.ContainsKey(cmd.GiftId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Gift {cmd.GiftId} not found") });
                return;
            }

            var evt = new GiftUpdated
            {
                GiftId = cmd.GiftId,
                Name = cmd.Name,
                Description = cmd.Description,
                Priority = cmd.Priority
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gift updated successfully" });
        });

        // Handle TakeGift command
        Receive<TakeGift>(cmd =>
        {
            if (!_state.Gifts.ContainsKey(cmd.GiftId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Gift {cmd.GiftId} not found") });
                return;
            }

            var gift = _state.Gifts[cmd.GiftId];
            if (gift.Status == Models.GiftStatus.Taken)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Gift {cmd.GiftId} is already taken") });
                return;
            }

            var evt = new GiftTaken
            {
                GiftId = cmd.GiftId,
                TakenByMemberId = cmd.TakenByMemberId
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gift taken successfully" });
        });

        // Handle ReleaseGift command
        Receive<ReleaseGift>(cmd =>
        {
            if (!_state.Gifts.ContainsKey(cmd.GiftId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Gift {cmd.GiftId} not found") });
                return;
            }

            var evt = new GiftReleased
            {
                GiftId = cmd.GiftId,
                ReleasedByMemberId = cmd.ReleasedByMemberId
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gift released successfully" });
        });

        // Handle RemoveGift command
        Receive<RemoveGift>(cmd =>
        {
            if (!_state.Gifts.ContainsKey(cmd.GiftId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Gift {cmd.GiftId} not found") });
                return;
            }

            var evt = new GiftRemoved
            {
                GiftId = cmd.GiftId,
                MemberId = cmd.MemberId,
                Reason = cmd.Reason
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gift removed successfully" });
        });

        // Handle AddRelationship command
        Receive<AddRelationship>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new RelationshipAdded
            {
                RelationshipId = cmd.RelationshipId,
                FromMemberId = cmd.MemberId,
                ToMemberId = cmd.ToMemberId,
                Type = cmd.Type
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Relationship added successfully" });
        });

        // Handle UpdateRelationship command
        Receive<UpdateRelationship>(cmd =>
        {
            if (!_state.Relationships.ContainsKey(cmd.RelationshipId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Relationship {cmd.RelationshipId} not found") });
                return;
            }

            var evt = new RelationshipUpdated
            {
                RelationshipId = cmd.RelationshipId,
                NewType = cmd.NewType
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Relationship updated successfully" });
        });

        // Handle RemoveRelationship command
        Receive<RemoveRelationship>(cmd =>
        {
            if (!_state.Relationships.ContainsKey(cmd.RelationshipId))
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Relationship {cmd.RelationshipId} not found") });
                return;
            }

            var evt = new RelationshipRemoved
            {
                RelationshipId = cmd.RelationshipId,
                FromMemberId = cmd.MemberId,
                ToMemberId = cmd.ToMemberId
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Relationship removed successfully" });
        });

        // Handle SetPinCode command
        Receive<SetPinCode>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new MemberPinCodeSet
            {
                MemberId = cmd.MemberId,
                PinCode = cmd.PinCode
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "PIN code set successfully" });
        });

        // Handle ReorderGifts command
        Receive<ReorderGifts>(cmd =>
        {
            if (_state.Id == Guid.Empty || _state.IsDeleted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Member {_memberId} not found") });
                return;
            }

            var evt = new GiftsReordered
            {
                MemberId = cmd.MemberId,
                GiftOrders = cmd.GiftOrders
            };

            PersistEvent(evt);
            Sender.Tell(new Success { Message = "Gifts reordered successfully" });
        });

        // Handle GetMemberState query
        Receive<GetMemberState>(_ =>
        {
            if (_state.Id == Guid.Empty)
            {
                Sender.Tell(null);
            }
            else
            {
                Sender.Tell(_state.ToModel());
            }
        });
    }

    private void PersistEvent(object evt)
    {
        try
        {
            // Append event to member's stream in EventStore
            _eventStore.AppendEventAsync(_streamName, evt).Wait();

            // Apply event to in-memory state
            ApplyEvent(evt);

            _log.Debug($"Event {evt.GetType().Name} persisted for member {_memberId}");
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to persist event {evt.GetType().Name} for member {_memberId}");
            throw;
        }
    }

    private void ApplyEvent(object evt)
    {
        switch (evt)
        {
            case MemberAdded e:
                _state.Apply(e);
                break;
            case MemberUpdated e:
                _state.Apply(e);
                break;
            case MemberRemoved e:
                _state.Apply(e);
                break;
            case GiftAdded e:
                _state.Apply(e);
                break;
            case GiftUpdated e:
                _state.Apply(e);
                break;
            case GiftTaken e:
                _state.Apply(e);
                break;
            case GiftReleased e:
                _state.Apply(e);
                break;
            case GiftStatusChanged e:
                _state.Apply(e);
                break;
            case GiftRemoved e:
                _state.Apply(e);
                break;
            case GiftsReordered e:
                _state.Apply(e);
                break;
            case RelationshipAdded e:
                _state.Apply(e);
                break;
            case RelationshipUpdated e:
                _state.Apply(e);
                break;
            case RelationshipRemoved e:
                _state.Apply(e);
                break;
            case MemberPinCodeSet e:
                _state.Apply(e);
                break;
        }
    }

    protected override void PreStart()
    {
        base.PreStart();
        Self.Tell(new LoadState());
    }

    public static Props Props(Guid memberId, IEventStoreRepository eventStore) =>
        Akka.Actor.Props.Create(() => new MemberActor(memberId, eventStore));
}

// Internal messages
internal record LoadState;

/// <summary>
/// Success response
/// </summary>
public record Success
{
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Failure response
/// </summary>
public record CommandFailure
{
    public Exception Exception { get; init; } = new Exception("Unknown error");
}
