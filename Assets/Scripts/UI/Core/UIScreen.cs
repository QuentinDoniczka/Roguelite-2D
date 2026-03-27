using UnityEngine;

namespace RogueliteAutoBattler.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            EnsureCanvasGroup();
        }

        public virtual void OnShow()
        {
            EnsureCanvasGroup();
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        public virtual void OnHide()
        {
            EnsureCanvasGroup();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        public virtual void OnPush()
        {
            OnHide();
        }

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
