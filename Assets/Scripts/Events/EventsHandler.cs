using System.Collections.Generic;
public class EventsHandler
{
    public delegate void EventReceiver();
    private static Dictionary<string, EventReceiver> _events;
    public static void SubscribeToEvent(string eventName, EventReceiver listener)
    {
        if (_events == null)
            _events = new Dictionary<string, EventReceiver>();
        if (!_events.ContainsKey(eventName))
            _events.Add(eventName, null);
        _events[eventName] += listener;
    }
    public static void UnsubscribeToEvent(string eventName, EventReceiver listener)
    {
        if (_events != null)
            if (_events.ContainsKey(eventName))
                _events[eventName] -= listener;
    }
    public static void TriggerEvent(string eventName)
    {
        if (_events == null)
        {
            UnityEngine.Debug.LogWarning("No events subscribed");
            return;
        }
        if (_events.ContainsKey(eventName))
            _events[eventName]?.Invoke();
    }
}