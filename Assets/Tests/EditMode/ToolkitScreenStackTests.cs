using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ToolkitScreenStackTests
    {
        private ScreenStack _screenStack;
        private StubScreen _root;

        [SetUp]
        public void SetUp()
        {
            _root = new StubScreen();
            _screenStack = new ScreenStack(_root);
        }

        [Test]
        public void Constructor_SetsRootAsCurrent()
        {
            Assert.AreEqual(_root, _screenStack.Current);
            Assert.AreEqual(1, _screenStack.Count);
        }

        [Test]
        public void Push_MakesNewScreenCurrent()
        {
            var screen = new StubScreen();

            _screenStack.Push(screen);

            Assert.AreEqual(screen, _screenStack.Current);
            Assert.AreEqual(2, _screenStack.Count);
        }

        [Test]
        public void Push_CallsOnPushOnPrevious()
        {
            var screen = new StubScreen();

            _screenStack.Push(screen);

            Assert.AreEqual(1, _root.PushCount);
        }

        [Test]
        public void Push_CallsOnShowOnNew()
        {
            var screen = new StubScreen();

            _screenStack.Push(screen);

            Assert.AreEqual(1, screen.ShowCount);
        }

        [Test]
        public void Pop_ReturnsToPrevious()
        {
            var screen = new StubScreen();
            _screenStack.Push(screen);

            _screenStack.Pop();

            Assert.AreEqual(_root, _screenStack.Current);
        }

        [Test]
        public void Pop_CallsOnHideOnPopped()
        {
            var screen = new StubScreen();
            _screenStack.Push(screen);

            _screenStack.Pop();

            Assert.AreEqual(1, screen.HideCount);
        }

        [Test]
        public void Pop_CallsOnPopOnNewCurrent()
        {
            var screen = new StubScreen();
            _screenStack.Push(screen);

            _screenStack.Pop();

            Assert.AreEqual(1, _root.PopCount);
        }

        [Test]
        public void Pop_OnRootOnly_ReturnsNull()
        {
            IScreen result = _screenStack.Pop();

            Assert.IsNull(result);
            Assert.AreEqual(1, _screenStack.Count);
        }

        [Test]
        public void Clear_RemovesAllButRoot()
        {
            _screenStack.Push(new StubScreen());
            _screenStack.Push(new StubScreen());
            _screenStack.Push(new StubScreen());

            _screenStack.Clear();

            Assert.AreEqual(1, _screenStack.Count);
            Assert.AreEqual(_root, _screenStack.Current);
        }

        [Test]
        public void ShowCurrent_CallsOnShow()
        {
            _screenStack.ShowCurrent();

            Assert.AreEqual(1, _root.ShowCount);
        }

        [Test]
        public void HideCurrent_CallsOnHide()
        {
            _screenStack.HideCurrent();

            Assert.AreEqual(1, _root.HideCount);
        }
    }
}
