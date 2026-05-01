using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal sealed class BranchTabController
    {
        private const float GhostDiameterPx = SkillTreeCanvasElement.NodeRadiusPx * 2f;
        private const float GhostHalfPx = SkillTreeCanvasElement.NodeRadiusPx;
        private const float DistanceMin = 0.5f;
        private const float DistanceMax = 10f;
        private const float DistanceDefault = 2f;
        private const float AngleMin = 0f;
        private const float AngleMax = 360f;
        private const float AngleDefault = 0f;
        private const int CostDefault = 1;
        private const float CostMultiplierDefault = 1f;
        private const int CostAdditiveDefault = 0;
        private const float StatModifierValuePerLevelDefault = 1f;
        private const int FirstNodeId = 1;

        private const int MaxLevelFallback = 1;

        private readonly SkillTreeData _data;
        private readonly SkillTreeCanvasElement _canvas;
        private readonly Func<int?> _selectedIdProvider;

        private readonly Slider _distance;
        private readonly Slider _angle;
        private readonly EnumField _stat;
        private readonly IntegerField _cost;
        private readonly EnumField _costType;
        private readonly Button _generate;
        private readonly VisualElement _ghost;

        internal BranchTabController(
            VisualElement tabContent,
            VisualElement canvasOverlayHost,
            SkillTreeData data,
            SkillTreeCanvasElement canvas,
            Func<int?> selectedIdProvider)
        {
            _data = data;
            _canvas = canvas;
            _selectedIdProvider = selectedIdProvider;

            _distance = new Slider("Distance", DistanceMin, DistanceMax) { name = "branch-distance", value = DistanceDefault };
            _distance.RegisterValueChangedCallback(_ => UpdatePreview());

            _angle = new Slider("Angle (°, 0=up clockwise)", AngleMin, AngleMax) { name = "branch-angle", value = AngleDefault };
            _angle.RegisterValueChangedCallback(_ => UpdatePreview());

            _stat = new EnumField("Stat", StatType.Hp) { name = "branch-stat" };

            _cost = new IntegerField("Cost") { name = "branch-cost", value = CostDefault };

            _costType = new EnumField("Cost Type", SkillTreeData.CostType.Gold) { name = "branch-cost-type" };

            _generate = new Button(Generate) { name = "branch-generate", text = "Generate" };
            _generate.SetEnabled(false);

            tabContent.Add(_distance);
            tabContent.Add(_angle);
            tabContent.Add(_stat);
            tabContent.Add(_cost);
            tabContent.Add(_costType);
            tabContent.Add(_generate);

            _ghost = new VisualElement();
            _ghost.AddToClassList("preview-ghost");
            _ghost.style.position = Position.Absolute;
            _ghost.style.width = GhostDiameterPx;
            _ghost.style.height = GhostDiameterPx;
            _ghost.style.display = DisplayStyle.None;
            canvasOverlayHost.Add(_ghost);
        }

        internal void OnSelectionChanged(int? selectedId)
        {
            _generate.SetEnabled(selectedId.HasValue);
            UpdatePreview();
        }

        internal void UpdatePreview()
        {
            int? parentId = _selectedIdProvider();
            if (!parentId.HasValue || _data == null)
            {
                _ghost.style.display = DisplayStyle.None;
                return;
            }

            SkillTreeData.SkillNodeEntry? parentNode = FindNode(parentId.Value);
            if (!parentNode.HasValue)
            {
                _ghost.style.display = DisplayStyle.None;
                return;
            }

            Vector2 newDataPos = BranchGeometry.ComputeBranchPosition(parentNode.Value.position, _distance.value, _angle.value);
            Vector2 screenPx = _canvas.DataToScreen(newDataPos);

            _ghost.style.translate = new StyleTranslate(new Translate(
                screenPx.x - GhostHalfPx,
                screenPx.y - GhostHalfPx));
            _ghost.style.display = DisplayStyle.Flex;
        }

        internal SkillTreeData.SkillNodeEntry BuildEntryFromUI()
        {
            int? parentId = _selectedIdProvider();
            SkillTreeData.SkillNodeEntry? parentNode = parentId.HasValue ? FindNode(parentId.Value) : null;

            Vector2 newPos = parentNode.HasValue
                ? BranchGeometry.ComputeBranchPosition(parentNode.Value.position, _distance.value, _angle.value)
                : Vector2.zero;

            int newId = ComputeNextId();
            var statType = (StatType)_stat.value;

            return new SkillTreeData.SkillNodeEntry
            {
                id = newId,
                position = newPos,
                connectedNodeIds = new List<int>(),
                costType = (SkillTreeData.CostType)_costType.value,
                maxLevel = _data != null ? _data.DefaultGeneratedMaxLevel : MaxLevelFallback,
                baseCost = _cost.value,
                costMultiplierOdd = CostMultiplierDefault,
                costMultiplierEven = CostMultiplierDefault,
                costAdditivePerLevel = CostAdditiveDefault,
                statModifierType = statType,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = StatModifierValuePerLevelDefault
            };
        }

        internal void Generate()
        {
            int? parentId = _selectedIdProvider();
            if (!parentId.HasValue || _data == null) return;

            SkillTreeData.SkillNodeEntry? parentNode = FindNode(parentId.Value);
            if (!parentNode.HasValue) return;

            SkillTreeData.SkillNodeEntry entry = BuildEntryFromUI();

            _data.AddBranchNode(entry, parentId.Value);
            EditorUtility.SetDirty(_data);

            _canvas.SetData(_data, parentId);
        }

        private SkillTreeData.SkillNodeEntry? FindNode(int id)
        {
            if (_data == null) return null;
            foreach (var node in _data.Nodes)
            {
                if (node.id == id) return node;
            }
            return null;
        }

        private int ComputeNextId()
        {
            if (_data == null || _data.Nodes.Count == 0) return FirstNodeId;
            int max = 0;
            foreach (var node in _data.Nodes)
            {
                if (node.id > max) max = node.id;
            }
            return max + 1;
        }

        internal Slider Distance => _distance;
        internal Slider Angle => _angle;
        internal EnumField Stat => _stat;
        internal IntegerField Cost => _cost;
        internal EnumField CostType => _costType;
        internal Button GenerateButton => _generate;
        internal VisualElement PreviewElement => _ghost;
    }
}
