using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Widgets
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
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
        private readonly List<Image> _spheres = new List<Image>();
        private readonly List<Image> _lines = new List<Image>();
        private bool _initializedForTest;

        private void Start()
        {
            if (_initializedForTest) return;

            var managers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length > 0)
            {
                _levelManager = managers[0];
                _levelManager.OnLevelStarted += OnLevelChanged;
                _levelManager.OnStepStarted += OnStepChanged;
                Rebuild(_levelManager.TotalStepsInCurrentLevel, _levelManager.CurrentStepIndex);
            }
        }

        private void OnDestroy()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelStarted -= OnLevelChanged;
                _levelManager.OnStepStarted -= OnStepChanged;
            }
        }

        private void Rebuild(int totalSteps, int currentStep)
        {
            _spheres.Clear();
            _lines.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            var layoutGroup = GetComponent<HorizontalLayoutGroup>();

            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 0;

            for (int i = 0; i < totalSteps; i++)
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

                if (i < totalSteps - 1)
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

            UpdateVisuals(currentStep);
        }

        private void UpdateVisuals(int currentStep)
        {
            for (int i = 0; i < _spheres.Count; i++)
            {
                if (i < currentStep)
                    _spheres[i].color = _completedColor;
                else if (i == currentStep)
                    _spheres[i].color = _currentColor;
                else
                    _spheres[i].color = _upcomingColor;
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].color = i < currentStep ? _completedColor : _upcomingColor;
            }
        }

        private void OnLevelChanged(int stageIndex, int levelIndex)
        {
            Rebuild(_levelManager.TotalStepsInCurrentLevel, 0);
        }

        private void OnStepChanged(int stepIndex)
        {
            UpdateVisuals(stepIndex);
        }

        internal void InitializeForTest(LevelManager levelManager)
        {
            _initializedForTest = true;
            _levelManager = levelManager;

            _levelManager.OnLevelStarted += OnLevelChanged;
            _levelManager.OnStepStarted += OnStepChanged;
            Rebuild(_levelManager.TotalStepsInCurrentLevel, _levelManager.CurrentStepIndex);
        }

        internal void SimulateStepChange(int stepIndex) => UpdateVisuals(stepIndex);

        internal int SphereCount => _spheres.Count;
        internal int LineCount => _lines.Count;
        internal Color GetSphereColor(int index) => _spheres[index].color;
        internal Color GetLineColor(int index) => _lines[index].color;
        internal Color CompletedColor => _completedColor;
        internal Color CurrentColor => _currentColor;
        internal Color UpcomingColor => _upcomingColor;
    }
}
