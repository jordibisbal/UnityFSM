using System;
namespace JordiBisbal.FSM {
    /// <summary>
    /// The action has already been defined for that state
    /// </summary>
    public class ActionAlreadyExistsException : FiniteStateMachineException {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public ActionAlreadyExistsException(string message) : base(message) { }
    }
}
