using System;
using System.Collections;
using RogueliteAutoBattler.Economy;
using TMPro;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class GoldHudBadge : MonoBehaviour
    {
        private TMP_Text _label;
        private GoldWallet _wallet;
        private RectTransform _rectTransform;
        private Coroutine _punchCoroutine;

        private void Awake()
        {
            _label = GetComponentInChildren<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            var wallets = FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            if (wallets.Length > 0)
            {
                _wallet = wallets[0];
                _wallet.OnGoldChanged += UpdateDisplay;
                UpdateDisplay(_wallet.Gold);
            }
        }

        private void OnDestroy()
        {
            if (_wallet != null)
                _wallet.OnGoldChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int total)
        {
            if (_label != null)
                _label.text = GoldFormatter.Format(total);
        }

        public void Punch(Action onComplete = null)
        {
            if (_rectTransform == null)
                return;

            if (_punchCoroutine != null)
                StopCoroutine(_punchCoroutine);

            _punchCoroutine = StartCoroutine(PunchCoroutine(onComplete));
        }

        private IEnumerator PunchCoroutine(Action onComplete)
        {
            const float peakScale = 1.15f;
            const float halfDuration = 0.075f;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easeOut = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(1f, peakScale, easeOut);
                _rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easeIn = t * t;
                float scale = Mathf.Lerp(peakScale, 1f, easeIn);
                _rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            _rectTransform.localScale = Vector3.one;
            _punchCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
