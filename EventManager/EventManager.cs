using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Dispatch messages to subcribers, by broadcasting to the subscribers o by delivering just to one of them
/// For targeted events "@", the name, and the instance id of the target is add to the  event name
/// </summary>
public class EventManager : MonoBehaviour {

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
    /// Get the singleton
    /// </summary>
    private static EventManager getSingleton {
        get {
            if (!eventManagerSingleton) {
                EventManager[] eventManagers = FindObjectsOfType(typeof(EventManager)) as EventManager[];
                
                foreach (EventManager eventManager in eventManagers) {
                    if (! eventManager.defaultEventManager) {
                        continue;
                    }

                    if (eventManagerSingleton) {
                        Debug.LogError("Must be only one default EventManger in the active scene.");
                    }
                    eventManagerSingleton = FindObjectOfType(typeof(EventManager)) as EventManager;
                }               

                if (!eventManagerSingleton) {
                    Debug.LogError("Must be one default EventManger (in a GameObject) in the active scene.");
                }
            }

            return eventManagerSingleton;
        }
    }

    /// <summary>
    /// Attacht a callback to eventName
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

    public static void StopListening(string eventName, UnityAction<object> listener, GameObject target = null) {
        if (eventManagerSingleton == null) return;

        assertEventNameIsValid(eventName);

        eventName = targetedEventName(eventName, target);

        ParametrizedEvent thisEvent = null;
        if (getSingleton.eventDictionary.TryGetValue(eventName, out thisEvent)) {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void StopListening(UnityAction<object> listener) {
        if (eventManagerSingleton == null) return;

        foreach (KeyValuePair<string, ParametrizedEvent> thisEvent in getSingleton.eventDictionary ) {
            thisEvent.Value.RemoveListener(listener);
        }
    }

    private static void TriggerEventNoNameCheck(string eventName, object message) {

    }

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

    private static string targetedEventName(string eventName, GameObject target) {
        if (!target) {

            return eventName;
        }
        return "{eventName}@{target.name} ({target.GetInstanceID()})";
    }

    private static void assertEventNameIsValid(string eventName) {
        // Fucked bastards !!! Another regex dialect ? Really ??? :(
        if (! (new Regex(@"^[\w./]+$").IsMatch(eventName))) {
            throw new InvalidEventNameException(eventName);
        };
    }

    private void Update() {
        if (this == getSingleton) {
            TriggerEvent(EventManager.update, null);
        }
    }

    private void AllwaysUpdate() {
        if (this == getSingleton) {
            TriggerEvent(EventManager.allwaysUpdate, null);
        }
    }


}
