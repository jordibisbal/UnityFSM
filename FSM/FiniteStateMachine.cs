using System.Collections.Generic;
using System;
using UnityEngine;

namespace JordiBisbal.FSM {
    using EventManager;
    using Guard = System.Func<bool>;

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
            this.myName = name;
            this.myOnArrive = onArrive;
            this.myOnUpdate = onUpdate;
        }
    }

    /// <summary>
    /// Fine State Machine (FSM) implementation
    /// Copyright 2017, Jordi Bisbal (jordi.bisbal@gmail.com)
    /// </summary>
    public sealed class FiniteStateMachine {

        /// <summary>
        /// Current state
        /// </summary>
        public string state {
            get {
                if (myState == null) {
                    throw new InvalidStateException("The finite state machine has no state at all (null), not initialized ?");
                }
                return myState.name;
            }
        }

        /// <summary>
        /// Debug actions and states
        /// </summary>
        public bool debug = false;

        /// <summary>
        /// Dictionary with valid states and actions to be called once we the states are reached
        /// </summary>
        private Dictionary<string, State> states = new Dictionary<string, State>();

        /// <summary>
        /// Dictionary with configured transitions
        /// </summary>
        private Dictionary<string, Dictionary<string, Guard>> transitions = new Dictionary<string, Dictionary<string, Guard>>();

        /// <summary>
        /// Dictionary with configured actions
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> actions = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Strict guarding
        /// </summary>
        private bool strictGuarding;

        /// <summary>
        /// Called when there is a state change
        /// </summary>
        private Action onChangeAction = null;

        /// <summary>
        /// Current state, the state
        /// </summary>
        private State myState = null;

        /// <summary>
        /// Transition from a state to the same state are ignored (no callbacks)
        /// </summary>
        private bool ignoreSelfTransitions;

        /// <summary>
        /// If an action in requested but the current state have no instructions for it, just ignore
        /// </summary>
        private bool ignoreUnkownAction;

        private bool subscribedToUpdate = false;

        /// <summary>
        /// Constructs the state machine.
        /// 
        /// </summary>
        /// <param name="strictGuarding">Strict guarding if true</param>
        /// <param name="ignoreSelfTransitions">Transitions from a state to the same state are ignored, no callbacks are called</param>
        public FiniteStateMachine(bool strictGuarding = true, bool ignoreSelfTransitions = true, bool ignoreUnknownActions = true) {
            this.strictGuarding = strictGuarding;
            this.ignoreSelfTransitions = ignoreSelfTransitions;
            this.ignoreUnkownAction = ignoreUnknownActions;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~FiniteStateMachine() {
            if (subscribedToUpdate) {
                EventManager.StopListening(EventManager.update, OnUpdate);
            }
        }

        /// <summary>
        /// Return true is the current state has the given name, if not initialized, returns false
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool isState(string stateName) {
            return (myState != null) && (myState.name == stateName);
        }

        /// <summary>
        /// Initializes the state, can be called just once (when the state machine has no state)
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
        public FiniteStateMachine initialize(string stateName) {
            myState = getState(stateName);
            if (debug) {
                Debug.Log("State Machine Initialized to " + stateName);
            }

            return this;
        }

        /// <summary>
        /// Asserts the given state is known, if not throws an InvalidStateException.
        /// </summary>
        /// <param name="state">State to assert exists</param>
        private State getState(string stateName) {
            if (!states.ContainsKey(stateName)) {
                throw new InvalidStateException("State " + stateName + " is not valid (unkown)");
            }

            State state;
            states.TryGetValue(stateName, out state);

            return state;
        }

        /// <summary>
        /// Asserts the given state is known, if not throws an InvalidStateException.
        /// </summary>
        /// <param name="sourceState">Source state</param>
        /// <param name="targetState">Target state</param>
        private void assertTransitionIsValid(string sourceState, string targetState) {
            if (!isTransitionValid(sourceState, targetState)) {
                throw new InvalidStateTransitionException("Transition from " + sourceState + " to " + targetState + " is not valid");
            }
        }

        /// <summary>
        /// Tries to change the state to New State.
        /// 
        /// If no state set, no transition is need, no guards apply, just apply.
        /// If no transitions are found, just set the state.
        /// </summary>
        /// <param name="newState">New state</param>
        /// <returns>Fluent interface</returns>
        private FiniteStateMachine setState(string newState) {

            if (ignoreSelfTransitions && newState == myState.name) {
                return this;
            }

            // Asserts the current transition is defined
            assertTransitionIsValid(myState.name, newState);

            // If no transitions are found, just set the state
            checkGuardsAndSetState(newState);

            return this;
        }

        /// <summary>
        /// Checks the guards for the current transition, if allowed, change to the new state.
        /// 
        /// Guard system NOT IMPLEMENTED YET !!!
        /// </summary>
        /// <param name="newState">New statem, if no state is given (null), the new State is targetState</param>
        private void checkGuardsAndSetState(string newState) {
            // TODO : Check transition guards

            myState = getState(newState);

            if (myState.onArrive != null) {
                myState.onArrive();
            }

            if (onChangeAction != null) {
                onChangeAction();
            }
        }

        /// <summary>
        /// Adds a new state
        /// </summary>
        /// <param name="name">State name</param>
        /// <param name="onArrive">Callback to be called when the state is set</param>
        /// <returns>Fluent interface</returns>
        public FiniteStateMachine addState(string name, Action onArrive = null, Action onUpdate = null) {

            if (states.ContainsKey(name)) {
                throw new StateAlreadyExistsException("State " + name + " already exists");
            }

            if ((onUpdate != null) && !subscribedToUpdate) {
                EventManager.StartListening(EventManager.update, OnUpdate);
                subscribedToUpdate = true;
            }
            states.Add(name, new State(name, onArrive, onUpdate));

            return this;
        }

        /// <summary>
        /// Returns the transition callback for the current transition
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <returns>The transition</returns>
        private Guard getTransitionGuard(string from, string to) {

            assertTransitionIsValid(from, to);
            Dictionary<string, Guard> fromCollection;
            transitions.TryGetValue(from, out fromCollection);
            Guard guard;
            fromCollection.TryGetValue(to, out guard);

            return guard;
        }

        /// <summary>
        /// Returns whenever the current transition is defined
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <returns>The transition is defined or not</returns>
        private bool isTransitionValid(string from, string to) {

            Dictionary<string, Guard> fromCollection;
            if (!transitions.TryGetValue(from, out fromCollection)) {
                return false;
            }

            return fromCollection.ContainsKey(to);
        }

        /// <summary>
        /// Adds a new transition
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <param name="guard">Guard for this transition, if null, no guard is set, if on strict transition, the transition will be denied (will throw
        /// an InvalidStateTransitionException</param>
        /// <param name="transition">Callback to be called on every update call, if null, the transition will ocurr inmediatly</param>
        /// <returns>Fluid interface</returns>
        public FiniteStateMachine addTransition(string from, string to, Guard guard = null) {

            Dictionary<string, Guard> toDictionary;

            if (!transitions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, Guard>();
                transitions.Add(from, toDictionary);
            }

            transitions.TryGetValue(from, out toDictionary);
            if (toDictionary.ContainsKey(to)) {
                throw new InvalidStateTransitionException("Transition from " + from + " to " + to + " already defined");
            }
            toDictionary.Add(to, guard);

            return this;
        }

        /// <summary>
        /// Adds a new action, if the described trasition is not defines, autodefine it with and autoaccepting guard and no update callback 
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <param name="guard">Guard for this transition, if null, no guard is set, if on strict transition, the transition will be denied (will throw
        /// an InvalidStateTransitionException</param>
        /// <param name="transition">Callback to be called on every update call, if null, the transition will ocurr inmediatly</param>
        /// <returns>Fluid interface</returns>
        public FiniteStateMachine addAction(string from, string action, string goToState) {

            Dictionary<string, string> toDictionary;

            // Autogenerates transition
            if (!isTransitionValid(from, goToState)) {
                addTransition(from, goToState, () => true);
            }

            if (!actions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, string>();
                actions.Add(from, toDictionary);
            }

            actions.TryGetValue(from, out toDictionary);
            if (toDictionary.ContainsKey(action)) {
                throw new ActionAlreadyExistsException("Action " + action + " for state " + from + " has already been defined");
            }
            toDictionary.Add(action, goToState);

            return this;
        }

        /// <summary>
        /// Executes the given action (sets state) for the current state
        /// If ignoreUnknownAction is set and the action is undefined, just returns, if not, throws an exception
        /// </summary>
        /// <param name="action">Action to perform for the current state</param>
        /// <returns></returns>
        public FiniteStateMachine doAction(string action) {
            Dictionary<string, string> toDictionary;

            string from = myState.name;

            if (!actions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, string>();
                actions.Add(from, toDictionary);
            }

            actions.TryGetValue(from, out toDictionary);
            if (!toDictionary.ContainsKey(action)) {
                if (!ignoreUnkownAction) {
                    throw new UnknownActionException("Unknown action " + action + " for state " + from);
                }
                if (debug) {
                    Debug.Log("Unknown Action " + action);
                }

                return this;
            }

            string goToState;
            toDictionary.TryGetValue(action, out goToState);
            setState(goToState);

            if (debug) {
                Debug.Log("Action " + action + " changed state to " + goToState);
            }

            return this;
        }

        /// <summary>
        /// Execute the delegate for the current (in progress) transition if any
        /// </summary>
        /// <param name="deltaTime">The deltaTime to be passed to the delegate</param>
        public void OnUpdate(object message) {
            if (myState != null && myState.onUpdate != null) {
                myState.onUpdate();
            }
        }

        /// <summary>
        /// Sets the on change callback, that Action is called on every state change
        /// </summary>    /// 
        /// <param name="onChange">The onChange callback Action</param>
        /// <returns></returns>
        public FiniteStateMachine onChange(Action onChange) {
            onChangeAction = onChange;

            return this;
        }
    }
}
