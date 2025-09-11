using FluffyVoid.Events;
using NSubstitute;

namespace FluffyVoid.EventBus.Tests;

public class GlobalEventBusTests
{
    private MockEvent _event;
    private Action<object, MockEvent> _subscriber = Substitute.For<Action<object, MockEvent>>();

    [SetUp]
    public void Setup()
    {
        _event = Substitute.For<MockEvent>();
        int count = GlobalEventBus.Count<MockEvent>();
        for (int i = 0; i < count; ++i)
        {
            GlobalEventBus.Unsubscribe(_subscriber);
            GlobalEventBus.Unsubscribe<MockEvent>(null!);
        }
        _subscriber.ClearReceivedCalls();
    }

    [Test]
    public void TestPublishWithNoSubscribers()
    {
        Assert.DoesNotThrow(() => GlobalEventBus.Publish(Substitute.For<object>(), Substitute.For<EventArgs>()));

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(0));
        _subscriber.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<MockEvent>());
    }
    [Test]
    public void TestPublishWithSingleSubscriber()
    {
        GlobalEventBus.Subscribe(_subscriber);

        GlobalEventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(1));
        _subscriber.Received(1).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribers()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);

        GlobalEventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(4));
        _subscriber.Received(4).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribersWhereOneCrashes()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe((object sender, MockEvent @event) =>
        {
            int i = 0;
            int j = 12 / i;
        });
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);

        GlobalEventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(3));
        _subscriber.Received(3).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithMultipleSubscribersWhereOneIsNull()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe<MockEvent>(null!);
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);

        GlobalEventBus.Publish(Substitute.For<object>(), _event);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(3));
        _subscriber.Received(3).Invoke(Arg.Any<object>(), _event);
    }
    [Test]
    public void TestPublishWithNullValues()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);

        GlobalEventBus.Publish(null!, (MockEvent)null!);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(4));
        _subscriber.Received(4).Invoke(null!, null!);
    }
    [Test]
    public void TestSubscribeWithNewEvent()
    {
        GlobalEventBus.Subscribe(_subscriber);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(1));
    }
    [Test]
    public void TestSubscribeWithExistingEvent()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Subscribe(_subscriber);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(2));
    }
    [Test]
    public void TestSubscribeWithNullSubscriber()
    {
        GlobalEventBus.Subscribe<MockEvent>(null!);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(1));
    }
    [Test]
    public void TestUnsubscribeWithNoEventSubscribed()
    {
        GlobalEventBus.Unsubscribe(_subscriber);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    [Test]
    public void TestUnsubscribeWithExistingEvent()
    {
        GlobalEventBus.Subscribe(_subscriber);
        GlobalEventBus.Unsubscribe(_subscriber);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    [Test]
    public void TestUnsubscribeWithNullSubscriber()
    {
        GlobalEventBus.Unsubscribe<MockEvent>(null!);

        Assert.That(GlobalEventBus.Count<MockEvent>(), Is.EqualTo(0));
    }
    public class MockSender
    {

    }
    public class MockEvent : EventArgs
    {

    }
}