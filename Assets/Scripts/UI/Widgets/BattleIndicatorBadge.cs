using System.Collections;
using RogueliteAutoBattler.Combat;
using TMPro;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class BattleIndicatorBadge : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text _compactLabel;
        [SerializeField] private TMP_Text _announcementLabel;

        [Header("Announcement")]
        [SerializeField] private CanvasGroup _announcementGroup;

        private const float AnnouncementFadeInDuration = 0.2f;
        private const float AnnouncementHoldDuration = 1.2f;
        private const float AnnouncementFadeOutDuration = 0.4f;
        private const float AnnouncementPeakScale = 1.1f;
        private const float AnnouncementStartScale = 0.7f;

        private LevelManager _levelManager;
        private Coroutine _announcementCoroutine;
        private RectTransform _announcementRect;

        private void Start()
        {
            var managers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length > 0)
            {
                _levelManager = managers[0];
                _levelManager.OnLevelStarted += OnLevelChanged;
                UpdateCompactLabel(_levelManager.CurrentStageIndex, _levelManager.CurrentLevelIndex);
            }

            if (_announcementGroup != null)
            {
                _announcementRect = _announcementGroup.GetComponent<RectTransform>();
                _announcementGroup.alpha = 0f;
            }
        }

        private void OnDestroy()
        {
            if (_levelManager != null)
                _levelManager.OnLevelStarted -= OnLevelChanged;
        }

        private void OnLevelChanged(int stageIndex, int levelIndex)
        {
            UpdateCompactLabel(stageIndex, levelIndex);
            PlayAnnouncement(stageIndex, levelIndex);
        }

        private void UpdateCompactLabel(int stageIndex, int levelIndex)
        {
            if (_compactLabel != null)
                _compactLabel.text = $"{stageIndex + 1}-{levelIndex + 1}";
        }

        private void PlayAnnouncement(int stageIndex, int levelIndex)
        {
            if (_announcementCoroutine != null)
                StopCoroutine(_announcementCoroutine);

            _announcementCoroutine = StartCoroutine(AnnouncementCoroutine(stageIndex, levelIndex));
        }

        private IEnumerator AnnouncementCoroutine(int stageIndex, int levelIndex)
        {
            if (_announcementLabel != null)
                _announcementLabel.text = $"Stage {stageIndex + 1} - Level {levelIndex + 1}";

            RectTransform announcementRect = _announcementRect;

            float elapsed = 0f;
            while (elapsed < AnnouncementFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / AnnouncementFadeInDuration);
                float easeOut = 1f - (1f - t) * (1f - t);

                if (_announcementGroup != null)
                    _announcementGroup.alpha = Mathf.Lerp(0f, 1f, easeOut);

                if (announcementRect != null)
                {
                    float scale = Mathf.Lerp(AnnouncementStartScale, AnnouncementPeakScale, easeOut);
                    announcementRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            if (_announcementGroup != null)
                _announcementGroup.alpha = 1f;
            if (announcementRect != null)
                announcementRect.localScale = Vector3.one;

            yield return new WaitForSeconds(AnnouncementHoldDuration);

            elapsed = 0f;
            while (elapsed < AnnouncementFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / AnnouncementFadeOutDuration);
                float easeIn = t * t;

                if (_announcementGroup != null)
                    _announcementGroup.alpha = Mathf.Lerp(1f, 0f, easeIn);

                if (announcementRect != null)
                {
                    float scale = Mathf.Lerp(1f, AnnouncementPeakScale, easeIn);
                    announcementRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            if (_announcementGroup != null)
                _announcementGroup.alpha = 0f;
            if (announcementRect != null)
                announcementRect.localScale = Vector3.one;

            _announcementCoroutine = null;
        }

        internal string CompactText => _compactLabel != null ? _compactLabel.text : "";
        internal float AnnouncementAlpha => _announcementGroup != null ? _announcementGroup.alpha : 0f;
        internal string AnnouncementText => _announcementLabel != null ? _announcementLabel.text : "";

        internal void InitializeForTest(LevelManager levelManager, TMP_Text compactLabel, TMP_Text announcementLabel, CanvasGroup announcementGroup)
        {
            _levelManager = levelManager;
            _compactLabel = compactLabel;
            _announcementLabel = announcementLabel;
            _announcementGroup = announcementGroup;

            _levelManager.OnLevelStarted += OnLevelChanged;
            UpdateCompactLabel(_levelManager.CurrentStageIndex, _levelManager.CurrentLevelIndex);

            if (_announcementGroup != null)
            {
                _announcementRect = _announcementGroup.GetComponent<RectTransform>();
                _announcementGroup.alpha = 0f;
            }
        }
    }
}
