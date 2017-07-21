using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// An invalid state is requested
    /// </summary>
    public class UninitializedStateException : FiniteStateMachineException {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public UninitializedStateException(string message) : base(message) { }
    }
}
