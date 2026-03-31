using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class StepProgressBar : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Sprite _sphereSprite;
        [SerializeField] private Color _completedColor = Color.white;
        [SerializeField] private Color _currentColor = new Color(0.39f, 0.58f, 0.93f, 1f);
        [SerializeField] private Color _upcomingColor = new Color(1f, 1f, 1f, 0.3f);

        [Header("Sizing")]
        [SerializeField] private float _sphereSize = 16f;
        [SerializeField] private float _lineHeight = 3f;

        private LevelManager _levelManager;
        private readonly List<Image> _spheres = new();
        private readonly List<Image> _lines = new();
        private bool _initializedForTest;

        private void Start()
        {
            if (_initializedForTest) return;

            var managers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length > 0)
            {
                _levelManager = managers[0];
                _levelManager.OnStageStarted += OnStageChanged;
                _levelManager.OnLevelStarted += OnLevelChanged;
                Rebuild(_levelManager.TotalLevelsInCurrentStage, _levelManager.CurrentLevelIndex);
            }
        }

        private void OnDestroy()
        {
            if (_levelManager != null)
            {
                _levelManager.OnStageStarted -= OnStageChanged;
                _levelManager.OnLevelStarted -= OnLevelChanged;
            }
        }

        private void Rebuild(int totalLevels, int currentLevel)
        {
            _spheres.Clear();
            _lines.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            if (!TryGetComponent<HorizontalLayoutGroup>(out var layoutGroup))
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 0;

            for (int i = 0; i < totalLevels; i++)
            {
                var sphereGo = new GameObject($"Sphere_{i}");
                sphereGo.transform.SetParent(transform, false);

                var sphereImage = sphereGo.AddComponent<Image>();
                sphereImage.sprite = _sphereSprite;
                sphereImage.raycastTarget = false;
                _spheres.Add(sphereImage);

                var sphereLayout = sphereGo.AddComponent<LayoutElement>();
                sphereLayout.preferredWidth = _sphereSize;
                sphereLayout.preferredHeight = _sphereSize;

                if (i < totalLevels - 1)
                {
                    var lineGo = new GameObject($"Line_{i}");
                    lineGo.transform.SetParent(transform, false);

                    var lineImage = lineGo.AddComponent<Image>();
                    lineImage.raycastTarget = false;
                    _lines.Add(lineImage);

                    var lineLayout = lineGo.AddComponent<LayoutElement>();
                    lineLayout.flexibleWidth = 1;
                    lineLayout.minHeight = _lineHeight;
                    lineLayout.preferredHeight = _lineHeight;
                }
            }

            UpdateVisuals(currentLevel);
        }

        private void UpdateVisuals(int currentLevel)
        {
            for (int i = 0; i < _spheres.Count; i++)
            {
                if (i < currentLevel)
                    _spheres[i].color = _completedColor;
                else if (i == currentLevel)
                    _spheres[i].color = _currentColor;
                else
                    _spheres[i].color = _upcomingColor;
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].color = i < currentLevel ? _completedColor : _upcomingColor;
            }
        }

        private void OnStageChanged(int stageIndex, int levelIndex)
        {
            Rebuild(_levelManager.TotalLevelsInCurrentStage, levelIndex);
        }

        private void OnLevelChanged(int stageIndex, int levelIndex)
        {
            UpdateVisuals(levelIndex);
        }

        internal void InitializeForTest(LevelManager levelManager)
        {
            _initializedForTest = true;
            _levelManager = levelManager;

            if (_sphereSize == 0f) _sphereSize = 16f;
            if (_lineHeight == 0f) _lineHeight = 3f;

            _levelManager.OnStageStarted += OnStageChanged;
            _levelManager.OnLevelStarted += OnLevelChanged;
            Rebuild(_levelManager.TotalLevelsInCurrentStage, _levelManager.CurrentLevelIndex);
        }

        internal int SphereCount => _spheres.Count;
        internal int LineCount => _lines.Count;
        internal Color GetSphereColor(int index) => _spheres[index].color;
        internal Color GetLineColor(int index) => _lines[index].color;
        internal Color CompletedColor => _completedColor;
        internal Color CurrentColor => _currentColor;
        internal Color UpcomingColor => _upcomingColor;
    }
}
