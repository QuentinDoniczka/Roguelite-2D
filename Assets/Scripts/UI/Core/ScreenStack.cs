using System.Collections.Generic;

namespace RogueliteAutoBattler.UI.Core
{
    /// <summary>
    /// Manages a stack of UIScreens for a single tab. Plain C# class.
    /// The root screen starts hidden; call ShowCurrent() to reveal it.
    /// </summary>
    public class ScreenStack
    {
        private readonly Stack<UIScreen> _stack;
        private readonly UIScreen _root;

        /// <summary>
        /// The screen currently on top of the stack.
        /// </summary>
        public UIScreen Current => _stack.Peek();

        /// <summary>
        /// Number of screens in the stack.
        /// </summary>
        public int Count => _stack.Count;

        public ScreenStack(UIScreen rootScreen)
        {
            _root = rootScreen;
            _stack = new Stack<UIScreen>();
            _stack.Push(_root);
        }

        /// <summary>
        /// Pushes a new screen on top of the current one.
        /// </summary>
        public void Push(UIScreen screen)
        {
            Current.OnPush();
            _stack.Push(screen);
            screen.OnShow();
        }

        /// <summary>
        /// Pops the top screen. Returns null if only the root remains.
        /// </summary>
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

        /// <summary>
        /// Pops all screens above the root, hiding each one.
        /// Does not show the root; call ShowCurrent() separately if needed.
        /// </summary>
        public void Clear()
        {
            while (_stack.Count > 1)
            {
                UIScreen popped = _stack.Pop();
                popped.OnHide();
            }
        }

        /// <summary>
        /// Shows the current top screen.
        /// </summary>
        public void ShowCurrent()
        {
            Current.OnShow();
        }

        /// <summary>
        /// Hides the current top screen.
        /// </summary>
        public void HideCurrent()
        {
            Current.OnHide();
        }
    }
}
