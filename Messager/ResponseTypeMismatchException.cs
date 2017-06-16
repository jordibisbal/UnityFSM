using System;

namespace JordiBisbal.Messager {
    /// <summary>
    /// The message response is not of the expected type, eg. a Response intead of a BooleanResponse
    /// </summary>
    public class responseTypeMisMatch : Exception {
        public responseTypeMisMatch(string message) : base(message) { }
    }
}
