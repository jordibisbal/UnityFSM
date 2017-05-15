using System;

/// <summary>
/// An invalid state transition is requested
/// </summary>
public class InvalidStateTransitionException : Exception {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Message</param>
    public InvalidStateTransitionException(string message) : base(message) { }
};
