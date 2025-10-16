namespace ChristmasGiftCollection.Core.Repositories;

/// <summary>
/// Repository for persisting and retrieving events from EventStore (Kurrent)
/// </summary>
public interface IEventStoreRepository
{
    /// <summary>
    /// Appends an event to a stream
    /// </summary>
    /// <param name="streamName">The stream identifier (typically member ID)</param>
    /// <param name="event">The event to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AppendEventAsync(string streamName, object @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all events from a stream
    /// </summary>
    /// <param name="streamName">The stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of events in order</returns>
    Task<IReadOnlyList<object>> ReadStreamAsync(string streamName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all distinct stream names (for discovering all members)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stream names</returns>
    Task<IReadOnlyList<string>> GetAllStreamNamesAsync(CancellationToken cancellationToken = default);
}
