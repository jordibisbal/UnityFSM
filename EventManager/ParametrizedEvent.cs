using UnityEngine.Events;

namespace JordiBisbal.EventManager {
    /// <summary>
    /// Just derives from UnityEvent
    /// </summary>
    [System.Serializable]
    public class ParametrizedEvent : UnityEvent<object> { }
}
