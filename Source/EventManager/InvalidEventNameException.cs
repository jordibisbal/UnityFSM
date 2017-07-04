using System;
using System.Runtime.Serialization;

namespace JordiBisbal.EventManager {
    /// <summary>
    /// An exception thrown when an invalid name for an event is used
    /// </summary>
    internal class InvalidEventNameException : Exception {
        public InvalidEventNameException() {}
        public InvalidEventNameException(string message) : base(message) {}
        public InvalidEventNameException(string message, Exception innerException) : base(message, innerException) {}
        protected InvalidEventNameException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}
