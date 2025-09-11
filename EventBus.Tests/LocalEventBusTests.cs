using FluffyVoid.Events;
using NSubstitute;

namespace FluffyVoid.EventBus.Tests;

public class LocalEventBusTests
{
    private LocalEventBus _eventBus;
    private MockEvent _event;
    private Action<object, MockEvent> _subscriber = Substitute.For<Action<object, MockEvent>>();

    [SetUp]
    public void Setup()
    {
        _eventBus = new LocalEventBus();
        _event = Substitute.For<MockEvent>();
        _subscriber.ClearReceivedCalls();
    }

    [Test]
    public void TestPublishWithNoSubscribers()
    {
        Assert.DoesNotThrow(() => _eventBus.Publish(Substitute.For<object>(), Substitute.For<EventArgs>()));

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
        _subscriber.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<MockEvent>());
    }
    [Test]
    public void TestPublishWithSingleSubscriber()
    {
        _eventBus.Subscribe(_subscriber);

        _eventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
        _subscriber.Received(1).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribers()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);

        _eventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(4));
        _subscriber.Received(4).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribersWhereOneCrashes()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe((object sender, MockEvent @event) =>
        {
            int i = 0;
            int j = 12 / i;
        });
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);

        _eventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(3));
        _subscriber.Received(3).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribersWhereOneIsNull()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe<MockEvent>(null!);
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);

        _eventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(3));
        _subscriber.Received(3).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithNullValues()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);

        _eventBus.Publish(null!, (MockEvent)null!);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(4));
        _subscriber.Received(4).Invoke(null!, null!);
    }
    [Test]
    public void TestSubscribeWithNewEvent()
    {
        _eventBus.Subscribe(_subscriber);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
    }
    [Test]
    public void TestSubscribeWithExistingEvent()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Subscribe(_subscriber);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(2));
    }
    [Test]
    public void TestSubscribeWithNullSubscriber()
    {
        _eventBus.Subscribe<MockEvent>(null!);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
    }
    [Test]
    public void TestUnsubscribeWithNoEventSubscribed()
    {
        _eventBus.Unsubscribe(_subscriber);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    [Test]
    public void TestUnsubscribeWithExistingEvent()
    {
        _eventBus.Subscribe(_subscriber);
        _eventBus.Unsubscribe(_subscriber);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    [Test]
    public void TestUnsubscribeWithNullSubscriber()
    {
        _eventBus.Unsubscribe<MockEvent>(null!);

        Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    public class MockSender
    {

    }
    public class MockEvent : EventArgs
    {

    }
}