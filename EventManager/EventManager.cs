using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

namespace JordiBisbal.EventManager {
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
        /// <param name="quiet">If we try to get the singleton and it is not already created, do not emit any error to console</param>
        /// <returns></returns>
        private static EventManager getSingleton(bool quiet = false) {
            if (!eventManagerSingleton) {
                EventManager[] eventManagers = { };
                try {
                    eventManagers = FindObjectsOfType(typeof(EventManager)) as EventManager[];
                }
                catch (System.Exception) { }

                foreach (EventManager eventManager in eventManagers) {
                    if (!eventManager.defaultEventManager) {
                        continue;
                    }

                    if (eventManagerSingleton) {
                        Debug.LogError("There must be only one default EventManger");
                    }
                    eventManagerSingleton = FindObjectOfType(typeof(EventManager)) as EventManager;
                }

                if (!eventManagerSingleton && !quiet) {
                    Debug.LogError("There must be one default EventManger (in a GameObject)");
                }
            }
            return eventManagerSingleton;
        }

        /// <summary>
        /// Subscribe a given object to an event, either for targeted or not events, the even if subscribed as subscriber so all
        /// event subscribed this way, can be freed by just calling to StopListering(subscriber)
        /// </summary>
        /// <param name="subscriber">Object the event system will use the instance Id to subscribe, this will be used when unsubscribing</param>
        /// <param name="eventName">Name of the event the listener will respond to</param>
        /// <param name="listener">Listener Action to catch the event</param>
        /// <param name="target">If </param>
        public static void StartListening(Object subscriber, string eventName, UnityAction<object> listener, Object target = null) {
            EventManager singleton = getSingleton();
            string key = "" + subscriber.GetInstanceID();

            if (!singleton.subscribers.ContainsKey(key)) {
                singleton.subscribers.Add(key, new ArrayList());
            }
            ArrayList namedEvents;
            singleton.subscribers.TryGetValue(key, out namedEvents);
            namedEvents.Add(new OwnedListener(subscriber, listener, eventName, target));

            StartListening(eventName, listener, target);
        }

        /// <summary>
        /// Attaches a callback to eventName, ei. starts listening for eventName.
        /// If a target is specified, then just targeted events will be received, if no target is specified, targeted events will not be attached (received)
        /// To free the event registration, StopListering(eventName, listener) must be called, ie. the unsubcription needs booth the event name and the callback to be done
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public static void StartListening(string eventName, UnityAction<object> listener, UnityEngine.Object target = null) {
            ParametrizedEvent thisEvent = null;

            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            if (getSingleton().eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.AddListener(listener);
            }
            else {
                thisEvent = new ParametrizedEvent();
                thisEvent.AddListener(listener);
                getSingleton().eventDictionary.Add(eventName, thisEvent);
            }

            if (getSingleton().debugEvents) {
                Debug.Log("Event listener added : " + eventName);
            }
        }

        /// <summary>
        /// Unsubstibe to all events that where subscribed as subscriber
        /// </summary>
        /// <param name="subscriber"></param>
        public static void StopListening(Object subscriber) {
            // On stop listening it is ok to ignore if no singleton is already created as we could disable an object before the singleton is instantiated
            if (getSingleton(true) == null) return;

            EventManager singleton = getSingleton();
            string key = "" + subscriber.GetInstanceID();

            if (!singleton.subscribers.ContainsKey(key)) {
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
        /// Deataches the given listener to eventName event.
        /// If a target is specified, then just targeted events will be dettached, if no target is specified, targeted events will not be dettached.
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public static void StopListening(string eventName, UnityAction<object> listener, Object target = null) {
            if (singletonInitialized()) return;
            // On stop listening it is ok to ignore if no singleton is already created as we could disable an object before the singleton is instantiated
            if (getSingleton() == null) return;

            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            ParametrizedEvent thisEvent = null;
            if (getSingleton().eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Returns true is the singleton has been initialized, at some points (like stopping listening) if the singleton does not in fact exists, we dont't
        /// care as nothing has to be done
        /// </summary>
        /// <returns>Whatever the singleton has been initialized</returns>
        private static bool singletonInitialized() {
            return eventManagerSingleton == null;
        }

        /// <summary>
        /// Deattaches all events (targeted or not) at wich listener is attached
        /// </summary>
        /// <param name="listener"></param>
        public static void StopListening(UnityAction<object> listener) {
            if (eventManagerSingleton == null) return;

            foreach (KeyValuePair<string, ParametrizedEvent> thisEvent in getSingleton().eventDictionary) {
                thisEvent.Value.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Sends the message to all attached listeners
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public static void TriggerEvent(string eventName, object message, GameObject target = null) {
            assertEventNameIsValid(eventName);

            if (getSingleton().debugEvents) {
                Debug.Log("Event triggered : " + eventName);
            }

            eventName = targetedEventName(eventName, target);

            ParametrizedEvent thisEvent = null;
            if (getSingleton().eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.Invoke(message);
            }
        }

        /// <summary>
        /// Sends a message after some amount of time, that is done on allwaysUpdate loop, so the precision is the one of that. 
        /// </summary>
        /// <param name="time">Sends the message after time seconds</param>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public static void TriggerEventAfter(float time, string eventName, object message, GameObject target = null) {
            getSingleton().delayedEvents.Add(new DelayedEvent(eventName, message, target, Time.realtimeSinceStartup + time));
        }

        /// <summary>
        /// Calculates the targeted event name for an event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        /// <returns></returns>
        private static string targetedEventName(string eventName, UnityEngine.Object target) {
            if (!target) {

                return eventName;
            }
            return eventName + "@" + target.name + "(" + target.GetInstanceID() + ")";
        }

        /// <summary>
        /// Checks if the eventName is valid or nor
        /// </summary>
        /// <param name="eventName">Event name</param>
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
}
