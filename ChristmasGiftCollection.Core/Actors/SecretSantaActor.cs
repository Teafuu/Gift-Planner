using Akka.Actor;
using Akka.Event;
using ChristmasGiftCollection.Core.Actors.Commands;
using ChristmasGiftCollection.Core.Events;
using ChristmasGiftCollection.Core.Repositories;

namespace ChristmasGiftCollection.Core.Actors;

/// <summary>
/// Actor that handles commands for a single Secret Santa raffle and maintains its aggregate state
/// Each raffle gets its own actor instance
/// </summary>
public class SecretSantaActor : ReceiveActor, IWithStash
{
    private readonly IEventStoreRepository _eventStore;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Guid _raffleId;
    private readonly string _streamName;
    private SecretSantaAggregate _state = new();

    public IStash Stash { get; set; } = null!;

    public SecretSantaActor(Guid raffleId, IEventStoreRepository eventStore)
    {
        _raffleId = raffleId;
        _eventStore = eventStore;
        _streamName = $"secretsanta-{raffleId}";

        // Load state from events on actor initialization
        Become(Loading);
    }

    private void Loading()
    {
        ReceiveAsync<LoadState>(async _ =>
        {
            try
            {
                _log.Info($"Loading state for Secret Santa raffle {_raffleId}");

                // Load all events for this raffle from EventStore
                var events = await _eventStore.ReadStreamAsync(_streamName);

                // Replay events to build current state
                foreach (var evt in events)
                {
                    ApplyEvent(evt);
                }

                _log.Info($"Loaded {events.Count} events for raffle {_raffleId}");

                // Switch to ready state
                Become(Ready);

                // Process any stashed messages
                Stash.UnstashAll();
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to load state for raffle {_raffleId}");
                throw;
            }
        });

        // Stash all other messages until loading is complete
        ReceiveAny(_ => Stash.Stash());
    }

    private void Ready()
    {
        // Handle CreateSecretSantaRaffle command
        Receive<CreateSecretSantaRaffle>(cmd =>
        {
            if (_state.Id != Guid.Empty && !_state.IsCancelled)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Raffle {_raffleId} already exists") });
                return;
            }

            // Validate participant count is even
            if (cmd.ParticipantIds.Count % 2 != 0)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException("Participant count must be even") });
                return;
            }

            if (cmd.ParticipantIds.Count < 2)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException("At least 2 participants required") });
                return;
            }

            var evt = new SecretSantaRaffleCreated
            {
                RaffleId = cmd.RaffleId,
                Name = cmd.Name,
                ParticipantIds = cmd.ParticipantIds,
                Budget = cmd.Budget,
                CreatedByMemberId = cmd.CreatedByMemberId,
                Year = cmd.Year
            };

            PersistEvent(evt);
            Sender.Tell(new Success());
        });

        // Handle ExecuteSecretSantaRaffle command
        Receive<ExecuteSecretSantaRaffle>(cmd =>
        {
            if (_state.Id == Guid.Empty)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Raffle {_raffleId} not found") });
                return;
            }

            if (_state.IsCancelled)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException("Cannot execute a cancelled raffle") });
                return;
            }

            if (_state.IsExecuted)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException("Raffle has already been executed") });
                return;
            }

            // Perform the Secret Santa assignment
            var assignments = PerformSecretSantaAssignment(_state.ParticipantIds);

            var evt = new SecretSantaRaffleExecuted
            {
                RaffleId = cmd.RaffleId,
                Assignments = assignments
            };

            PersistEvent(evt);
            Sender.Tell(new Success());
        });

        // Handle CancelSecretSantaRaffle command
        Receive<CancelSecretSantaRaffle>(cmd =>
        {
            if (_state.Id == Guid.Empty)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException($"Raffle {_raffleId} not found") });
                return;
            }

            if (_state.IsCancelled)
            {
                Sender.Tell(new CommandFailure { Exception = new InvalidOperationException("Raffle is already cancelled") });
                return;
            }

            var evt = new SecretSantaRaffleCancelled
            {
                RaffleId = cmd.RaffleId,
                Reason = cmd.Reason,
                CancelledByMemberId = cmd.CancelledByMemberId
            };

            PersistEvent(evt);
            Sender.Tell(new Success());
        });

        // Handle GetSecretSantaState query
        Receive<GetSecretSantaState>(_ =>
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

    /// <summary>
    /// Performs Secret Santa assignment ensuring no one gets themselves
    /// Uses Fisher-Yates shuffle with cycle validation
    /// </summary>
    private Dictionary<Guid, Guid> PerformSecretSantaAssignment(List<Guid> participants)
    {
        if (participants == null) throw new ArgumentNullException(nameof(participants));
        if (participants.Count < 2) throw new ArgumentException("Need at least 2 participants.");

        var rnd = new Random();            // consider injecting/reusing for testability
        var a = new List<Guid>(participants);
        int n = a.Count;

        // Sattolo's algorithm: produces a single n-cycle (no fixed points)
        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(0, i);        // note: j < i (differs from Fisher–Yates)
            (a[i], a[j]) = (a[j], a[i]);
        }

        var assignments = new Dictionary<Guid, Guid>(n);
        for (int i = 0; i < n; i++)
        {
            assignments[a[i]] = a[(i + 1) % n]; // circular pairing
        }

        return assignments;
    }

    private void PersistEvent(object evt)
    {
        try
        {
            // Append event to raffle's stream in EventStore
            _eventStore.AppendEventAsync(_streamName, evt).Wait();

            // Apply event to current state
            ApplyEvent(evt);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to persist event for raffle {_raffleId}");
            throw;
        }
    }

    private void ApplyEvent(object evt)
    {
        switch (evt)
        {
            case SecretSantaRaffleCreated e:
                _state.Apply(e);
                break;
            case SecretSantaRaffleExecuted e:
                _state.Apply(e);
                break;
            case SecretSantaRaffleCancelled e:
                _state.Apply(e);
                break;
        }
    }

    protected override void PreStart()
    {
        Self.Tell(new LoadState());
    }

    public static Props Props(Guid raffleId, IEventStoreRepository eventStore) =>
        Akka.Actor.Props.Create(() => new SecretSantaActor(raffleId, eventStore));
}
