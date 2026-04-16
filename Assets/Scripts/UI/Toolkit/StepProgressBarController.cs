using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class StepProgressBarController
    {
        private const string SphereClass = "step-sphere";
        private const string SphereSpecialClass = "step-sphere--special";
        private const string SphereCompletedClass = "step-sphere--completed";
        private const string SphereCurrentClass = "step-sphere--current";
        private const string SphereUpcomingClass = "step-sphere--upcoming";
        private const string LineClass = "step-line";
        private const string LineCompletedClass = "step-line--completed";
        private const string LineUpcomingClass = "step-line--upcoming";
        private const string ScrollDotClass = "step-scroll-dot";

        private readonly VisualElement _container;
        private readonly List<VisualElement> _spheres = new List<VisualElement>();
        private readonly List<VisualElement> _lines = new List<VisualElement>();

        private VisualElement _scrollDot;
        private LevelManager _levelManager;
        private WorldConveyor _conveyor;
        private int _currentStep;
        private bool _dotActive;
        private int _dotFromIndex;
        private int _dotToIndex;
        private Func<int, StepType> _getStepType;

        public StepProgressBarController(VisualElement container)
        {
            _container = container;
        }

        public void Initialize()
        {
            var managers = UnityEngine.Object.FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            if (managers.Length == 0)
            {
                return;
            }

            _levelManager = managers[0];
            _conveyor = _levelManager.GetComponent<WorldConveyor>();
            _getStepType = _levelManager.GetStepType;

            _levelManager.OnLevelStarted += OnLevelChanged;
            _levelManager.OnStepStarted += OnStepChanged;

            if (_conveyor != null)
            {
                _conveyor.OnScrollStarted += OnConveyorScrollStarted;
                _conveyor.OnScrollComplete += OnConveyorScrollComplete;
            }

            Rebuild(_levelManager.TotalStepsInCurrentLevel, _levelManager.CurrentStepIndex);
        }

        public void Dispose()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelStarted -= OnLevelChanged;
                _levelManager.OnStepStarted -= OnStepChanged;
            }

            if (_conveyor != null)
            {
                _conveyor.OnScrollStarted -= OnConveyorScrollStarted;
                _conveyor.OnScrollComplete -= OnConveyorScrollComplete;
            }
        }

        public void UpdateDotPosition()
        {
            if (!_dotActive || _conveyor == null) return;
            if (_dotFromIndex < 0 || _dotFromIndex >= _spheres.Count) return;

            float fromX = _spheres[_dotFromIndex].worldBound.center.x;
            float toX;

            if (_dotToIndex < _spheres.Count)
            {
                toX = _spheres[_dotToIndex].worldBound.center.x;
            }
            else
            {
                toX = _container.worldBound.xMax;
            }

            float t = _conveyor.ScrollProgress;
            float dotX = Mathf.Lerp(fromX, toX, t);

            float dotWidth = _scrollDot.resolvedStyle.width;
            float dotHeight = _scrollDot.resolvedStyle.height;

            float dotLocalX = dotX - _container.worldBound.x - (dotWidth / 2f);
            float dotLocalY = (_container.resolvedStyle.height - dotHeight) / 2f;

            _scrollDot.style.left = dotLocalX;
            _scrollDot.style.top = dotLocalY;
        }

        private void Rebuild(int totalSteps, int currentStep)
        {
            StopDotScroll();
            _spheres.Clear();
            _lines.Clear();
            _container.Clear();

            for (int i = 0; i < totalSteps; i++)
            {
                var sphere = new VisualElement();
                sphere.AddToClassList(SphereClass);

                if (_getStepType != null && _getStepType(i) == StepType.Special)
                {
                    sphere.AddToClassList(SphereSpecialClass);
                }

                _spheres.Add(sphere);
                _container.Add(sphere);

                if (i < totalSteps - 1)
                {
                    var line = new VisualElement();
                    line.AddToClassList(LineClass);
                    _lines.Add(line);
                    _container.Add(line);
                }
            }

            _scrollDot = new VisualElement();
            _scrollDot.AddToClassList(ScrollDotClass);
            _scrollDot.style.display = DisplayStyle.None;
            _scrollDot.style.position = Position.Absolute;
            _container.Add(_scrollDot);

            UpdateVisuals(currentStep);
        }

        private void UpdateVisuals(int currentStep)
        {
            _currentStep = currentStep;

            for (int i = 0; i < _spheres.Count; i++)
            {
                if (i < currentStep)
                {
                    SetSphereState(_spheres[i], SphereCompletedClass);
                }
                else if (i == currentStep)
                {
                    SetSphereState(_spheres[i], _dotActive ? SphereCompletedClass : SphereCurrentClass);
                }
                else
                {
                    SetSphereState(_spheres[i], SphereUpcomingClass);
                }
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                if (i < currentStep)
                {
                    SetLineState(_lines[i], LineCompletedClass);
                }
                else
                {
                    SetLineState(_lines[i], LineUpcomingClass);
                }
            }
        }

        private void SetSphereState(VisualElement sphere, string stateClass)
        {
            sphere.RemoveFromClassList(SphereCompletedClass);
            sphere.RemoveFromClassList(SphereCurrentClass);
            sphere.RemoveFromClassList(SphereUpcomingClass);
            sphere.AddToClassList(stateClass);
        }

        private void SetLineState(VisualElement line, string stateClass)
        {
            line.RemoveFromClassList(LineCompletedClass);
            line.RemoveFromClassList(LineUpcomingClass);
            line.AddToClassList(stateClass);
        }

        private void OnLevelChanged(int stageIndex, int levelIndex)
        {
            Rebuild(_levelManager.TotalStepsInCurrentLevel, 0);
        }

        private void OnStepChanged(int stepIndex)
        {
            UpdateVisuals(stepIndex);
        }

        private void OnConveyorScrollStarted()
        {
            int toIndex = _currentStep + 1;
            if (toIndex <= _spheres.Count)
            {
                StartDotScroll(_currentStep, toIndex);
            }
        }

        private void OnConveyorScrollComplete()
        {
            StopDotScroll();
            UpdateVisuals(_currentStep);
        }

        private void StartDotScroll(int fromIndex, int toIndex)
        {
            _dotFromIndex = fromIndex;
            _dotToIndex = toIndex;
            _dotActive = true;
            _scrollDot.style.display = DisplayStyle.Flex;
            _scrollDot.BringToFront();
            UpdateVisuals(_currentStep);
        }

        private void StopDotScroll()
        {
            if (!_dotActive) return;
            _dotActive = false;
            if (_scrollDot != null)
            {
                _scrollDot.style.display = DisplayStyle.None;
            }
        }

        internal int SphereCount => _spheres.Count;
        internal int LineCount => _lines.Count;
        internal bool IsScrollDotActive => _dotActive;

        internal bool SphereHasClass(int index, string className) =>
            _spheres[index].ClassListContains(className);

        internal void InitializeForTest(LevelManager levelManager)
        {
            _levelManager = levelManager;
            _conveyor = _levelManager.GetComponent<WorldConveyor>();
            _getStepType = _levelManager.GetStepType;

            _levelManager.OnLevelStarted += OnLevelChanged;
            _levelManager.OnStepStarted += OnStepChanged;

            if (_conveyor != null)
            {
                _conveyor.OnScrollStarted += OnConveyorScrollStarted;
                _conveyor.OnScrollComplete += OnConveyorScrollComplete;
            }

            Rebuild(_levelManager.TotalStepsInCurrentLevel, _levelManager.CurrentStepIndex);
        }

        internal void SimulateStepChange(int stepIndex) => OnStepChanged(stepIndex);
        internal void SimulateScrollStart() => OnConveyorScrollStarted();
    }
}
