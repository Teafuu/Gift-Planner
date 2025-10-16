using EventStore.Client;
using System.Text;
using System.Text.Json;

namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// EventStore (Kurrent) implementation of the event repository
/// </summary>
public class EventStoreRepository : IEventStoreRepository
{
    private readonly EventStoreClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventStoreRepository(EventStoreClient client)
    {
        _client = client;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task AppendEventAsync(string streamName, object @event, CancellationToken cancellationToken = default)
    {
        var eventType = @event.GetType().Name;
        var eventData = SerializeEvent(@event, eventType);

        await _client.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            new[] { eventData },
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<object>> ReadStreamAsync(string streamName, CancellationToken cancellationToken = default)
    {
        var events = new List<object>();

        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var state = await result.ReadState;

        // Stream doesn't exist yet
        if (state == ReadState.StreamNotFound)
        {
            return events;
        }

        await foreach (var resolvedEvent in result)
        {
            var deserializedEvent = DeserializeEvent(resolvedEvent);
            if (deserializedEvent != null)
            {
                events.Add(deserializedEvent);
            }
        }

        return events;
    }

    public async Task<IReadOnlyList<string>> GetAllStreamNamesAsync(CancellationToken cancellationToken = default)
    {
        var streamNames = new List<string>();

        // Read from $streams system stream to get all streams
        // Note: This requires system projections to be enabled
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            "$streams",
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var state = await result.ReadState;

        if (state == ReadState.StreamNotFound)
        {
            return streamNames;
        }

        await foreach (var resolvedEvent in result)
        {
            if (resolvedEvent.Event.EventType == "$>")
            {
                var streamName = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
                // Filter out system streams (those starting with $)
                if (!streamName.StartsWith("$"))
                {
                    streamNames.Add(streamName);
                }
            }
        }

        return streamNames;
    }

    private EventData SerializeEvent(object @event, string eventType)
    {
        var json = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions);
        var data = Encoding.UTF8.GetBytes(json);

        // Store the CLR type name in metadata for deserialization
        var metadata = JsonSerializer.Serialize(new { ClrType = @event.GetType().AssemblyQualifiedName }, _jsonOptions);
        var metadataBytes = Encoding.UTF8.GetBytes(metadata);

        return new EventData(
            Uuid.NewUuid(),
            eventType,
            data,
            metadataBytes);
    }

    private object? DeserializeEvent(ResolvedEvent resolvedEvent)
    {
        try
        {
            // Get CLR type from metadata
            var metadataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span);
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metadataJson, _jsonOptions);

            if (metadata == null || !metadata.TryGetValue("clrType", out var clrTypeElement))
            {
                return null;
            }

            var clrTypeName = clrTypeElement.GetString();
            if (string.IsNullOrEmpty(clrTypeName))
            {
                return null;
            }

            var type = Type.GetType(clrTypeName);
            if (type == null)
            {
                return null;
            }

            var json = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            return JsonSerializer.Deserialize(json, type, _jsonOptions);
        }
        catch
        {
            // Log error in production
            return null;
        }
    }
}
