using System;
using System.Runtime.Serialization;

namespace JordiBisbal.EventManager {
    [Serializable]
    internal class MissConfiguredException : Exception {
        public MissConfiguredException() {
        }

        public MissConfiguredException(string message) : base(message) {
        }

        public MissConfiguredException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MissConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}