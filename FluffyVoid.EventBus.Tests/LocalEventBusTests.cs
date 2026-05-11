using NSubstitute;

namespace FluffyVoid.EventBus.Tests
{
    public class LocalEventBusTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public abstract class MockEvent : EventArgs { }

        private MockEvent     _event;
        private LocalEventBus _eventBus;
        private readonly Action<object, MockEvent> _subscriber =
            Substitute.For<Action<object, MockEvent>>();

        [SetUp]
        public void Setup()
        {
            _eventBus = new LocalEventBus();
            _event    = Substitute.For<MockEvent>();
            _subscriber.ClearReceivedCalls();
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
            _eventBus.Subscribe((object _, MockEvent _) =>
                {
                    // ReSharper disable once ConvertToConstant.Local
                    int i = 0;
                    // ReSharper disable once IntDivisionByZero
                    // ReSharper disable once UnusedVariable
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
        public void TestPublishWithNoSubscribers()
        {
            Assert.DoesNotThrow(() => _eventBus.Publish(
                                    Substitute.For<object>(),
                                    Substitute.For<EventArgs>()));

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
            _subscriber.DidNotReceive()
                       .Invoke(Arg.Any<object>(), Arg.Any<MockEvent>());
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
        public void TestPublishWithSingleSubscriber()
        {
            _eventBus.Subscribe(_subscriber);

            _eventBus.Publish(Substitute.For<object>(), _event);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
            _subscriber.Received(1).Invoke(Arg.Any<object>(), _event);
        }
        [Test]
        public void TestSubscribeWithExistingEvent()
        {
            _eventBus.Subscribe(_subscriber);
            _eventBus.Subscribe(_subscriber);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(2));
        }
        [Test]
        public void TestSubscribeWithNewEvent()
        {
            _eventBus.Subscribe(_subscriber);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
        }
        [Test]
        public void TestSubscribeWithNullSubscriber()
        {
            _eventBus.Subscribe<MockEvent>(null!);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(1));
        }
        [Test]
        public void TestUnsubscribeWithExistingEvent()
        {
            _eventBus.Subscribe(_subscriber);
            _eventBus.Unsubscribe(_subscriber);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
        }
        [Test]
        public void TestUnsubscribeWithNoEventSubscribed()
        {
            _eventBus.Unsubscribe(_subscriber);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
        }
        [Test]
        public void TestUnsubscribeWithNullSubscriber()
        {
            _eventBus.Unsubscribe<MockEvent>(null!);

            Assert.That(_eventBus.Count<MockEvent>(), Is.EqualTo(0));
        }
    }
}
