using NUnit.Framework;
using JordiBisbal.EventManager;
using UnityEngine.Events;
using UnityEngine;

public class EventManagerTest {
    [Test]
    public void testInvalidEventNameException() {
        InvalidEventNameException exception = Assert.Throws<InvalidEventNameException>(delegate () {
            EventManager eventManager = new EventManager();
            UnityAction<object> listener = message => { };
            eventManager.StartListening("event ", listener);            
        });
        Assert.AreEqual("Invalid event name \"event \"", exception.Message);
    }

    [Test]
    public void testMisconfiguredException() {
        MissConfiguredException exception = Assert.Throws<MissConfiguredException>(delegate () {
            EventManager eventManager = new EventManager();
            eventManager.AllwaysUpdate();
        });
        Assert.AreEqual("No time delegate has been provided for this eventManager", exception.Message);
    }

    [Test]
    public void testListening() {
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => {};
        eventManager.StartListening("event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testStopListening() {
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => { };
        eventManager.StartListening("event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
        eventManager.StopListening("event", listener);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testStopListeningBySubcriber() {
        EventManager eventManager = new EventManager();
        Object subscriber = GetTarget();
        UnityAction<object> listener = message => { };
        eventManager.StartListening(subscriber, "event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
        eventManager.StopListening(subscriber);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testStopListeningBySubcriberMultiple() {
        EventManager eventManager = new EventManager();
        Object subscriber = GetTarget();
        UnityAction<object> listener = message => { };
        UnityAction<object> listener2 = message => { };
        eventManager.StartListening(subscriber, "event", listener);
        eventManager.StartListening(subscriber, "event", listener2);
        Assert.AreEqual(2, eventManager.ListenerCount("event"));
        eventManager.StopListening(subscriber);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testStopListeningBySubcriberMixedTarget() {
        EventManager eventManager = new EventManager();
        Object subscriber = GetTarget();
        Object target = GetTarget();
        UnityAction<object> listener = message => { };
        UnityAction<object> listener2 = message => { };
        eventManager.StartListening(subscriber, "event", listener, target);
        eventManager.StartListening(subscriber, "event", listener2);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
        Assert.AreEqual(1, eventManager.ListenerCount("event", target));
        Assert.AreEqual(2, eventManager.ListenerCount());
        eventManager.StopListening(subscriber);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
        Assert.AreEqual(0, eventManager.ListenerCount("event", target));
        Assert.AreEqual(0, eventManager.ListenerCount());
    }

    [Test]
    public void testCount() {
        EventManager eventManager = new EventManager();
        Object subscriber = GetTarget();
        Object target = GetTarget();
        UnityAction<object> listener = message => { };
        UnityAction<object> listener2 = message => { };
        UnityAction<object> listener3 = message => { };
        eventManager.StartListening(subscriber, "event", listener, target);
        eventManager.StartListening(subscriber, "event", listener2);
        eventManager.StartListening(subscriber, "event", listener3);
        Assert.AreEqual(2, eventManager.ListenerCount("event"));
        Assert.AreEqual(1, eventManager.ListenerCount("event", target));
        Assert.AreEqual(3, eventManager.ListenerCount());
        eventManager.StopListening(subscriber);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
        Assert.AreEqual(0, eventManager.ListenerCount("event", target));
        Assert.AreEqual(0, eventManager.ListenerCount());
    }

    [Test]
    public void testListeningTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => { };
        eventManager.StartListening("event", listener, target);
        Assert.AreEqual(1, eventManager.ListenerCount("event", target));
    }

    [Test]
    public void testStopListeningTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => { };
        eventManager.StartListening("event", listener, target);
        Assert.AreEqual(1, eventManager.ListenerCount("event", target));
        eventManager.StopListening("event", listener, target);
        Assert.AreEqual(0, eventManager.ListenerCount("event", target));
    }


    [Test]
    public void testListeningButNotTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => { };
        eventManager.StartListening("event", listener);
        Assert.AreEqual(0, eventManager.ListenerCount("event", target));
    }

    private static Object GetTarget() {
        Object target = new GameObject("target");
        return target;
    }

    [Test]
    public void testListeningTargetedButTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        UnityAction<object> listener = message => { };
        eventManager.StartListening("event", listener, target);
        Assert.AreEqual(0, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testListeningAndReceiving() {
        EventManager eventManager = new EventManager();
        bool called = false;
        UnityAction<object> listener = message => { called = true; };
        eventManager.StartListening("event", listener);
        eventManager.TriggerEvent("event", null);
        Assert.True(called);
    }

    [Test]
    public void testListeningWithSubscriberAndReceiving() {
        Object subscriber = GetTarget();
        EventManager eventManager = new EventManager();
        bool called = false;
        UnityAction<object> listener = message => { called = true; };
        eventManager.StartListening(subscriber, "event", listener);
        eventManager.TriggerEvent("event", null);
        Assert.True(called);
    }

    [Test]
    public void testListeningAndReceivingTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        bool called = false;
        UnityAction<object> listener = message => { called = true; };
        eventManager.StartListening("event", listener, target);
        eventManager.TriggerEvent("event", null, target);
        Assert.True(called);
    }

    [Test]
    public void testListeningNotTargetedAndNotReceivingTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        bool called = false;
        UnityAction<object> listener = message => { called = true; };
        eventManager.StartListening("event", listener);
        eventManager.TriggerEvent("event", null, target);
        Assert.False(called);
    }

    [Test]
    public void testListeningTargetedAndNotReceivingNotTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        bool called = false;
        UnityAction<object> listener = message => { called = true; };
        eventManager.StartListening("event", listener, target);
        eventManager.TriggerEvent("event", null);
        Assert.False(called);
    }

    [Test]
    public void testMessageIsSend() {
        EventManager eventManager = new EventManager();
        string receivedMessage = "";
        UnityAction<object> listener = message => { receivedMessage = (string) message; };
        eventManager.StartListening("event", listener);
        eventManager.TriggerEvent("event", "test message");
        Assert.AreEqual("test message", receivedMessage);
    }

    [Test]
    public void testMessageIsSendTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();
        string receivedMessage = "";
        UnityAction<object> listener = message => { receivedMessage = (string)message; };
        eventManager.StartListening("event", listener, target);
        eventManager.TriggerEvent("event", "test message", target);
        Assert.AreEqual("test message", receivedMessage);
    }

    [Test]
    public void testCountsMessageListenerTargeted() {
        Object target = GetTarget();
        EventManager eventManager = new EventManager();

        UnityAction<object> listener  = message => { };
        UnityAction<object> listener2 = message => { };
        UnityAction<object> listener3 = message => { };

        Assert.AreEqual(0, eventManager.ListenerCount("event", target));
        eventManager.StartListening("event", listener, target);
        Assert.AreEqual(1, eventManager.ListenerCount("event", target));
        eventManager.StartListening("event", listener2, target);
        Assert.AreEqual(2, eventManager.ListenerCount("event", target));
        eventManager.StartListening("event", listener3, target);
        Assert.AreEqual(3, eventManager.ListenerCount("event", target));
        eventManager.StopListening("event", listener2, target);
        Assert.AreEqual(2, eventManager.ListenerCount("event", target));
    }

    [Test]
    public void testCountsMessageListenerNotTargeted() {
        EventManager eventManager = new EventManager();

        UnityAction<object> listener = message => { };
        UnityAction<object> listener2 = message => { };
        UnityAction<object> listener3 = message => { };

        Assert.AreEqual(0, eventManager.ListenerCount("event"));
        eventManager.StartListening("event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
        eventManager.StartListening("event", listener2);
        Assert.AreEqual(2, eventManager.ListenerCount("event"));
        eventManager.StartListening("event", listener3);
        Assert.AreEqual(3, eventManager.ListenerCount("event"));
        eventManager.StopListening("event", listener2);
        Assert.AreEqual(2, eventManager.ListenerCount("event"));
    }

    [Test]
    public void testCountsMessageListeneingTwice() {
        EventManager eventManager = new EventManager();

        UnityAction<object> listener = message => { };

        Assert.AreEqual(0, eventManager.ListenerCount("event"));
        eventManager.StartListening("event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
        eventManager.StartListening("event", listener);
        Assert.AreEqual(2, eventManager.ListenerCount("event"));
        eventManager.StopListening("event", listener);
        Assert.AreEqual(1, eventManager.ListenerCount("event"));
    }


    [Test]
    public void testDelayedEventIsNotSentBeforeTime() {
        bool received = false;
        UnityAction<object> listener = message => { received = (string)message == "test"; };
        float time = 1000;

        EventManager eventManager = new EventManager(() => { return time; });

        eventManager.StartListening("event", listener);
        eventManager.TriggerEventAfter(1.0f, "event", "test");
        Assert.False(received);
        eventManager.AllwaysUpdate();
        Assert.False(received);
    }

    [Test]
    public void testDelayedEventIsSentAfterExactTime() {
        bool received = false;
        UnityAction<object> listener = message => { received = (string)message == "test"; };
        float time = 1000;

        EventManager eventManager = new EventManager(() => { return time; });

        eventManager.StartListening("event", listener);
        eventManager.TriggerEventAfter(1.0f, "event", "test");
        time = 1001;
        eventManager.AllwaysUpdate();
        Assert.True(received);
    }

    [Test]
    public void testDelayedEventIsSentAfterExtraTime() {
        bool received = false;
        UnityAction<object> listener = message => { received = (string)message == "test"; };
        float time = 1000;

        EventManager eventManager = new EventManager(() => { return time; });

        eventManager.StartListening("event", listener);
        eventManager.TriggerEventAfter(1.0f, "event", "test");
        time = 10001;
        eventManager.AllwaysUpdate();
        Assert.True(received);
    }

    [Test]
    public void testFlushedDelayedTargetedEventAreNotSent() {
        bool received = false;
        UnityAction<object> listener = message => { received = (string)message == "test"; };
        float time = 1000;
        Object target = GetTarget();

        EventManager eventManager = new EventManager(() => { return time; });

        eventManager.StartListening("event", listener, target);
        eventManager.TriggerEventAfter(1.0f, "event", "test", target);
        time = 10001;
        eventManager.FlushDelayedEvents("event");
        eventManager.AllwaysUpdate();
        Assert.True(received);
    }
}
