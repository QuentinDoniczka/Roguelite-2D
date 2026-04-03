using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
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
        [SerializeField] private float _specialSphereSize = 24f;
        [SerializeField] private float _lineHeight = 3f;
        [SerializeField] private float _lineMinWidth = 4f;

        private const float ScrollDotSizeMultiplier = 1.2f;

        private LevelManager _levelManager;
        private WorldConveyor _conveyor;
        private HorizontalLayoutGroup _layoutGroup;
        private readonly List<Image> _spheres = new List<Image>();
        private readonly List<Image> _lines = new List<Image>();
        private Func<int, StepType> _getStepType;
        private bool _initializedForTest;

        private int _currentStep;
        private Image _scrollDot;
        private RectTransform _scrollDotRect;
        private int _dotFromIndex;
        private int _dotToIndex;
        private bool _dotActive;
        private readonly Vector3[] _worldCorners = new Vector3[4];

        private void Awake()
        {
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        }

        private void Start()
        {
            if (_initializedForTest) return;

            var managers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length > 0)
            {
                _levelManager = managers[0];
                _levelManager.OnLevelStarted += OnLevelChanged;
                _levelManager.OnStepStarted += OnStepChanged;
                _getStepType = _levelManager.GetStepType;
                WireConveyor();
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

            UnwireConveyor();
        }

        private void Rebuild(int totalSteps, int currentStep)
        {
            StopDotScroll();
            _spheres.Clear();
            _lines.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
#endif
                    Destroy(child);
            }

            _layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            _layoutGroup.childControlWidth = true;
            _layoutGroup.childControlHeight = true;
            _layoutGroup.childForceExpandWidth = false;
            _layoutGroup.childForceExpandHeight = false;
            _layoutGroup.spacing = 0;

            for (int i = 0; i < totalSteps; i++)
            {
                var sphereGo = new GameObject($"Sphere_{i}");
                sphereGo.transform.SetParent(transform, false);

                var sphereImage = sphereGo.AddComponent<Image>();
                sphereImage.sprite = _sphereSprite;
                sphereImage.raycastTarget = false;
                _spheres.Add(sphereImage);

                bool isSpecial = _getStepType != null && _getStepType(i) == StepType.Special;
                float size = isSpecial ? _specialSphereSize : _sphereSize;

                var sphereLayout = sphereGo.AddComponent<LayoutElement>();
                sphereLayout.minWidth = size;
                sphereLayout.minHeight = size;
                sphereLayout.preferredWidth = size;
                sphereLayout.preferredHeight = size;

                if (i < totalSteps - 1)
                {
                    var lineWrapperGo = new GameObject($"LineWrapper_{i}");
                    lineWrapperGo.transform.SetParent(transform, false);

                    var wrapperLayout = lineWrapperGo.AddComponent<LayoutElement>();
                    wrapperLayout.flexibleWidth = 1;
                    wrapperLayout.minWidth = _lineMinWidth;
                    wrapperLayout.preferredHeight = _sphereSize;
                    wrapperLayout.minHeight = _sphereSize;

                    var lineGo = new GameObject($"Line_{i}");
                    lineGo.transform.SetParent(lineWrapperGo.transform, false);

                    var lineImage = lineGo.AddComponent<Image>();

                    var lineRect = lineGo.GetComponent<RectTransform>();
                    lineRect.anchorMin = new Vector2(0f, 0.5f);
                    lineRect.anchorMax = new Vector2(1f, 0.5f);
                    lineRect.sizeDelta = new Vector2(0f, _lineHeight);
                    lineRect.anchoredPosition = Vector2.zero;
                    lineImage.raycastTarget = false;
                    _lines.Add(lineImage);
                }
            }

            UpdateVisuals(currentStep);
            CreateScrollDot();
        }

        private void CreateScrollDot()
        {
            var dotGo = new GameObject("ScrollDot");
            dotGo.transform.SetParent(transform, false);

            _scrollDot = dotGo.AddComponent<Image>();
            _scrollDot.sprite = _sphereSprite;
            _scrollDot.color = _currentColor;
            _scrollDot.raycastTarget = false;

            _scrollDotRect = dotGo.GetComponent<RectTransform>();
            float dotSize = _sphereSize * ScrollDotSizeMultiplier;
            _scrollDotRect.sizeDelta = new Vector2(dotSize, dotSize);

            var layout = dotGo.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;

            dotGo.SetActive(false);
        }

        private void UpdateVisuals(int currentStep)
        {
            _currentStep = currentStep;

            for (int i = 0; i < _spheres.Count; i++)
            {
                if (i < currentStep)
                    _spheres[i].color = _completedColor;
                else if (i == currentStep)
                    _spheres[i].color = _dotActive ? _completedColor : _currentColor;
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

        private void WireConveyor()
        {
            _conveyor = _levelManager != null ? _levelManager.GetComponent<WorldConveyor>() : null;
            if (_conveyor != null)
            {
                _conveyor.OnScrollStarted += OnConveyorScrollStarted;
                _conveyor.OnScrollComplete += OnConveyorScrollComplete;
            }
        }

        private void UnwireConveyor()
        {
            if (_conveyor != null)
            {
                _conveyor.OnScrollStarted -= OnConveyorScrollStarted;
                _conveyor.OnScrollComplete -= OnConveyorScrollComplete;
            }
        }

        private void OnConveyorScrollStarted()
        {
            int toIndex = _currentStep + 1;
            if (toIndex <= _spheres.Count)
                StartDotScroll(_currentStep, toIndex);
        }

        private void OnConveyorScrollComplete()
        {
            StopDotScroll();
            UpdateVisuals(_currentStep);
        }

        private void StartDotScroll(int fromIndex, int toIndex)
        {
            if (_scrollDot == null) return;
            _dotFromIndex = fromIndex;
            _dotToIndex = toIndex;
            _dotActive = true;
            _scrollDot.gameObject.SetActive(true);
            _scrollDot.transform.SetAsLastSibling();
            UpdateVisuals(_currentStep);
        }

        private void StopDotScroll()
        {
            if (!_dotActive) return;
            _dotActive = false;
            if (_scrollDot != null)
                _scrollDot.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!_dotActive || _conveyor == null || _scrollDotRect == null) return;
            if (_dotFromIndex < 0 || _dotFromIndex >= _spheres.Count) return;

            Vector3 fromPos = _spheres[_dotFromIndex].transform.position;
            Vector3 toPos;

            if (_dotToIndex < _spheres.Count)
            {
                toPos = _spheres[_dotToIndex].transform.position;
            }
            else
            {
                var parentRect = (RectTransform)transform;
                parentRect.GetWorldCorners(_worldCorners);
                toPos = new Vector3(_worldCorners[2].x, fromPos.y, fromPos.z);
            }

            float t = _conveyor.ScrollProgress;
            _scrollDotRect.position = Vector3.Lerp(fromPos, toPos, t);
        }

        internal void InitializeForTest(LevelManager levelManager)
        {
            _initializedForTest = true;
            _levelManager = levelManager;
            _getStepType = _levelManager.GetStepType;

            _levelManager.OnLevelStarted += OnLevelChanged;
            _levelManager.OnStepStarted += OnStepChanged;
            WireConveyor();
            Rebuild(_levelManager.TotalStepsInCurrentLevel, _levelManager.CurrentStepIndex);
        }

        internal void SimulateStepChange(int stepIndex) => OnStepChanged(stepIndex);
        internal void SimulateScrollStart() => OnConveyorScrollStarted();

        internal int SphereCount => _spheres.Count;
        internal int LineCount => _lines.Count;
        internal Color GetSphereColor(int index) => _spheres[index].color;
        internal Color GetLineColor(int index) => _lines[index].color;
        internal Color CompletedColor => _completedColor;
        internal Color CurrentColor => _currentColor;
        internal Color UpcomingColor => _upcomingColor;
        internal bool IsScrollDotActive => _scrollDot != null && _scrollDot.gameObject.activeSelf;
        internal float GetSpherePreferredWidth(int index) => _spheres[index].GetComponent<LayoutElement>().preferredWidth;
        internal float SphereSize => _sphereSize;
        internal float SpecialSphereSize => _specialSphereSize;
    }
}
