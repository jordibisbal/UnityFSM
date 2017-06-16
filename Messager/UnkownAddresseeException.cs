using System;

namespace JordiBisbal.Messager {
    /// <summary>
    /// There is no receiver for this message
    /// </summary>
    public class UnkownAddresseeException : Exception {
        public UnkownAddresseeException(string message) : base(message) {}
    }
}
