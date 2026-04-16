using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    internal class StubScreen : IScreen
    {
        public const string HiddenClass = "hidden";

        public VisualElement Root { get; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public int PushCount { get; private set; }
        public int PopCount { get; private set; }

        public StubScreen()
        {
            Root = new VisualElement();
        }

        public void OnShow()
        {
            ShowCount++;
            Root.RemoveFromClassList(HiddenClass);
        }

        public void OnHide()
        {
            HideCount++;
            Root.AddToClassList(HiddenClass);
        }

        public void OnPush()
        {
            PushCount++;
            OnHide();
        }

        public void OnPop()
        {
            PopCount++;
            OnShow();
        }
    }
}
