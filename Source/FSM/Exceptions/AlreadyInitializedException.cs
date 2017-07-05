using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    [Serializable]
    public class AlreadyInitializedException : FiniteStateMachineException {
        public AlreadyInitializedException() {
        }

        public AlreadyInitializedException(string message) : base(message) {
        }

        public AlreadyInitializedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected AlreadyInitializedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
