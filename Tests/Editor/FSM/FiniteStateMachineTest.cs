using NUnit.Framework;
using JordiBisbal.FSM;
using JordiBisbal.EventManager;

public class FiniteStateMachineTest {
    [Test]
    public void testSingleStateFSM() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state");
        Assert.AreEqual("state", stateMachine.state.name);
    }

    [Test]
    public void testIsStateFSM() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state");
        Assert.True(stateMachine.IsState("state"));
    }

    [Test]
    public void testIsNotStateFSM() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state");
        Assert.False(stateMachine.IsState("notState"));
    }

    [Test]
    public void testSingleStateRepeatStateFSM() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        StateAlreadyExistsException exception = Assert.Throws<StateAlreadyExistsException>(delegate () {
            stateMachine.AddState("state");
            stateMachine.AddState("state");
        });
        Assert.AreEqual("State \"state\" already exists", exception.Message);
    }

    [Test]
    public void testUnititalizedFSM() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        UninitializedFiniteStateMachineException exception = Assert.Throws<UninitializedFiniteStateMachineException>(delegate () {
            stateMachine.AddState("state");
            stateMachine.IsState("state");
        });
        Assert.AreEqual("The FiniteStateMachine has not been initialized yet", exception.Message);
    }

    [Test]
    public void testInitialization() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state", new IntegerValue(33));

        Assert.AreEqual("state", stateMachine.state.name);
        Assert.AreEqual(33, ((IntegerValue) stateMachine.value).AsInt());
    }

    [Test]
    public void testSetValue() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state", new IntegerValue(33));
        stateMachine.value = new IntegerValue(66);

        Assert.AreEqual("state", stateMachine.state.name);
        Assert.AreEqual(66, ((IntegerValue)stateMachine.value).AsInt());
    }

    [Test]
    public void testSetStateValue() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state", new IntegerValue(33));
        stateMachine.setStateValue(stateMachine.state, new IntegerValue(66));

        Assert.AreEqual("state", stateMachine.state.name);
        Assert.AreEqual(66, ((IntegerValue)stateMachine.state.value).AsInt());
    }

    [Test]
    public void testSetUnkownStateValue() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        UnknownStateException exception = Assert.Throws<UnknownStateException>(delegate () {
            stateMachine.AddState("state");
            stateMachine.Initialize("state", new IntegerValue(33));
            stateMachine.setStateValue("unknown state", new IntegerValue(66));
        });

        Assert.AreEqual("State \"unknown state\" is unknown", exception.Message);
    }


    [Test]
    public void testSetCurrentStateValue() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();

        stateMachine.AddState("state");
        stateMachine.Initialize("state", new IntegerValue(33));
        stateMachine.setCurrentStateValue(new IntegerValue(66));

        Assert.AreEqual("state", stateMachine.state.name);
        Assert.AreEqual(66, ((IntegerValue)stateMachine.state.value).AsInt());
    }

    [Test]
    public void testSingleStateWithUpdateButNoEventManager() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        ValuedAction onUpdate = (State state) => { };
        ThereIsNoEVentManagerException exception = Assert.Throws<ThereIsNoEVentManagerException>(delegate () {
            stateMachine.AddState("state", null, onUpdate);

            stateMachine.Initialize("state");
        });
        Assert.AreEqual("No event manager on this FiniteStateMachine to take care of update events", exception.Message);
    }

    [Test]
    public void testInitializeToUnkownState() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        UnknownStateException exception = Assert.Throws<UnknownStateException>(delegate () {
            stateMachine.AddState("state");

            stateMachine.Initialize("state33");
        });
        Assert.AreEqual("State \"state33\" is unknown", exception.Message);
    }

    [Test]
    public void testInitializeTwice() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        AlreadyInitializedException exception = Assert.Throws<AlreadyInitializedException>(delegate () {
            stateMachine.AddState("state");

            stateMachine.Initialize("state");
            stateMachine.Initialize("state");
        });
        Assert.AreEqual("The FiniteStateMachine has already been initialized", exception.Message);
    }

    [Test]
    public void testSingleStateWithUpdate() {
        EventManager eventManager = new EventManager();
        FiniteStateMachine stateMachine = new FiniteStateMachine(eventManager);
        string value = "no";
        ValuedAction onUpdate = (State state) => { value = "yes";  };
        stateMachine.AddState("state", null, onUpdate);

        stateMachine.Initialize("state");

        Assert.AreEqual("no", value);
        eventManager.Update();
        Assert.AreEqual("yes", value);
    }

    [Test]
    public void testChangeState() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        string value = "no";
        ValuedAction onArrive = (State state) => { value = "yes"; };
        stateMachine
            .AddState("state")
            .AddState("state2", onArrive)

            .AddAction("state", "doit", "state2")
            .Initialize("state")
        ;

        Assert.AreEqual("state", stateMachine.state.name);
        stateMachine.DoAction("doit");
        Assert.AreEqual("state2", stateMachine.state.name);
    }

    [Test]
    public void testChangeOnArrive() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        string value = "no";
        ValuedAction onArrive = (State state) => { value = "yes"; };
        stateMachine
            .AddState("state")
            .AddState("state2", onArrive)

            .AddAction("state", "doit", "state2")

            .Initialize("state")
        ;

        Assert.AreEqual("no", value);
        stateMachine.DoAction("doit");
        Assert.AreEqual("yes", value);
    }

    [Test]
    public void testDefineTransition() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        stateMachine
            .AddState("state")
            .AddState("state2")

            .AddTransition("state", "state2")
            .AddAction("state", "doit", "state2")                

            .Initialize("state")

            .DoAction("doit")
        ;

        Assert.AreEqual("state2", stateMachine.state.name);
    }

    [Test]
    public void testDefineTwiceTransition() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        StateTransitionAlreadyDefinedException exception = Assert.Throws<StateTransitionAlreadyDefinedException>(delegate () {
            stateMachine
                .AddState("state")
                .AddState("state2")

                .AddAction("state", "doit", "state2")
                .AddTransition("state", "state2")
            ;
        });

        Assert.AreEqual("Transition from \"state\" to \"state2\" already defined", exception.Message);
    }

    [Test]
    public void testDefineTwiceAction() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        ActionAlreadyExistsException exception = Assert.Throws<ActionAlreadyExistsException>(delegate () {
            stateMachine
                .AddState("from state")
                .AddState("state2")

                .AddAction("from state", "doit", "state2")
                .AddAction("from state", "doit", "state2")
            ;
        });

        Assert.AreEqual("Action \"doit\" for \"from state\" state has already been defined", exception.Message);
    }

    [Test]
    public void testOnChange() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        string value = "no";
        ValuedAction onChange = (State state) => { value = "yes"; };
        stateMachine
            .AddState("state")
            .AddState("state2")

            .AddAction("state", "doit", "state2")

            .SetOnChange(onChange)
            .Initialize("state")
        ;

        Assert.AreEqual("no", value);
        stateMachine.DoAction("doit");
        Assert.AreEqual("yes", value);
    }

    [Test]
    public void testUnknownAction() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        
        stateMachine
            .AddState("state")
            .Initialize("state")
            .DoAction("doit");
        ;
        
        Assert.AreEqual("state", stateMachine.state.name);
    }

    [Test]
    public void testUnknownActionNotIgnoring() {
        FiniteStateMachine stateMachine = new FiniteStateMachine(null, true, true, false);
        UnknownActionException exception = Assert.Throws<UnknownActionException>(delegate () {
            stateMachine
                .AddState("state")
                .Initialize("state")
                .DoAction("doit");
            ;
        });

        Assert.AreEqual("Unknown action \"doit\" for state \"state\"", exception.Message);
    }

    [Test]
    public void testUnitializedOnAction() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        UninitializedFiniteStateMachineException exception = Assert.Throws<UninitializedFiniteStateMachineException>(delegate () {
            stateMachine

                .DoAction("doit");
            ;
        });

        Assert.AreEqual("The FiniteStateMachine has not been initialized yet", exception.Message);
    }

    [Test]
    public void testUnknownState() {
        FiniteStateMachine stateMachine = new FiniteStateMachine();
        UnknownStateException exception = Assert.Throws<UnknownStateException>(delegate () {
            stateMachine
                .AddState("state")

                .AddAction("state", "doit", "state2")
            ;
        });

        Assert.AreEqual("State \"state2\" is unknown", exception.Message);
    }

    [Test]
    public void testSelfTransition() {
        FiniteStateMachine stateMachine = new FiniteStateMachine(null, false, false);
        string value = "no";
        ValuedAction onChange = (State state) => { value = "yes"; };
        stateMachine
            .AddState("state")

            .AddAction("state", "doit", "state")

            .SetOnChange(onChange)
            .Initialize("state")
        ;

        Assert.AreEqual("no", value);
        stateMachine.DoAction("doit");
        Assert.AreEqual("yes", value);
    }

    [Test]
    public void testSelfTransitionIgnoring() {
        FiniteStateMachine stateMachine = new FiniteStateMachine(null, false, true);
        string value = "no";
        ValuedAction onChange = (State state) => { value = "yes"; };
        stateMachine
            .AddState("state")

            .AddAction("state", "doit", "state")

            .SetOnChange(onChange)
            .Initialize("state")
        ;

        Assert.AreEqual("no", value);
        stateMachine.DoAction("doit");
        Assert.AreEqual("no", value);
    }
}
