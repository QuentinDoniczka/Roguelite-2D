using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public interface IScreen
    {
        VisualElement Root { get; }
        void OnShow();
        void OnHide();
        void OnPush();
        void OnPop();
    }
}
