using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    [Serializable]
    public class UninitializedFiniteStateMachineException : Exception {
        public UninitializedFiniteStateMachineException() {
        }

        public UninitializedFiniteStateMachineException(string message) : base(message) {
        }

        public UninitializedFiniteStateMachineException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UninitializedFiniteStateMachineException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
