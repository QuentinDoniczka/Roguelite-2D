using System;
using System.Collections;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class GoldBadgeController
    {
        private const float PeakScale = 1.15f;
        private const float HalfDuration = 0.075f;

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

        public void Initialize()
        {
            var wallets = UnityEngine.Object.FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            if (wallets.Length == 0)
            {
                return;
            }

            _wallet = wallets[0];
            _wallet.OnGoldChanged += OnGoldChanged;
            _goldLabel.text = GoldFormatter.Format(_wallet.Gold);
        }

        internal void InitializeForTest(GoldWallet wallet)
        {
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
                float t = Mathf.Clamp01(elapsed / HalfDuration);
                float easeOut = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(1f, PeakScale, easeOut);
                _badgeRoot.style.scale = new Scale(new Vector3(scale, scale, 1f));
                yield return null;
            }

            elapsed = 0f;

            while (elapsed < HalfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / HalfDuration);
                float easeIn = t * t;
                float scale = Mathf.Lerp(PeakScale, 1f, easeIn);
                _badgeRoot.style.scale = new Scale(new Vector3(scale, scale, 1f));
                yield return null;
            }

            _badgeRoot.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            _punchCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
