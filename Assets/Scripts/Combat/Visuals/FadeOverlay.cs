using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeOverlay : MonoBehaviour
    {
        private const float BlockRaycastsAlphaThreshold = 0.5f;

        [Header("Fade")]
        [SerializeField] private float _fadeDuration = 0.5f;
        private CanvasGroup _canvasGroup;
        private Coroutine _activeFade;

        internal float FadeDuration => _fadeDuration;
        internal float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;

        private void Awake()
        {
            Initialize();
        }

        public Coroutine FadeIn()
        {
            if (_activeFade != null) StopCoroutine(_activeFade);
            _activeFade = StartCoroutine(FadeCoroutine(1f));
            return _activeFade;
        }

        public Coroutine FadeOut()
        {
            if (_activeFade != null) StopCoroutine(_activeFade);
            _activeFade = StartCoroutine(FadeCoroutine(0f));
            return _activeFade;
        }

        private IEnumerator FadeCoroutine(float targetAlpha)
        {
            if (_fadeDuration <= 0f)
            {
                _canvasGroup.alpha = targetAlpha;
                _canvasGroup.blocksRaycasts = targetAlpha > BlockRaycastsAlphaThreshold;
                _activeFade = null;
                yield break;
            }

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = targetAlpha > BlockRaycastsAlphaThreshold;
            _activeFade = null;
        }

        internal void InitializeForTest()
        {
            Initialize();
        }

        private void Initialize()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        internal void SetFadeDurationForTest(float duration) => _fadeDuration = duration;
    }
}
