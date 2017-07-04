using System.Collections.Generic;
using UnityEngine.Events;

namespace JordiBisbal.EventManager {    
    public class ParametrizedEvents {
        private List<UnityAction<object>> events = new List<UnityAction<object>>();

        public void Invoke(object message) {
            foreach (UnityAction<object> action in events) {
                action(message);
            }
        }

        public void AddListener(UnityAction<object> listener) {
            events.Add(listener);
        }

        public void RemoveListener(UnityAction<object> listener) {
            events.Remove(listener);
        }

        public int Count() {
            return events.Count;
        }
    }
}
