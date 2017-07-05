using System;
using System.Runtime.Serialization;

namespace JordiBisbal.FSM {
    [Serializable]
    public class FiniteStateMachineException : Exception {
        public FiniteStateMachineException() {
        }

        public FiniteStateMachineException(string message) : base(message) {
        }

        public FiniteStateMachineException(string message, Exception innerException) : base(message, innerException) {
        }

        protected FiniteStateMachineException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
