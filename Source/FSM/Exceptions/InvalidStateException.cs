using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// An invalid state is requested
    /// </summary>
    public class InvalidStateException : FiniteStateMachineException {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public InvalidStateException(string message) : base(message) { }
    }
}
