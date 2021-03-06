using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// An invalid state is requested
    /// </summary>
    public class StateAlreadyExistsException : FiniteStateMachineException {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public StateAlreadyExistsException(string message) : base(message) { }
    }
}
