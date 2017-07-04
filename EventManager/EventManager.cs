using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System;

namespace JordiBisbal.EventManager {
    /// <summary>
    /// Dispatch messages to subcribers, by broadcasting to the subscribers o by delivering just to one of them
    /// For targeted events "@", the name, and the instance id of the target is add to the  event name
    /// </summary>
    public class EventManager {

        /// <summary>
        /// Messages that EventManager sends by itself
        /// </summary>
        public const string update        = "EventManager.update";
        public const string allwaysUpdate = "EventManager.allwaysUpdate";
        public const string log           = "EventManager.log";

        /// <summary>
        /// Should debug all events ?
        /// </summary>
        public bool debugEvents = false;

        /// <summary>
        /// Stored the subcribers lists
        /// </summary>
        Dictionary<string, ParametrizedEvents> eventDictionary = new Dictionary<string, ParametrizedEvents>();

        /// <summary>
        /// Subscribers list
        /// </summary>
        Dictionary<string, ArrayList> subscribers = new Dictionary<string, ArrayList>();

        /// <summary>
        /// Delayed events
        /// </summary>
        ArrayList delayedEvents = new ArrayList();

        /// <summary>
        /// Return the current time (based on game start)
        /// </summary>
        Func<float> timeProviderDelegate;

        public EventManager(Func<float> timeProviderDelegate = null) {
            this.timeProviderDelegate = timeProviderDelegate;
        }

        /// <summary>
        /// Subscribe a given object to an event, either for targeted or not events, the even if subscribed as subscriber so all
        /// event subscribed this way, can be freed by just calling to StopListering(subscriber)
        /// </summary>
        /// <param name="subscriber">Object the event system will use the instance Id to subscribe, this will be used when unsubscribing</param>
        /// <param name="eventName">Name of the event the listener will respond to</param>
        /// <param name="listener">Listener Action to catch the event</param>
        /// <param name="target">If </param>
        public void StartListening(UnityEngine.Object subscriber, string eventName, UnityAction<object> listener, UnityEngine.Object target = null) {            
            string key = "" + subscriber.GetInstanceID();
            
            if (! subscribers.ContainsKey(key)) {
                subscribers.Add(key, new ArrayList());
            }
            ArrayList namedEvents;
            subscribers.TryGetValue(key, out namedEvents);
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
        public void StartListening(string eventName, UnityAction<object> listener, UnityEngine.Object target = null) {
            ParametrizedEvents thisEvent = null;
            
            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.AddListener(listener);
            }
            else {
                thisEvent = new ParametrizedEvents();
                thisEvent.AddListener(listener);
                eventDictionary.Add(eventName, thisEvent);
            }

            DebugEvent("Event listener added : " + eventName);
        }

        /// <summary>
        /// Logs the event
        /// </summary>
        /// <param name="eventName">Message to log</param>
        private void DebugEvent(string message) {
            if (debugEvents) {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// Unsubstibe to all events that where subscribed as subscriber
        /// </summary>
        /// <param name="subscriber"></param>
        public void StopListening(UnityEngine.Object subscriber) {
            // On stop listening it is ok to ignore if no singleton is already created as we could disable an object before the singleton is instantiated
            string key = "" + subscriber.GetInstanceID();

            if (!subscribers.ContainsKey(key)) {
                return;
            }

            ArrayList ownedEvents;
            subscribers.TryGetValue(key, out ownedEvents);
            foreach (OwnedListener ownedListener in ownedEvents) {
                StopListening(ownedListener.eventName, ownedListener.listener, ownedListener.target);
            }
            subscribers.Remove(key);
        }
        /// <summary>
        /// Deataches the given listener to eventName event.
        /// If a target is specified, then just targeted events will be dettached, if no target is specified, targeted events will not be dettached.
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public void StopListening(string eventName, UnityAction<object> listener, UnityEngine.Object target = null) {
            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            ParametrizedEvents thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Deattaches all events (targeted or not) at wich listener is attached
        /// </summary>
        /// <param name="listener"></param>
        public void StopListening(UnityAction<object> listener) {
            foreach (KeyValuePair<string, ParametrizedEvents> thisEvent in eventDictionary) {
                thisEvent.Value.RemoveListener(listener);
            }
        }

        /// <summary>
        /// Sends the message to all attached listeners
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="message">The message to be passed</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public void TriggerEvent(string eventName, object message = null, UnityEngine.Object target = null) {
            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            DebugEvent("Event triggered : " + eventName);

            ParametrizedEvents thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                thisEvent.Invoke(message);
            }
        }

        /// <summary>
        /// Counts attached listeners
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public int ListenerCount(string eventName, UnityEngine.Object target = null) {
            assertEventNameIsValid(eventName);

            eventName = targetedEventName(eventName, target);

            DebugEvent("Event triggered : " + eventName);

            ParametrizedEvents thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                return thisEvent.Count();
            }

            return 0;
        }

        /// <summary>
        /// Counts all attached listeners
        /// </summary>
        public int ListenerCount() {
            int total = 0;

            foreach (ParametrizedEvents paremitrizedEvents in eventDictionary.Values) {
                total = total + paremitrizedEvents.Count();
            }

            return total;
        }

        /// <summary>
        /// Sends a message after some amount of time, that is done on allwaysUpdate loop, so the precision is the one of that. 
        /// </summary>
        /// <param name="time">Sends the message after time seconds</param>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        public void TriggerEventAfter(float time, string eventName, object message, UnityEngine.Object target = null) {
            delayedEvents.Add(new DelayedEvent(eventName, message, target, GetTime() + time));
        }

        /// <summary>
        /// Calculates the targeted event name for an event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        /// <returns></returns>
        private string targetedEventName(string eventName, UnityEngine.Object target) {
            if (target == null) {

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
            if (!(new Regex(@"^[\w./:]+$").IsMatch(eventName))) {
                throw new InvalidEventNameException(eventName);
            };
        }

        /// <summary>
        /// Sends a tick messatge to listners subscribed to EventManager.update
        /// </summary>
        public void Update() {
            TriggerEvent(EventManager.update, null);
        }

        /// <summary>
        /// Sends a tick messatge to listners subscribed to EventManager.allwaysUpdate and delivers delayed events
        /// </summary>
        public void AllwaysUpdate() {
            TriggerEvent(EventManager.allwaysUpdate, null);
            float now = GetTime();

            for (int i = delayedEvents.Count - 1; i >= 0; i--) {
                DelayedEvent theEvent = (DelayedEvent)delayedEvents[i];
                if (theEvent.when <= now) {
                    TriggerEvent(theEvent.eventName, theEvent.message, theEvent.target);
                    delayedEvents.Remove(theEvent);
                }
            }
        }

        /// <summary>
        /// Return the current time
        /// </summary>
        /// <returns></returns>
        private float GetTime() {
            if (timeProviderDelegate == null) {
                throw new MissConfiguredException("No time provider has been setup for this eventManager");
            }
            float now = timeProviderDelegate();
            return now;
        }

        /// <summary>
        /// Remove delayed events matching eventName and target
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="target">Event Target</param>
        public void FlushDelayedEvents(string eventName, GameObject target = null) {
            for (int i = delayedEvents.Count - 1; i > 0; i--) {
                DelayedEvent theEvent = (DelayedEvent)delayedEvents[i];
                if ((theEvent.eventName == eventName) && (theEvent.target == target)) {
                    delayedEvents.Remove(theEvent);
                }
            }
        }
    }
}
