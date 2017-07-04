using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    /// <summary>
    /// Thrown when the FSM need an event manager, but none given
    /// </summary>
    internal class ThereIsNoEVentManagerException : Exception {
        public ThereIsNoEVentManagerException() {
        }

        public ThereIsNoEVentManagerException(string message) : base(message) {
        }

        public ThereIsNoEVentManagerException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ThereIsNoEVentManagerException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
