using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeOverlay : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.5f;
        private CanvasGroup _canvasGroup;

        internal float FadeDuration => _fadeDuration;
        internal float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        public Coroutine FadeIn() => StartCoroutine(FadeCoroutine(1f));
        public Coroutine FadeOut() => StartCoroutine(FadeCoroutine(0f));

        private IEnumerator FadeCoroutine(float targetAlpha)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
        }

        internal void InitializeForTest()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        internal void SetFadeDurationForTest(float duration) => _fadeDuration = duration;
    }
}
