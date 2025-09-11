namespace FluffyVoid.Events;

/// <summary>
///     Local event bus that can track event subscribers and allowing decoupled raising of events by objects within the project
///     Thread safe Event Bus
/// </summary>
public class LocalEventBus
{
    /// <summary>
    ///     Thread locking object to protect data from being accessed from multiple threads
    /// </summary>
    private readonly Lock _threadLock = new Lock();
    /// <summary>
    ///     Event lookup table that associates an event to a list of callbacks
    /// </summary>
    private readonly Dictionary<Type, List<Delegate>> _events;

    /// <summary>
    ///     Default constructor used to initialize the event bus
    /// </summary>
    public LocalEventBus()
    {
        _events = new Dictionary<Type, List<Delegate>>();
    }

    /// <summary>
    ///     Publishes an event out to all subscribed listeners
    /// </summary>
    /// <param name="sender">The publisher of the event</param>
    /// <param name="eventArgs">The event data sent with the event</param>
    /// <typeparam name="TEvent">The type of event to publish</typeparam>
    public void Publish<TEvent>(object sender, TEvent eventArgs)
    {
        Type eventId = typeof(TEvent);
        List<Delegate> callbacks;
        lock (_threadLock)
        {
            if (!_events.ContainsKey(eventId))
            {
                return;
            }

            callbacks = _events[eventId];
        }

        for (int index = 0;
             index < callbacks.Count;
             index++)
        {
            Delegate currentCallback = callbacks[index];
            try
            {
                if (currentCallback is not Action<object, TEvent>
                    callback)
                {
                    callbacks.RemoveAt(index--);
                    continue;
                }

                callback.Invoke(sender, eventArgs);
            }
            catch
            {
                callbacks.RemoveAt(index--);
            }
        }

        lock (_threadLock)
        {
            _events[eventId] = callbacks;
        }
    }
    /// <summary>
    ///     Subscribes a callback to an event
    /// </summary>
    /// <param name="callback">The callback to subscribe to the event with</param>
    /// <typeparam name="TEvent">The type of event to subscribe to</typeparam>
    public void Subscribe<TEvent>(Action<object, TEvent> callback)
        where TEvent : EventArgs
    {
        Type eventId = typeof(TEvent);
        lock (_threadLock)
        {
            if (!_events.ContainsKey(eventId))
            {
                _events[eventId] = new List<Delegate>();
            }

            _events[eventId].Add(callback);
        }
    }
    /// <summary>
    ///     Unsubscribes a callback from an event
    /// </summary>
    /// <param name="callback">The callback to unsubscribe from the event with</param>
    /// <typeparam name="TEvent">The type of event to unsubscribe from</typeparam>
    public void Unsubscribe<TEvent>(Action<object, TEvent> callback)
        where TEvent : EventArgs
    {
        Type eventId = typeof(TEvent);
        lock (_threadLock)
        {
            if (!_events.ContainsKey(eventId))
            {
                return;
            }

            _events[eventId].Remove(callback);
        }
    }
}