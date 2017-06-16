using UnityEngine;
using UnityEngine.Events;

namespace JordiBisbal.EventManager {
    /// <summary>
    /// Stores the needed info, so event could be unsubscribed by just the subscriber, not needed to be unsubscribed one by one, with eventName and callback.
    /// </summary>
    struct OwnedListener {
        /// <summary>
        /// The object the event is assigned to (instance id will be used as index)
        /// </summary>
        public Object owner;
        /// <summary>
        /// The callback
        /// </summary>
        public UnityAction<object> listener;
        /// <summary>
        /// The event name
        /// </summary>
        public string eventName;
        /// <summary>
        /// The target (for targeted events)
        /// </summary>
        public Object target;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">The object the event is assigned to (instance id will be used as index)</param>
        /// <param name="listener">The callback that will be executed when the event is triggered</param>
        /// <param name="eventName">The event name</param>
        /// <param name="target">The target (for targeted events)</param>
        public OwnedListener(Object owner, UnityAction<object> listener, string eventName, Object target) {
            this.owner = owner;
            this.listener = listener;
            this.eventName = eventName;
            this.target = target;
        }
    }
}
