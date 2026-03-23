using UnityEngine;

namespace RogueliteAutoBattler.UI.Core
{
    /// <summary>
    /// Base class for all UI screens. Uses CanvasGroup for visibility control.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            EnsureCanvasGroup();
        }

        /// <summary>
        /// Called when this screen becomes visible.
        /// </summary>
        public virtual void OnShow()
        {
            EnsureCanvasGroup();
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// Called when this screen is hidden.
        /// </summary>
        public virtual void OnHide()
        {
            EnsureCanvasGroup();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        /// <summary>
        /// Called when another screen is pushed on top of this one.
        /// </summary>
        public virtual void OnPush()
        {
            OnHide();
        }

        /// <summary>
        /// Called when this screen returns to the front after the one above is popped.
        /// </summary>
        public virtual void OnPop()
        {
            OnShow();
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}
