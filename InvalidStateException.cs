using System;

/// <summary>
/// An invalid state is requested
/// </summary>
public class InvalidStateException : Exception {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Message</param>
    public InvalidStateException(string message) : base(message) { }
};
