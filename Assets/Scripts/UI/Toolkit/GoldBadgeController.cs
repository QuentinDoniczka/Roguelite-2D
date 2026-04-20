using System;
using System.Collections;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class GoldBadgeController
    {
        private const string LogTag = "[GoldBadgeController]";
        private const float PeakScale = 1.15f;
        private const float HalfDuration = 0.075f;
        private const float RestScale = 1f;

        private readonly VisualElement _badgeRoot;
        private readonly Label _goldLabel;
        private readonly MonoBehaviour _coroutineHost;

        private GoldWallet _wallet;
        private Coroutine _punchCoroutine;

        internal string DisplayText => _goldLabel.text;

        public GoldBadgeController(VisualElement badgeRoot, Label goldLabel, MonoBehaviour coroutineHost)
        {
            _badgeRoot = badgeRoot;
            _goldLabel = goldLabel;
            _coroutineHost = coroutineHost;
        }

        public void Initialize(GoldWallet wallet)
        {
            if (wallet == null)
            {
                Debug.LogWarning($"{LogTag} Initialize called with null wallet; badge will not update.");
                return;
            }

            _wallet = wallet;
            _wallet.OnGoldChanged += OnGoldChanged;
            _goldLabel.text = GoldFormatter.Format(_wallet.Gold);
        }

        public void Dispose()
        {
            if (_wallet != null)
            {
                _wallet.OnGoldChanged -= OnGoldChanged;
            }
        }

        public void Punch(Action onComplete = null)
        {
            if (_punchCoroutine != null)
            {
                _coroutineHost.StopCoroutine(_punchCoroutine);
            }

            _punchCoroutine = _coroutineHost.StartCoroutine(PunchCoroutine(onComplete));
        }

        private void OnGoldChanged(int total)
        {
            _goldLabel.text = GoldFormatter.Format(total);
            Punch();
        }

        private IEnumerator PunchCoroutine(Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < HalfDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / HalfDuration);
                float easeOut = 1f - (1f - normalizedTime) * (1f - normalizedTime);
                ApplyUniformScale(Mathf.Lerp(RestScale, PeakScale, easeOut));
                yield return null;
            }

            elapsed = 0f;

            while (elapsed < HalfDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / HalfDuration);
                float easeIn = normalizedTime * normalizedTime;
                ApplyUniformScale(Mathf.Lerp(PeakScale, RestScale, easeIn));
                yield return null;
            }

            ApplyUniformScale(RestScale);
            _punchCoroutine = null;
            onComplete?.Invoke();
        }

        private void ApplyUniformScale(float scale)
        {
            _badgeRoot.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }
    }
}
