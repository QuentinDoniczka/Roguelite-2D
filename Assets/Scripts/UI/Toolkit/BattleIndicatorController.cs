using System.Collections;
using RogueliteAutoBattler.Combat.Levels;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class BattleIndicatorController
    {
        private const float FadeInDuration = 0.2f;
        private const float HoldDuration = 1.2f;
        private const float FadeOutDuration = 0.4f;
        private const float PeakScale = 1.1f;
        private const float StartScale = 0.7f;
        private const string VisibleClass = "announcement-overlay--visible";

        private static readonly WaitForSeconds WaitAnnouncementHold = new WaitForSeconds(HoldDuration);

        private readonly Label _compactLabel;
        private readonly VisualElement _announcementOverlay;
        private readonly Label _announcementLabel;
        private readonly MonoBehaviour _coroutineHost;

        private LevelManager _levelManager;
        private Coroutine _announcementCoroutine;

        public BattleIndicatorController(
            Label compactLabel,
            VisualElement announcementOverlay,
            Label announcementLabel,
            MonoBehaviour coroutineHost)
        {
            _compactLabel = compactLabel;
            _announcementOverlay = announcementOverlay;
            _announcementLabel = announcementLabel;
            _coroutineHost = coroutineHost;
        }

        public void Initialize()
        {
            var managers = Object.FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length == 0)
            {
                return;
            }

            _levelManager = managers[0];
            _levelManager.OnLevelStarted += OnLevelChanged;
            UpdateCompactLabel(_levelManager.CurrentStageIndex, _levelManager.CurrentLevelIndex);
        }

        public void Dispose()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelStarted -= OnLevelChanged;
            }
        }

        private void OnLevelChanged(int stageIndex, int levelIndex)
        {
            UpdateCompactLabel(stageIndex, levelIndex);
            PlayAnnouncement(stageIndex, levelIndex);
        }

        private void UpdateCompactLabel(int stageIndex, int levelIndex)
        {
            _compactLabel.text = $"{stageIndex + 1}-{levelIndex + 1}";
        }

        private void PlayAnnouncement(int stageIndex, int levelIndex)
        {
            if (_announcementCoroutine != null)
            {
                _coroutineHost.StopCoroutine(_announcementCoroutine);
            }

            _announcementCoroutine = _coroutineHost.StartCoroutine(AnnouncementCoroutine(stageIndex, levelIndex));
        }

        private IEnumerator AnnouncementCoroutine(int stageIndex, int levelIndex)
        {
            _announcementLabel.text = $"Stage {stageIndex + 1} - Level {levelIndex + 1}";
            _announcementOverlay.AddToClassList(VisibleClass);

            float elapsed = 0f;
            while (elapsed < FadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeInDuration);
                float easeOut = 1f - (1f - t) * (1f - t);

                _announcementOverlay.style.opacity = new StyleFloat(Mathf.Lerp(0f, 1f, easeOut));
                float scale = Mathf.Lerp(StartScale, PeakScale, easeOut);
                _announcementOverlay.style.scale = new Scale(new Vector3(scale, scale, 1f));

                yield return null;
            }

            _announcementOverlay.style.opacity = new StyleFloat(1f);
            _announcementOverlay.style.scale = new Scale(new Vector3(1f, 1f, 1f));

            yield return WaitAnnouncementHold;

            elapsed = 0f;
            while (elapsed < FadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeOutDuration);
                float easeIn = t * t;

                _announcementOverlay.style.opacity = new StyleFloat(Mathf.Lerp(1f, 0f, easeIn));
                float scale = Mathf.Lerp(1f, PeakScale, easeIn);
                _announcementOverlay.style.scale = new Scale(new Vector3(scale, scale, 1f));

                yield return null;
            }

            _announcementOverlay.style.opacity = new StyleFloat(0f);
            _announcementOverlay.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            _announcementOverlay.RemoveFromClassList(VisibleClass);

            _announcementCoroutine = null;
        }

        internal string CompactText => _compactLabel.text;
        internal float AnnouncementOpacity => _announcementOverlay.resolvedStyle.opacity;
        internal string AnnouncementText => _announcementLabel.text;

        internal void InitializeForTest(LevelManager levelManager)
        {
            _levelManager = levelManager;
            _levelManager.OnLevelStarted += OnLevelChanged;
            UpdateCompactLabel(_levelManager.CurrentStageIndex, _levelManager.CurrentLevelIndex);
        }
    }
}
