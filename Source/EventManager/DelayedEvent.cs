using UnityEngine;

namespace JordiBisbal.EventManager {
    /// <summary>
    /// Struct that holds a dalaye event, ie. an event that is trigged after a given amount of time
    /// </summary>
    struct DelayedEvent {
        /// <summary>
        /// Event name
        /// </summary>
        private string myEventName;
        public string eventName { get { return myEventName; } }

        /// <summary>
        /// Message to be passed, be aware of the live cicle of this message, if the message can be destroyed7invalidated before the event is triggered
        /// the event target should be able to deal with it
        /// </summary>
        public object myMessage;
        public object message { get { return myMessage; } }

        /// <summary>
        /// Target of the event (if any)
        /// </summary>
        private Object myTarget;
        public  Object target { get { return myTarget; } }

        /// <summary>
        /// When in realtimeSinceStartup time then event should be triggered
        /// </summary>
        private float myWhen;
        public float when { get { return myWhen; } }

        /// <summary>
        /// Struct constructor
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="message"></param>
        /// <param name="target"></param>
        /// <param name="when"></param>
        public DelayedEvent(string eventName, object message, Object target, float when) {
            this.myEventName = eventName;
            this.myMessage = message;
            this.myTarget = target;
            this.myWhen = when;
        }
    }
}
