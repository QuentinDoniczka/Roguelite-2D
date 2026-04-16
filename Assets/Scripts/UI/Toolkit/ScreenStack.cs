using System.Collections.Generic;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class ScreenStack
    {
        private readonly Stack<IScreen> _stack;
        private readonly IScreen _root;

        public IScreen Current => _stack.Peek();
        public int Count => _stack.Count;

        public ScreenStack(IScreen rootScreen)
        {
            _root = rootScreen;
            _stack = new Stack<IScreen>();
            _stack.Push(_root);
        }

        public void Push(IScreen screen)
        {
            Current.OnPush();
            _stack.Push(screen);
            screen.OnShow();
        }

        public IScreen Pop()
        {
            if (_stack.Count <= 1)
            {
                return null;
            }

            IScreen popped = _stack.Pop();
            popped.OnHide();
            Current.OnPop();
            return popped;
        }

        public void Clear()
        {
            while (_stack.Count > 1)
            {
                IScreen popped = _stack.Pop();
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
