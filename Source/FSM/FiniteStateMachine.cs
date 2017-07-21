using System.Collections.Generic;
using System;
using UnityEngine;

namespace JordiBisbal.FSM {
    using EventManager;
    using JordiBisba.FSM;
    using Guard = Func<bool>;

    /// <summary>
    /// Fine State Machine (FSM, can also hold a vlue, global or for each state) implementation
    /// Copyright 2017, Jordi Bisbal (jordi.bisbal@gmail.com)
    /// </summary>
    public sealed class FiniteStateMachine {

        /// <summary>
        /// Current state
        /// </summary>
        public State state {
            get {
                if (myState == null) {
                    throw new UninitializedStateException("The finite state machine is uninitialized");
                }
                return myState;
            }
        }

        /// <summary>
        /// Global FSM value
        /// </summary>
        public Value myValue = null;

        /// <summary>
        /// Global FSM value
        /// </summary>
        public Value value {
            get {
                return myValue;
            }
            set {
                myValue = value;
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
        private ValuedAction onChangeAction = null;

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
        private bool ignoreUnkownActions = true;

        /// <summary>
        /// The FSM is subscribed to Update loop (to call callbacks on every update)
        /// </summary>
        private bool subscribedToUpdate = false;

        /// <summary>
        /// Event manager (to subscribe to updates/allwaysUpdates)
        /// </summary>
        private EventManager eventManager;

        /// <summary>
        /// Constructs the state machine.
        /// 
        /// </summary>
        /// <param name="strictGuarding">Strict guarding if true</param>
        /// <param name="ignoreSelfTransitions">Transitions from a state to the same state are ignored, no callbacks are called</param>
        public FiniteStateMachine(EventManager eventManager = null, bool strictGuarding = true, bool ignoreSelfTransitions = true, bool ignoreUnknownActions = true) {
            this.eventManager = eventManager;
            this.strictGuarding = strictGuarding;
            this.ignoreSelfTransitions = ignoreSelfTransitions;
            this.ignoreUnkownActions = ignoreUnknownActions;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~FiniteStateMachine() {
            if (subscribedToUpdate) {
                eventManager.StopListening(EventManager.update, OnUpdate);
            }
        }

        /// <summary>
        /// Return true is the current state has the given name, if not initialized, returns false
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool IsState(string stateName) {
            AssertIsInitialized();
            return (myState.name == stateName);
        }


        /// <summary>
        /// Throws an exception is the FinateStateMachine has not been initialized
        /// </summary>
        private void AssertIsInitialized() {
            if (myState == null) {
                throw new UninitializedFiniteStateMachineException("The FiniteStateMachine has not been initialized yet");
            }
        }

        /// <summary>
        /// Initializes the state, can be called just once (when the state machine has no state)
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
        public FiniteStateMachine Initialize(string stateName, Value value = null) {
            if (myState != null) {
                throw new AlreadyInitializedException("The FiniteStateMachine has already been initialized");
            }
            myState = GetState(stateName);
            if (debug) {
                Debug.Log("State Machine Initialized to " + stateName);
            }

            this.value = value;

            return this;
        }

        /// <summary>
        /// Returns the given state, if the given state is not known, throws an UnknownStateException.
        /// </summary>
        /// <param name="state">State to assert exists</param>
        private State GetState(string stateName) {
            AssertStateExists(stateName);

            return states[stateName];
        }

        private void AssertStateExists(string stateName) {
            if (!states.ContainsKey(stateName)) {
                throw new UnknownStateException("State \"" + stateName + "\" is unknown");
            }
        }

        /// <summary>
        /// Sets the given state value, if the given state is not known, throws an UnknownStateException.
        /// </summary>
        /// <param name="state">State to assert exists</param>
        public State setStateValue(string stateName, Value value) {
            State state = GetState(stateName);
            states[stateName] = new State(state.name, state.onArrive, state.onUpdate, value);

            // Refresh current state
            if (myState.name == stateName) {
                myState = states[stateName];
            }

            return state;
        }

        /// <summary>
        /// Sets the given state value, if the given state is not known, throws an UnknownStateException.
        /// </summary>
        /// <param name="state">State to assert exists</param>
        public State setStateValue(State state, Value value) {
            return setStateValue(state.name, value);
        }

        /// <summary>
        /// Sets the current state value
        /// </summary>
        public State setCurrentStateValue(Value value) {
            return setStateValue(state.name, value);
        }

        /// <summary>
        /// Tries to change the state to New State.
        /// 
        /// If no state set, no transition is need, no guards apply, just apply.
        /// If no transitions are found, just set the state.
        /// </summary>
        /// <param name="newState">New state</param>
        /// <returns>Fluent interface</returns>
        private FiniteStateMachine setState(string newState, Value newValue) {

            if (ignoreSelfTransitions && newState == myState.name) {
                return this;
            }
            
            // If no transitions are found, just set the state
            checkGuardsAndSetState(newState, newValue);

            return this;
        }

        /// <summary>
        /// Checks the guards for the current transition, if allowed, change to the new state.
        /// 
        /// Guard system NOT IMPLEMENTED YET !!!
        /// </summary>
        /// <param name="newState">New statem, if no state is given (null), the new State is targetState</param>
        private void checkGuardsAndSetState(string newState, Value newValue) {
            // TODO : Check transition guards

            setStateValue(newState, newValue);

            myState = GetState(newState);

            if (myState.onArrive != null) {
                myState.onArrive(myState);
            }

            if (onChangeAction != null) {
                onChangeAction(myState);
            }
        }

        /// <summary>
        /// Adds a new state
        /// </summary>
        /// <param name="name">State name</param>
        /// <param name="onArrive">Callback to be called when the state is set</param>
        /// <param name="onUpdate">Callback to be called while the state is active (on update loop)</param>
        /// <param name="value">Initial value of the state (if any)</param>
        /// <returns></returns>
        public FiniteStateMachine AddState(string name, ValuedAction onArrive = null, ValuedAction onUpdate = null, Value value = null) {

            if (states.ContainsKey(name)) {
                throw new StateAlreadyExistsException("State \"" + name + "\" already exists");
            }

            SubscribeToUpdate(onUpdate);
            states.Add(name, new State(name, onArrive, onUpdate, value));

            return this;
        }

        /// <summary>
        /// Subscribe to update events
        /// </summary>
        /// <param name="onUpdate"></param>
        private void SubscribeToUpdate(ValuedAction onUpdate) {
            if ((onUpdate != null) && !subscribedToUpdate) {
                if (eventManager == null) {
                    throw new ThereIsNoEVentManagerException("No event manager on this FiniteStateMachine to take care of update events");
                }

                eventManager.StartListening(EventManager.update, OnUpdate);
                subscribedToUpdate = true;
            }
        }

        /// <summary>
        /// Adds a new state
        /// </summary>
        /// <param name="name">State name</param>
        /// <param name="onArrive">Callback to be called when the state is set</param>
        /// <param name="onUpdate">Callback to be called while the state is active (on update loop)</param>
        /// <param name="value">Initial value of the state (if any)</param>
        /// <returns></returns>
        public FiniteStateMachine AddState(string name, Value value) {
            return AddState(name, null, null, value);
        }


        /// <summary>
        /// Returns the transition callback for the current transition
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <returns>The transition</returns>
        private Guard getTransitionGuard(string from, string to) {

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
        /// <returns>Fluid interface</returns>
        public FiniteStateMachine AddTransition(string from, string to, Guard guard = null) {

            Dictionary<string, Guard> toDictionary;

            if (!transitions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, Guard>();
                transitions.Add(from, toDictionary);
            }

            transitions.TryGetValue(from, out toDictionary);
            if (toDictionary.ContainsKey(to)) {
                throw new StateTransitionAlreadyDefinedException("Transition from \"" + from + "\" to \"" + to + "\" already defined");
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
        public FiniteStateMachine AddAction(string from, string action, string goToState) {

            Dictionary<string, string> toDictionary;

            AssertStateExists(from);
            AssertStateExists(goToState);

            // Autogenerates transition
            if (!isTransitionValid(from, goToState)) {
                AddTransition(from, goToState, () => true);
            }

            if (!actions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, string>();
                actions.Add(from, toDictionary);
            }

            actions.TryGetValue(from, out toDictionary);
            if (toDictionary.ContainsKey(action)) {
                throw new ActionAlreadyExistsException("Action \"" + action + "\" for \"" + from + "\" state has already been defined");
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
        public FiniteStateMachine DoAction(string action, Value value = null) {
            Dictionary<string, string> toDictionary;

            AssertIsInitialized();

            string from = myState.name;

            if (!actions.ContainsKey(from)) {
                toDictionary = new Dictionary<string, string>();
                actions.Add(from, toDictionary);
            }

            actions.TryGetValue(from, out toDictionary);
            if (toDictionary == null) {
                throw new UnknownStateException("Unknow state \"" + from + "\"");
            }
            if (!toDictionary.ContainsKey(action)) {
                if (!ignoreUnkownActions) {
                    throw new UnknownActionException("Unknown action \"" + action + "\" for state \"" + from + "\"");
                }
                if (debug) {
                    Debug.Log("Unknown Action " + action);
                }

                return this;
            }

            string goToState;
            toDictionary.TryGetValue(action, out goToState);
            setState(goToState, value);

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
                myState.onUpdate(myState);
            }
        }

        /// <summary>
        /// Sets the on change callback, that Action is called on every state change
        /// </summary>     
        /// <param name="onChange">The onChange callback Action</param>
        /// <returns></returns>
        public FiniteStateMachine SetOnChange(ValuedAction onChange) {
            onChangeAction = onChange;

            return this;
        }
    }
}
