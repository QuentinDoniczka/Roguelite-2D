using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Combat
{
    [RequireComponent(typeof(Image))]
    public class CoinFly : MonoBehaviour
    {
        private const float ARC_HEIGHT_RATIO = 0.3f;
        private const float MIN_ARC_HEIGHT = 100f;

        private Image _image;
        private RectTransform _rectTransform;
        private Action<CoinFly> _returnToPool;
        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(Action<CoinFly> returnToPool)
        {
            _returnToPool = returnToPool;
        }

        public void Play(Vector2 startAnchoredPos, Vector2 targetAnchoredPos, float duration, Action onArrive)
        {
            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);

            _rectTransform.anchoredPosition = startAnchoredPos;
            gameObject.SetActive(true);
            _activeCoroutine = StartCoroutine(AnimateCoroutine(startAnchoredPos, targetAnchoredPos, duration, onArrive));
        }

        private IEnumerator AnimateCoroutine(Vector2 start, Vector2 target, float duration, Action onArrive)
        {
            float distance = Vector2.Distance(start, target);
            float arcHeight = Mathf.Max(distance * ARC_HEIGHT_RATIO, MIN_ARC_HEIGHT);

            Vector2 midpoint = (start + target) * 0.5f;
            Vector2 controlPoint = midpoint + Vector2.up * arcHeight;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float raw = Mathf.Clamp01(elapsed / duration);
                float t = raw * (2f - raw);

                float oneMinusT = 1f - t;
                Vector2 position = oneMinusT * oneMinusT * start
                                   + 2f * oneMinusT * t * controlPoint
                                   + t * t * target;

                _rectTransform.anchoredPosition = position;
                yield return null;
            }

            _rectTransform.anchoredPosition = target;
            onArrive?.Invoke();
            gameObject.SetActive(false);
            _returnToPool?.Invoke(this);
        }
    }
}
