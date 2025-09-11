namespace FluffyVoid.Events;

/// <summary>
///     Global event bus that can track event subscribers and allowing decoupled raising of events by objects within the project
///     Thread safe Event Bus
/// </summary>
[Serializable]
public class GlobalEventBus
{
    /// <summary>
    ///     Singleton instance member
    /// </summary>
    private static GlobalEventBus? s_instance;
    /// <summary>
    ///     Thread locking object to protect data from being accessed from multiple threads
    /// </summary>
    private static Lock s_threadLock = new Lock();
    /// <summary>
    ///     Event lookup table that associates an event to a list of callbacks
    /// </summary>
    private Dictionary<Type, List<Delegate>> _events;
    /// <summary>
    ///     Instance property that creates the instance during the first call to the EventBus, otherwise the EventBus
    ///     is unallocated
    /// </summary>
    private static GlobalEventBus Instance =>
        s_instance ??= new GlobalEventBus();

    /// <summary>
    ///     Default constructor used to initialize the event bus
    /// </summary>
    private GlobalEventBus()
    {
        _events = new Dictionary<Type, List<Delegate>>();
    }

    /// <summary>
    ///     Returns the current count of subscribers for a desired event type
    /// </summary>
    /// <typeparam name="TEvent">The type of event to get the count of subscribers for</typeparam>
    /// <returns>The number of subscribers to the event</returns>
    public static int Count<TEvent>()
        where TEvent : EventArgs
    {
        int count = 0;
        Type eventKey = typeof(TEvent);
        lock (s_threadLock)
        {
            if (Instance._events.ContainsKey(eventKey))
            {
                count = Instance._events[eventKey].Count;
            }
        }

        return count;
    }
    /// <summary>
    ///     Publishes an event out to all subscribed listeners
    /// </summary>
    /// <param name="sender">The publisher of the event</param>
    /// <param name="eventArgs">The event data sent with the event</param>
    /// <typeparam name="TEvent">The type of event to publish</typeparam>
    public static void Publish<TEvent>(object sender, TEvent eventArgs)
    {
        Type eventId = typeof(TEvent);
        List<Delegate> callbacks;
        lock (s_threadLock)
        {
            if (!Instance._events.ContainsKey(eventId))
            {
                return;
            }

            callbacks = Instance._events[eventId];
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

        lock (s_threadLock)
        {
            Instance._events[eventId] = callbacks;
        }
    }
    /// <summary>
    ///     Subscribes a callback to an event
    /// </summary>
    /// <param name="callback">The callback to subscribe to the event with</param>
    /// <typeparam name="TEvent">The type of event to subscribe to</typeparam>
    public static void Subscribe<TEvent>(Action<object, TEvent> callback)
        where TEvent : EventArgs
    {
        Type eventId = typeof(TEvent);
        lock (s_threadLock)
        {
            if (!Instance._events.ContainsKey(eventId))
            {
                Instance._events[eventId] = new List<Delegate>();
            }

            Instance._events[eventId].Add(callback);
        }
    }
    /// <summary>
    ///     Unsubscribes a callback from an event
    /// </summary>
    /// <param name="callback">The callback to unsubscribe from the event with</param>
    /// <typeparam name="TEvent">The type of event to unsubscribe from</typeparam>
    public static void Unsubscribe<TEvent>(Action<object, TEvent> callback)
        where TEvent : EventArgs
    {
        Type eventId = typeof(TEvent);
        lock (s_threadLock)
        {
            if (!Instance._events.ContainsKey(eventId))
            {
                return;
            }

            Instance._events[eventId].Remove(callback);
        }
    }
}