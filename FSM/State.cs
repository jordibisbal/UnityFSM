using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// Hols an state properties
    /// </summary>
    class State {
        /// <summary>
        /// State name
        /// </summary>    
        public string name { get { return myName; } }
        private string myName;

        /// <summary>
        /// Action to we called (if any) when the machine enters this state
        /// </summary>
        public Action onArrive { get { return myOnArrive; } }
        private Action myOnArrive;

        /// <summary>
        /// Action to be called (if any) on the UpdateLoop when the machine is on this state
        /// </summary>
        public Action onUpdate { get { return myOnUpdate; } }
        private Action myOnUpdate;

        /// <summary>
        /// State constructor
        /// </summary>
        /// <param name="name">State name</param>
        /// <param name="onArrive">Action to we called (if any) when the machine enters this state</param>
        /// <param name="onUpdate">Action to be called (if any) on the UpdateLoop when the machine is on this state</param>
        public State(string name, Action onArrive, Action onUpdate) {
            myName = name;
            myOnArrive = onArrive;
            myOnUpdate = onUpdate;
        }
    }
}
