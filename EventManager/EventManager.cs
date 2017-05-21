using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System;

struct DelayedEvent {
    public string eventName;
    public object message;
    public GameObject target;
    public float when;

    public DelayedEvent(string eventName, object message, GameObject target, float when) {
        this.eventName = eventName;
        this.message = message;
        this.target = target;
        this.when = when;
    }
}

struct OwnedListener {
    public UnityEngine.Object owner;
    public UnityAction<object> listener;
    public string eventName;
    public GameObject target;

    public OwnedListener(UnityEngine.Object owner, UnityAction<object> listener, string eventName, GameObject target) {
        this.owner     = owner;
        this.listener  = listener;
        this.eventName = eventName;
        this.target    = target;
    }
}

/// <summary>
/// Dispatch messages to subcribers, by broadcasting to the subscribers o by delivering just to one of them
/// For targeted events "@", the name, and the instance id of the target is add to the  event name
/// </summary>
public class EventManager : MonoBehaviour {

    /// <summary>
    /// Messages that EventManager can send by itself
    /// </summary>
    public const string update = "EventManager.update";
    public const string allwaysUpdate = "EventManager.allwaysUpdate";

    /// <summary>
    /// Is the default one, just can be one, that uses singleton
    /// </summary>
    public bool defaultEventManager = true;

    /// <summary>
    /// Should debug all events ?
    /// </summary>
    public bool debugEvents = true;

    /// <summary>
    /// Stored the subcribers lists
    /// </summary>
    private Dictionary<string, ParametrizedEvent> eventDictionary = new Dictionary<string, ParametrizedEvent>();

    /// <summary>
    /// The singleton, ie. the instance that will works as default(singleton) one
    /// </summary>
    private static EventManager eventManagerSingleton;

    /// <summary>
    /// Subscribers list
    /// </summary>
    private Dictionary<string, ArrayList> subscribers = new Dictionary<string, ArrayList>();

    /// <summary>
    /// Delayed events
    /// </summary>
    private ArrayList delayedEvents = new ArrayList();

    /// <summary>
    /// Get the singleton
    /// </summary>
    private static EventManager getSingleton {
        get {
            if (!eventManagerSingleton) {
                EventManager[] eventManagers = FindObjectsOfType(typeof(EventManager)) as EventManager[];

                foreach (EventManager eventManager in eventManagers) {
                    if (!eventManager.defaultEventManager) {
                        continue;
                    }

                    if (eventManagerSingleton) {
                        Debug.LogError("Must be only one default EventManger");
                    }
                    eventManagerSingleton = FindObjectOfType(typeof(EventManager)) as EventManager;
                }

                if (!eventManagerSingleton) {
                    Debug.LogError("Must be one default EventManger (in a GameObject)");
                }
            }

            return eventManagerSingleton;
        }
    }


    public static void StartListening(UnityEngine.Object subscriber, string eventName, UnityAction<object> listener, GameObject target = null) {
        EventManager singleton = getSingleton;
        string key = "" + subscriber.GetInstanceID();

        if (! singleton.subscribers.ContainsKey(key)) {
            singleton.subscribers.Add(key, new ArrayList());
        }
        ArrayList namedEvents;
        singleton.subscribers.TryGetValue(key, out namedEvents);
        namedEvents.Add(new OwnedListener(subscriber, listener, eventName, target));

        StartListening(eventName, listener, target);
    }

    public static void StopListening(UnityEngine.Object subscriber) {
        EventManager singleton = getSingleton;
        string key = "" + subscriber.GetInstanceID();

        if (! singleton.subscribers.ContainsKey(key)) {
            return;
        }

        ArrayList ownedEvents;
        singleton.subscribers.TryGetValue(key, out ownedEvents);
        foreach (OwnedListener ownedListener in ownedEvents) {
            StopListening(ownedListener.eventName, ownedListener.listener, ownedListener.target);            
        }
        singleton.subscribers.Remove(key);
    }

    /// <summary>
    /// Attaches a callback to eventName, ei. starts listening for eventName.
    /// If a target is specified, then just targeted events will be received, if no target is specified, targeted events will not be attached (received).
    /// </summary>
    /// <param name="eventName">The event name</param>
    /// <param name="listener">The callback yo be called</param>
    /// <param name="target">If set, the </param>    
    public static void StartListening(string eventName, UnityAction<object> listener, GameObject target = null) {
        ParametrizedEvent thisEvent = null;

        assertEventNameIsValid(eventName);

        eventName = targetedEventName(eventName, target);

        if (getSingleton.eventDictionary.TryGetValue(eventName, out thisEvent)) {
            thisEvent.AddListener(listener);
        }
        else {
            thisEvent = new ParametrizedEvent();
            thisEvent.AddListener(listener);
            getSingleton.eventDictionary.Add(eventName, thisEvent);
        }

        if (getSingleton.debugEvents) {
            Debug.Log("Event listener added : " + eventName);
        }
    }

    /// <summary>
    /// Deataches the given listener to eventName event.
    /// If a target is specified, then just targeted events will be deattached, if no target is specified, targeted events will not be deattached.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    /// <param name="target"></param>
    public static void StopListening(string eventName, UnityAction<object> listener, GameObject target = null) {
        if (eventManagerSingleton == null) return;

        assertEventNameIsValid(eventName);

        eventName = targetedEventName(eventName, target);

        ParametrizedEvent thisEvent = null;
        if (getSingleton.eventDictionary.TryGetValue(eventName, out thisEvent)) {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// Deattaches all events (targeted or not) at wich listener is attached
    /// </summary>
    /// <param name="listener"></param>
    public static void StopListening(UnityAction<object> listener) {
        if (eventManagerSingleton == null) return;

        foreach (KeyValuePair<string, ParametrizedEvent> thisEvent in getSingleton.eventDictionary) {
            thisEvent.Value.RemoveListener(listener);
        }
    }

    /// <summary>
    /// Sends the message to all attached listeners
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="message"></param>
    /// <param name="target"></param>
    public static void TriggerEvent(string eventName, object message, GameObject target = null) {
        assertEventNameIsValid(eventName);

        if (getSingleton.debugEvents) {
            Debug.Log("Event triggered : " + eventName);
        }

        eventName = targetedEventName(eventName, target);

        ParametrizedEvent thisEvent = null;
        if (getSingleton.eventDictionary.TryGetValue(eventName, out thisEvent)) {
            thisEvent.Invoke(message);
        }
    }

    /// <summary>
    /// Sends a message after some amount of time, that is done on allwaysUpdate loop, so the precision is tge one of that. 
    /// </summary>
    /// <param name="time">Sends the message after time seconds</param>
    /// <param name="eventName"></param>
    /// <param name="message"></param>
    /// <param name="target"></param>
    public static void TriggerEventAfter(float time, string eventName, object message, GameObject target = null) {
        getSingleton.delayedEvents.Add(new DelayedEvent(eventName, message, target, Time.realtimeSinceStartup + time));
    }

    /// <summary>
    /// Calculates the targeted event name for an event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static string targetedEventName(string eventName, GameObject target) {
        if (!target) {

            return eventName;
        }
        return eventName + "@" + target.name + "(" + target.GetInstanceID() + ")";
    }

    /// <summary>
    /// Checks if the eventName is valid or nor
    /// </summary>
    /// <param name="eventName"></param>
    private static void assertEventNameIsValid(string eventName) {
        // Fucked bastards !!! Another regex dialect ? Really ??? :(
        if (!(new Regex(@"^[\w./]+$").IsMatch(eventName))) {
            throw new InvalidEventNameException(eventName);
        };
    }

    /// <summary>
    /// Sends a tick messatge to listners subscribed to EventManager.update
    /// </summary>
    private void Update() {
        TriggerEvent(EventManager.update, null);
    }

    /// <summary>
    /// Sends a tick messatge to listners subscribed to EventManager.allwaysUpdate
    /// </summary>
    private void AllwaysUpdate() {
        TriggerEvent(EventManager.allwaysUpdate, null);

        float now = Time.realtimeSinceStartup;

        foreach (DelayedEvent theEvent in delayedEvents) {
            if (theEvent.when >= now) {
                TriggerEvent(theEvent.eventName, theEvent.message, theEvent.target);
                delayedEvents.Remove(theEvent);
            }
        }
    }
}
