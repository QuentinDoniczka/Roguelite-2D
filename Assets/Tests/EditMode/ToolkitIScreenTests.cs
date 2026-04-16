using NUnit.Framework;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ToolkitIScreenTests
    {
        private StubScreen _stub;

        [SetUp]
        public void SetUp()
        {
            _stub = new StubScreen();
        }

        [Test]
        public void StubScreen_Root_ReturnsVisualElement()
        {
            Assert.IsNotNull(_stub.Root);
        }

        [Test]
        public void StubScreen_OnShow_RemovesHiddenClass()
        {
            _stub.Root.AddToClassList(StubScreen.HiddenClass);

            _stub.OnShow();

            Assert.IsFalse(_stub.Root.ClassListContains(StubScreen.HiddenClass));
        }

        [Test]
        public void StubScreen_OnHide_AddsHiddenClass()
        {
            _stub.OnHide();

            Assert.IsTrue(_stub.Root.ClassListContains(StubScreen.HiddenClass));
        }

        [Test]
        public void StubScreen_OnPush_CallsOnHide()
        {
            _stub.OnPush();

            Assert.AreEqual(1, _stub.HideCount);
        }

        [Test]
        public void StubScreen_OnPop_CallsOnShow()
        {
            _stub.OnPop();

            Assert.AreEqual(1, _stub.ShowCount);
        }
    }
}
