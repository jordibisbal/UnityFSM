using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    [Serializable]
    internal class UnknownStateException : Exception {
        public UnknownStateException() {
        }

        public UnknownStateException(string message) : base(message) {
        }

        public UnknownStateException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownStateException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}