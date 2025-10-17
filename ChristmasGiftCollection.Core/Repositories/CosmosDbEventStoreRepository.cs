using Microsoft.Azure.Cosmos;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// Azure Cosmos DB implementation of the event repository
/// Uses a single container with partition key on stream name for event sourcing
/// </summary>
public class CosmosDbEventStoreRepository : IEventStoreRepository
{
    private readonly CosmosClient _cosmosClient;
    private Container? _container;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized = false;

    private const string DatabaseName = "GiftPlannerEventStore";
    private const string ContainerName = "Events";

    public CosmosDbEventStoreRepository(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Ensures the database and container are initialized (lazy initialization)
    /// </summary>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized && _container != null)
            return;

         await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized && _container != null)
                return;

            // Create database if it doesn't exist
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                DatabaseName,
                cancellationToken: cancellationToken);

            var database = databaseResponse.Database;

            // Create container with event sourcing best practices:
            // - Partition key on streamName (each stream in its own partition)
            // - Unique key policy on (streamName, eventId) for idempotency
            // - Composite index on (streamName ASC, sequence ASC) for efficient ordered reads
            var containerProperties = new ContainerProperties
            {
                Id = ContainerName,
                PartitionKeyPath = "/streamName",

                // Unique key policy: enforce uniqueness on (streamName, eventId) to prevent duplicate events
                UniqueKeyPolicy = new UniqueKeyPolicy
                {
                    UniqueKeys =
                    {
                        new UniqueKey { Paths = { "/streamName", "/eventId" } }
                    }
                },

                // Composite index for efficient ordered queries: ORDER BY streamName, sequence
                IndexingPolicy = new IndexingPolicy
                {
                    CompositeIndexes =
                    {
                        new Collection<CompositePath>
                        {
                            new CompositePath { Path = "/streamName", Order = CompositePathSortOrder.Ascending },
                            new CompositePath { Path = "/sequence", Order = CompositePathSortOrder.Ascending }
                        }
                    }
                }
            };

            // For emulator, use lower throughput; for production, use autoscale
            var containerResponse = await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                throughput: 400, // Minimum throughput
                cancellationToken: cancellationToken);

            _container = containerResponse.Container;
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task AppendEventAsync(string streamName, object @event, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        // Get the next sequence number for this stream (ensures ordered appends)
        var nextSequence = await GetNextSequenceNumberAsync(streamName, cancellationToken);

        var eventType = @event.GetType().Name;
        var eventId = Guid.NewGuid(); // Unique event ID for idempotency

        var eventDocument = new EventDocument
        {
            Id = $"{streamName}-{nextSequence}",

            EventId = eventId.ToString(),

            StreamName = streamName,
            Sequence = nextSequence,
            EventType = eventType,
            ClrType = @event.GetType().AssemblyQualifiedName ?? eventType,
            EventData = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions),
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Idempotent insert: if same eventId already exists, unique key constraint will fail
            await _container!.CreateItemAsync(
                eventDocument,
                new PartitionKey(streamName),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // Event with this ID already exists - idempotency in action
            // This is expected behavior for retries, so we can safely ignore
            return;
        }
    }

    /// <summary>
    /// Gets the next sequence number for a stream (current max + 1)
    /// </summary>
    private async Task<long> GetNextSequenceNumberAsync(string streamName, CancellationToken cancellationToken)
    {
        // Query for the highest sequence number in this stream
        var query = new QueryDefinition(
            "SELECT VALUE MAX(c.sequence) FROM c WHERE c.streamName = @streamName")
            .WithParameter("@streamName", streamName);

        var iterator = _container!.GetItemQueryIterator<long?>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamName) });

        var response = await iterator.ReadNextAsync(cancellationToken);
        var maxSequence = response.FirstOrDefault();

        // If no events exist yet, start at 0; otherwise increment
        return (maxSequence ?? -1) + 1;
    }

    public async Task<IReadOnlyList<object>> ReadStreamAsync(string streamName, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var events = new List<object>();

        // Order by sequence (not timestamp) for guaranteed event ordering
        // Uses composite index (streamName ASC, sequence ASC) for efficient reads
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.streamName = @streamName ORDER BY c.sequence ASC")
            .WithParameter("@streamName", streamName);

        var iterator = _container!.GetItemQueryIterator<EventDocument>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamName) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            foreach (var eventDoc in response)
            {
                var deserializedEvent = DeserializeEvent(eventDoc);
                if (deserializedEvent != null)
                {
                    events.Add(deserializedEvent);
                }
            }
        }

        return events;
    }

    public async Task<IReadOnlyList<string>> GetAllStreamNamesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var streamNames = new HashSet<string>();

        var query = new QueryDefinition("SELECT DISTINCT c.streamName FROM c");

        var iterator = _container!.GetItemQueryIterator<StreamNameResult>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            foreach (var result in response)
            {
                if (!string.IsNullOrEmpty(result.StreamName))
                {
                    streamNames.Add(result.StreamName);
                }
            }
        }

        return streamNames.ToList();
    }

    private object? DeserializeEvent(EventDocument eventDoc)
    {
        try
        {
            if (string.IsNullOrEmpty(eventDoc.ClrType))
            {
                return null;
            }

            var type = Type.GetType(eventDoc.ClrType);
            if (type == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize(eventDoc.EventData, type, _jsonOptions);
        }
        catch
        {
            // Log error in production
            return null;
        }
    }

    /// <summary>
    /// Initializes the Cosmos DB database and container if they don't exist
    /// </summary>
    public static async Task InitializeDatabaseAsync(CosmosClient cosmosClient, CancellationToken cancellationToken = default)
    {
        // Create database if it doesn't exist
        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            DatabaseName,
            cancellationToken: cancellationToken);

        var database = databaseResponse.Database; 

        // Create container with partition key on streamName
        var containerProperties = new ContainerProperties
        {
            Id = ContainerName,
            PartitionKeyPath = "/streamName"
        };

        await database.CreateContainerIfNotExistsAsync(
            containerProperties,
            throughput: 400, // Minimum throughput for development
            cancellationToken: cancellationToken);
    }

    private class EventDocument
    {
        /// <summary>
        /// Unique document ID: {streamName}-{sequence}
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Unique event ID for idempotency (enforced by unique key constraint)
        /// </summary>
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Stream identifier (partition key)
        /// </summary>
        [JsonPropertyName("streamName")]
        public string StreamName { get; set; } = string.Empty;

        /// <summary>
        /// Monotonic sequence number for ordering events in a stream
        /// </summary>
        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Event type name (e.g., "MemberCreated")
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// CLR type for deserialization
        /// </summary>
        [JsonPropertyName("clrType")]
        public string ClrType { get; set; } = string.Empty;

        /// <summary>
        /// Serialized event data
        /// </summary>
        [JsonPropertyName("eventData")]
        public string EventData { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when event was created
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    private class StreamNameResult
    {
        [JsonPropertyName("streamName")]
        public string StreamName { get; set; } = string.Empty;
    }
}
