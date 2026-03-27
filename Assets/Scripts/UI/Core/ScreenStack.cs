using System.Collections.Generic;

namespace RogueliteAutoBattler.UI.Core
{
    public class ScreenStack
    {
        private readonly Stack<UIScreen> _stack;
        private readonly UIScreen _root;

        public UIScreen Current => _stack.Peek();
        public int Count => _stack.Count;

        public ScreenStack(UIScreen rootScreen)
        {
            _root = rootScreen;
            _stack = new Stack<UIScreen>();
            _stack.Push(_root);
        }

        public void Push(UIScreen screen)
        {
            Current.OnPush();
            _stack.Push(screen);
            screen.OnShow();
        }

        public UIScreen Pop()
        {
            if (_stack.Count <= 1)
            {
                return null;
            }

            UIScreen popped = _stack.Pop();
            popped.OnHide();
            Current.OnPop();
            return popped;
        }

        public void Clear()
        {
            while (_stack.Count > 1)
            {
                UIScreen popped = _stack.Pop();
                popped.OnHide();
            }
        }

        public void ShowCurrent()
        {
            Current.OnShow();
        }

        public void HideCurrent()
        {
            Current.OnHide();
        }
    }
}
