using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// An action unknown for the curremt state has been requested
    /// </summary>
    public class UnknownActionException : FiniteStateMachineException {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public UnknownActionException(string message) : base(message) { }
    }
}
