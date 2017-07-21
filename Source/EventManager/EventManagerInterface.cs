using UnityEngine;
using UnityEngine.Events;

namespace JordiBisbal.EventManager {

    /// <summary>
    /// Dispatch messages to subcribers, by broadcasting to the subscribers o by delivering just to one of them
    /// For targeted events "@", the name, and the instance id of the target is add to the  event name
    /// </summary>
    public interface EventManagerInterface {

        /// <summary>
        /// Should debug all events ?
        /// </summary>
        bool debugEvents { get; set; } 

        /// <summary>
        /// Subscribe a given object to an event, either for targeted or not events, the even if subscribed as subscriber so all
        /// event subscribed this way, can be freed by just calling to StopListering(subscriber)
        /// </summary>
        /// <param name="subscriber">Object the event system will use the instance Id to subscribe, this will be used when unsubscribing</param>
        /// <param name="eventName">Name of the event the listener will respond to</param>
        /// <param name="listener">Listener Action to catch the event</param>
        /// <param name="target">If </param>
        void StartListening(UnityEngine.Object subscriber, string eventName, UnityAction<object> listener, UnityEngine.Object target = null);

        /// <summary>
        /// Attaches a callback to eventName, ei. starts listening for eventName.
        /// If a target is specified, then just targeted events will be received, if no target is specified, targeted events will not be attached (received)
        /// To free the event registration, StopListering(eventName, listener) must be called, ie. the unsubcription needs booth the event name and the callback to be done
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        void StartListening(string eventName, UnityAction<object> listener, UnityEngine.Object target = null);

        /// <summary>
        /// Unsubstibe to all events that where subscribed as subscriber
        /// </summary>
        /// <param name="subscriber"></param>
        void StopListening(UnityEngine.Object subscriber);

        /// <summary>
        /// Deataches the given listener to eventName event.
        /// If a target is specified, then just targeted events will be dettached, if no target is specified, targeted events will not be dettached.
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        void StopListening(string eventName, UnityAction<object> listener, UnityEngine.Object target = null);

        /// <summary>
        /// Deattaches all events (targeted or not) at wich listener is attached
        /// </summary>
        /// <param name="listener"></param>
        void StopListening(UnityAction<object> listener);

        /// <summary>
        /// Sends the message to all attached listeners
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="message">The message to be passed</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        void TriggerEvent(string eventName, object message = null, UnityEngine.Object target = null);

        /// <summary>
        /// Counts attached listeners
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        int ListenerCount(string eventName, UnityEngine.Object target = null);

        /// <summary>
        /// Counts all attached listeners
        /// </summary>
        int ListenerCount();

        /// <summary>
        /// Sends a message after some amount of time, that is done on allwaysUpdate loop, so the precision is the one of that. 
        /// </summary>
        /// <param name="time">Sends the message after time seconds</param>
        /// <param name="eventName">The event name</param>
        /// <param name="listener">The callback to be called</param>
        /// <param name="target">If set, the listener will reveice the event only when target is specified on triggering the event</param>    
        void TriggerEventAfter(float time, string eventName, object message, UnityEngine.Object target = null);

        /// <summary>
        /// Sends a tick messatge to listners subscribed to EventManager.update
        /// </summary>
        void Update();

        /// <summary>
        /// Sends a tick messatge to listners subscribed to EventManager.allwaysUpdate and delivers delayed events
        /// </summary>
        void AllwaysUpdate();

        /// <summary>
        /// Remove delayed events matching eventName and target
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="target">Event Target</param>
        void FlushDelayedEvents(string eventName, GameObject target = null);
    }
}
