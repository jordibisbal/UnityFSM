using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    [Serializable]
    public class StateTransitionAlreadyDefinedException : Exception {
        public StateTransitionAlreadyDefinedException() {
        }

        public StateTransitionAlreadyDefinedException(string message) : base(message) {
        }

        public StateTransitionAlreadyDefinedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected StateTransitionAlreadyDefinedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
