using JordiBisba.FSM;
using System;

namespace JordiBisbal.FSM {
    /// <summary>
    /// Hols an state properties
    /// </summary>
    public class State {
        /// <summary>
        /// State name
        /// </summary>    
        public string name { get { return myName; } }
        private string myName;

        /// <summary>
        /// Action to we called (if any) when the machine enters this state
        /// </summary>
        public ValuedAction onArrive { get { return myOnArrive; } }
        private ValuedAction myOnArrive;

        /// <summary>
        /// Action to be called (if any) on the UpdateLoop when the machine is on this state
        /// </summary>
        public ValuedAction onUpdate { get { return myOnUpdate; } }
        private ValuedAction myOnUpdate;

        /// <summary>
        /// A value associated with the state
        /// </summary>
        public  Value value { get { return myValue; } }
        private Value myValue = null;

        /// <summary>
        /// State constructor
        /// </summary>
        /// <param name="name">State name</param>
        /// <param name="onArrive">Action to we called (if any) when the machine enters this state</param>
        /// <param name="onUpdate">Action to be called (if any) on the UpdateLoop when the machine is on this state</param>
        public State(string name, ValuedAction onArrive, ValuedAction onUpdate, Value value = null) {
            myName     = name;
            myOnArrive = onArrive;
            myOnUpdate = onUpdate;
            myValue    = value;
        } 
    }
}
