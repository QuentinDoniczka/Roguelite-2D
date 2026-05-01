using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class SkillTreeDesignerWindow : EditorWindow
    {
        private const float MinZoom = 0.2f;
        private const float MaxZoom = 5f;
        private const float GridSpacing = 50f;
        private const float ConfigPanelWidthRatio = 0.3f;
        private const float NodeBorderThickness = 2f;
        private const float ZoomScrollSensitivity = 0.05f;
        private const float MinGridSpacingThreshold = 5f;
        private const float MinBranchPreviewDistance = 0.5f;
        private const float MaxBranchPreviewDistance = 10f;
        private const float MinBranchAngle = 0f;
        private const float MaxBranchAngle = 360f;
        private const float BranchPreviewDottedSegmentSize = 6f;
        private const int CostPreviewMaxRows = 10;
        private const float MinWindowWidth = 800f;
        private const float MinWindowHeight = 500f;
        private const int LeftMouseButton = 0;
        private const int MiddleMouseButton = 2;

        private static readonly Color CanvasBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color CrosshairColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color BranchPreviewTintColor = new Color(1f, 1f, 1f, 0.4f);

        private static readonly GUIContent LabelUnitSize = new GUIContent("Unit Size");
        private static readonly GUIContent LabelNodeSize = new GUIContent("Node Size");
        private static readonly GUIContent LabelNodeColor = new GUIContent("Node Color");
        private static readonly GUIContent LabelBorderNormal = new GUIContent("Border Normal");
        private static readonly GUIContent LabelBorderSelected = new GUIContent("Border Selected");
        private static readonly GUIContent LabelBaseCost = new GUIContent("Base Cost");
        private static readonly GUIContent LabelCostMultiplierOdd = new GUIContent("Multiplier (Odd Levels)");
        private static readonly GUIContent LabelCostMultiplierEven = new GUIContent("Multiplier (Even Levels)");
        private static readonly GUIContent LabelCostAdditive = new GUIContent("Additive / Level");
        private static readonly GUIContent LabelDefaultGeneratedMaxLevel = new GUIContent("Default Max Level (0 = unlimited)");
        private static readonly GUIContent LabelEdgeColor = new GUIContent("Edge Color");
        private static readonly GUIContent LabelEdgeThickness = new GUIContent("Edge Thickness");

        private SkillTreeData _data;
        private SerializedObject _serializedData;
        private SerializedProperty _propUnitSize;
        private SerializedProperty _propNodeSize;
        private SerializedProperty _propNodeColor;
        private SerializedProperty _propBorderNormalColor;
        private SerializedProperty _propBorderSelectedColor;
        private SerializedProperty _propBaseCost;
        private SerializedProperty _propCostMultiplierOdd;
        private SerializedProperty _propCostMultiplierEven;
        private SerializedProperty _propCostAdditivePerLevel;
        private SerializedProperty _propDefaultGeneratedMaxLevel;
        private SerializedProperty _propEdgeColor;
        private SerializedProperty _propEdgeThickness;
        private Vector2 _canvasOffset;
        private float _canvasZoom = 1f;
        private Vector2 _configScrollPos;
        private GUIStyle _nodeLabelStyle;
        private string[] _nodeLabels;
        private int _selectedNodeIndex = -1;
        private int _activeTab;
        private bool _branchPreviewActive;
        private int _branchPreviewParentIndex = -1;
        private int _branchPreviewPreviousTab;
        private BranchPreviewSettings _branchPreviewSettings = BranchPreviewSettings.Defaults;
        private BranchPreviewSettings _lastBranchPreviewSettings = BranchPreviewSettings.Defaults;

        [MenuItem("Roguelite/Skill Tree Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<SkillTreeDesignerWindow>("Skill Tree Designer");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            _data = AssetDatabase.LoadAssetAtPath<SkillTreeData>(EditorPaths.SkillTreeDataAsset);
            if (_data == null)
            {
                EditorUIFactory.EnsureDirectoryExists(EditorPaths.SkillTreeDataAsset);
                _data = CreateInstance<SkillTreeData>();
                AssetDatabase.CreateAsset(_data, EditorPaths.SkillTreeDataAsset);
                AssetDatabase.SaveAssets();
            }
            _serializedData = new SerializedObject(_data);
            CacheSerializedProperties();
            RebuildNodeLabels();
        }

        private void CacheSerializedProperties()
        {
            _propUnitSize = _serializedData.FindProperty(SkillTreeData.FieldNames.UnitSize);
            _propNodeSize = _serializedData.FindProperty(SkillTreeData.FieldNames.NodeSize);
            _propNodeColor = _serializedData.FindProperty(SkillTreeData.FieldNames.NodeColor);
            _propBorderNormalColor = _serializedData.FindProperty(SkillTreeData.FieldNames.BorderNormalColor);
            _propBorderSelectedColor = _serializedData.FindProperty(SkillTreeData.FieldNames.BorderSelectedColor);
            _propBaseCost = _serializedData.FindProperty(SkillTreeData.FieldNames.BaseCost);
            _propCostMultiplierOdd = _serializedData.FindProperty(SkillTreeData.FieldNames.CostMultiplierOdd);
            _propCostMultiplierEven = _serializedData.FindProperty(SkillTreeData.FieldNames.CostMultiplierEven);
            _propCostAdditivePerLevel = _serializedData.FindProperty(SkillTreeData.FieldNames.CostAdditivePerLevel);
            _propDefaultGeneratedMaxLevel = _serializedData.FindProperty(SkillTreeData.FieldNames.DefaultGeneratedMaxLevel);
            _propEdgeColor = _serializedData.FindProperty(SkillTreeData.FieldNames.EdgeColor);
            _propEdgeThickness = _serializedData.FindProperty(SkillTreeData.FieldNames.EdgeThickness);
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                EditorGUILayout.HelpBox("No SkillTreeData asset found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            float configWidth = position.width * ConfigPanelWidthRatio;
            float canvasWidth = position.width - configWidth;

            Rect canvasRect = new Rect(0, 0, canvasWidth, position.height);
            DrawCanvas(canvasRect);
            HandleCanvasInput(canvasRect);

            GUILayout.BeginArea(new Rect(canvasWidth, 0, configWidth, position.height));
            DrawConfigPanel();
            GUILayout.EndArea();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCanvas(Rect canvasRect)
        {
            EditorGUI.DrawRect(canvasRect, CanvasBackgroundColor);

            GUI.BeginClip(canvasRect);

            Vector2 center = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
            Vector2 origin = center + _canvasOffset;

            DrawGrid(canvasRect, origin);

            EditorGUI.DrawRect(new Rect(origin.x - 0.5f, 0, 1, canvasRect.height), CrosshairColor);
            EditorGUI.DrawRect(new Rect(0, origin.y - 0.5f, canvasRect.width, 1), CrosshairColor);

            if (_nodeLabelStyle == null)
            {
                _nodeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            float scaledUnit = _data.UnitSize * _canvasZoom;
            float scaledNodeSize = _data.NodeSize * _canvasZoom;
            float halfNode = scaledNodeSize * 0.5f;
            float scaledBorder = NodeBorderThickness * _canvasZoom;

            Handles.BeginGUI();

            Handles.color = _data.EdgeColor;
            var edges = _data.GetEdges();
            foreach (var (fromId, toId) in edges)
            {
                if (fromId >= _data.Nodes.Count || toId >= _data.Nodes.Count) continue;
                Vector2 fromPos = origin + _data.Nodes[fromId].position * scaledUnit;
                Vector2 toPos = origin + _data.Nodes[toId].position * scaledUnit;
                Handles.DrawLine(new Vector3(fromPos.x, fromPos.y, 0f), new Vector3(toPos.x, toPos.y, 0f));
            }

            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                var entry = _data.Nodes[i];
                Vector2 screenPos = origin + entry.position * scaledUnit;
                Vector3 center3D = new Vector3(screenPos.x, screenPos.y, 0f);
                Rect nodeRect = new Rect(screenPos.x - halfNode, screenPos.y - halfNode, scaledNodeSize, scaledNodeSize);

                Handles.color = (i == _selectedNodeIndex) ? _data.BorderSelectedColor : _data.BorderNormalColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode + scaledBorder);

                Handles.color = _data.NodeColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode);

                string label = _nodeLabels != null && i < _nodeLabels.Length ? _nodeLabels[i] : entry.id.ToString();
                GUI.Label(nodeRect, label, _nodeLabelStyle);
            }
            if (_branchPreviewActive && _branchPreviewParentIndex >= 0 && _branchPreviewParentIndex < _data.Nodes.Count)
            {
                Vector2 parentPos = _data.Nodes[_branchPreviewParentIndex].position;
                Vector2 previewPos = BranchPlacement.ComputeBranchPosition(parentPos, _branchPreviewSettings.distance, _branchPreviewSettings.angleDegrees);
                Vector2 parentScreen = origin + parentPos * scaledUnit;
                Vector2 previewScreen = origin + previewPos * scaledUnit;
                Handles.color = BranchPreviewTintColor;
                Handles.DrawDottedLine(
                    new Vector3(parentScreen.x, parentScreen.y, 0f),
                    new Vector3(previewScreen.x, previewScreen.y, 0f),
                    BranchPreviewDottedSegmentSize);
                Handles.DrawWireDisc(new Vector3(previewScreen.x, previewScreen.y, 0f), Vector3.forward, halfNode);
            }
            Handles.EndGUI();

            GUI.EndClip();
        }

        private void DrawGrid(Rect canvasRect, Vector2 origin)
        {
            float spacing = GridSpacing * _canvasZoom;

            if (spacing < MinGridSpacingThreshold) return;

            float startX = origin.x % spacing;
            for (float x = startX; x < canvasRect.width; x += spacing)
                EditorGUI.DrawRect(new Rect(x, 0, 1, canvasRect.height), GridLineColor);

            float startY = origin.y % spacing;
            for (float y = startY; y < canvasRect.height; y += spacing)
                EditorGUI.DrawRect(new Rect(0, y, canvasRect.width, 1), GridLineColor);
        }

        private void HandleCanvasInput(Rect canvasRect)
        {
            Event evt = Event.current;
            if (!canvasRect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.ScrollWheel)
            {
                float zoomDelta = 1f - evt.delta.y * ZoomScrollSensitivity;
                _canvasZoom = Mathf.Clamp(_canvasZoom * zoomDelta, MinZoom, MaxZoom);
                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDown && evt.button == LeftMouseButton && !evt.alt)
            {
                Vector2 mouseInCanvas = evt.mousePosition;
                Vector2 center = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
                Vector2 origin = center + _canvasOffset;

                int hitIndex = HitTestNode(mouseInCanvas, origin, _data.Nodes, _data.UnitSize, _data.NodeSize, _canvasZoom);
                _selectedNodeIndex = hitIndex;
                if (!_branchPreviewActive)
                    _activeTab = hitIndex >= 0 ? 1 : 0;
                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDrag && (evt.button == MiddleMouseButton || (evt.button == LeftMouseButton && evt.alt)))
            {
                _canvasOffset += evt.delta;
                evt.Use();
                Repaint();
            }
        }

        private void DrawConfigPanel()
        {
            _serializedData.Update();

            string[] tabLabels = _branchPreviewActive
                ? new[] { "Skill Tree", "Node", "Branch" }
                : new[] { "Skill Tree", "Node" };
            _activeTab = GUILayout.Toolbar(_activeTab, tabLabels);

            _configScrollPos = EditorGUILayout.BeginScrollView(_configScrollPos);

            if (_activeTab == 0)
                DrawSkillTreeTab();
            else if (_activeTab == 1)
                DrawNodeTab();
            else
                DrawBranchTab();

            if (_serializedData.ApplyModifiedProperties())
                Repaint();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillTreeTab()
        {
            EditorGUILayout.Space(8);

            EditorGUI.BeginChangeCheck();
            var newData = (SkillTreeData)EditorGUILayout.ObjectField("Data Asset", _data, typeof(SkillTreeData), false);
            if (EditorGUI.EndChangeCheck() && newData != null)
            {
                _data = newData;
                _serializedData = new SerializedObject(_data);
                CacheSerializedProperties();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propUnitSize, LabelUnitSize);
            EditorGUILayout.PropertyField(_propNodeSize, LabelNodeSize);
            EditorGUILayout.PropertyField(_propNodeColor, LabelNodeColor);
            EditorGUILayout.PropertyField(_propBorderNormalColor, LabelBorderNormal);
            EditorGUILayout.PropertyField(_propBorderSelectedColor, LabelBorderSelected);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Cost Defaults (applied on Generate)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("cost(0) = base, cost(n) = floor(prev \u00d7 mult) + add\nmult alternates: odd/even", MessageType.None);
            EditorGUILayout.PropertyField(_propBaseCost, LabelBaseCost);
            EditorGUILayout.PropertyField(_propCostMultiplierOdd, LabelCostMultiplierOdd);
            EditorGUILayout.PropertyField(_propCostMultiplierEven, LabelCostMultiplierEven);
            EditorGUILayout.PropertyField(_propCostAdditivePerLevel, LabelCostAdditive);
            EditorGUILayout.PropertyField(_propDefaultGeneratedMaxLevel, LabelDefaultGeneratedMaxLevel);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Edge Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propEdgeColor, LabelEdgeColor);
            EditorGUILayout.PropertyField(_propEdgeThickness, LabelEdgeThickness);

            EditorGUILayout.Space(16);

            EditorGUILayout.HelpBox(
                $"{_data.Nodes.Count} node(s). Select a node and use the Branch tab to add new nodes.",
                MessageType.Info);
        }

        private void DrawNodeTab()
        {
            EditorGUILayout.Space(8);

            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= _data.Nodes.Count)
            {
                EditorGUILayout.HelpBox("Select a node on the canvas to edit its properties.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Node {_selectedNodeIndex}", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Branch from Selected"))
            {
                _branchPreviewPreviousTab = _activeTab;
                _branchPreviewActive = true;
                _branchPreviewParentIndex = _selectedNodeIndex;
                _branchPreviewSettings = _lastBranchPreviewSettings;
                _branchPreviewSettings.angleDegrees = BranchPlacement.ComputeDefaultAngle(_data.Nodes[_selectedNodeIndex].position);
                _activeTab = 2;
                Repaint();
            }

            EditorGUILayout.Space(8);

            var node = _data.Nodes[_selectedNodeIndex];

            EditorGUI.BeginChangeCheck();

            var newCostType = (SkillTreeData.CostType)EditorGUILayout.EnumPopup("Cost Type", node.costType);
            int newMaxLevel = EditorGUILayout.IntField("Max Level (0=unlimited)", node.maxLevel);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Cost Formula", EditorStyles.boldLabel);
            int newBaseCost = EditorGUILayout.IntField("Base Cost", node.baseCost);
            float newMultOdd = EditorGUILayout.FloatField("Multiplier (Odd)", node.costMultiplierOdd);
            float newMultEven = EditorGUILayout.FloatField("Multiplier (Even)", node.costMultiplierEven);
            int newAdditive = EditorGUILayout.IntField("Additive / Level", node.costAdditivePerLevel);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Stat Modifier", EditorStyles.boldLabel);
            var newStatModType = (StatType)EditorGUILayout.EnumPopup("Stat", node.statModifierType);
            var newStatModMode = (SkillTreeData.StatModifierMode)EditorGUILayout.EnumPopup("Mode", node.statModifierMode);
            string statModValueLabel = newStatModMode == SkillTreeData.StatModifierMode.Percent
                ? "% / Level"
                : "Value / Level";
            float newStatModValue = EditorGUILayout.FloatField(statModValueLabel, node.statModifierValuePerLevel);
            if (newStatModMode == SkillTreeData.StatModifierMode.Percent)
                EditorGUILayout.HelpBox("Stored as percent (5 = +5%)", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                var updated = node;
                updated.costType = newCostType;
                updated.maxLevel = newMaxLevel;
                updated.baseCost = newBaseCost;
                updated.costMultiplierOdd = newMultOdd;
                updated.costMultiplierEven = newMultEven;
                updated.costAdditivePerLevel = newAdditive;
                updated.statModifierType = newStatModType;
                updated.statModifierMode = newStatModMode;
                updated.statModifierValuePerLevel = newStatModValue;

                _data.SetNode(_selectedNodeIndex, updated);
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Cost Preview", EditorStyles.boldLabel);
            int previewLevels = node.maxLevel > 0 ? Mathf.Min(node.maxLevel, CostPreviewMaxRows) : CostPreviewMaxRows;
            for (int lvl = 0; lvl < previewLevels; lvl++)
            {
                int cost = SkillTreeData.ComputeNodeCost(node, lvl);
                EditorGUILayout.LabelField($"  Level {lvl} \u2192 {lvl + 1}", $"{cost} {node.costType}");
            }
            if (node.maxLevel == 0)
                EditorGUILayout.LabelField("  ...", "(unlimited)");

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);

            bool isRoot = node.id == SkillTreeData.CentralNodeId;
            EditorGUI.BeginDisabledGroup(isRoot);
            if (GUILayout.Button("Delete Node"))
            {
                Undo.RegisterCompleteObjectUndo(_data, "Delete Node");
                _data.RemoveNode(node.id);
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
                _selectedNodeIndex = -1;
                _activeTab = 0;
                RebuildNodeLabels();
                _serializedData.Update();
                Repaint();
            }
            EditorGUI.EndDisabledGroup();

            if (isRoot)
                EditorGUILayout.HelpBox("The root node cannot be deleted — it's the unlock seed.", MessageType.Info);

        }

        private void DrawBranchTab()
        {
            EditorGUILayout.Space(8);

            if (!_branchPreviewActive || _branchPreviewParentIndex < 0 || _branchPreviewParentIndex >= _data.Nodes.Count)
            {
                EditorGUILayout.HelpBox("Branch preview not active.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Branch from Node {_branchPreviewParentIndex}", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _branchPreviewSettings.distance = EditorGUILayout.Slider("Distance", _branchPreviewSettings.distance, MinBranchPreviewDistance, MaxBranchPreviewDistance);
            if (EditorGUI.EndChangeCheck())
                Repaint();

            EditorGUI.BeginChangeCheck();
            _branchPreviewSettings.angleDegrees = EditorGUILayout.Slider("Angle (deg, 0=N, 90=E)", _branchPreviewSettings.angleDegrees, MinBranchAngle, MaxBranchAngle);
            if (EditorGUI.EndChangeCheck())
                Repaint();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
                ExecuteGenerateBranch();
            if (GUILayout.Button("Cancel"))
                CancelBranchPreview();
            EditorGUILayout.EndHorizontal();
        }

        private void CancelBranchPreview()
        {
            _branchPreviewActive = false;
            _branchPreviewParentIndex = -1;
            _activeTab = _branchPreviewPreviousTab;
            Repaint();
        }

        private void ExecuteGenerateBranch()
        {
            int parentIndex = _branchPreviewParentIndex;
            if (parentIndex < 0 || parentIndex >= _data.Nodes.Count) return;

            int parentId = _data.Nodes[parentIndex].id;
            Vector2 newPos = BranchPlacement.ComputeBranchPosition(_data.Nodes[parentIndex].position, _branchPreviewSettings.distance, _branchPreviewSettings.angleDegrees);
            int newId = SkillTreeNodeIdAllocator.ComputeNextNodeId(_data.Nodes);

            Undo.RegisterCompleteObjectUndo(_data, "Create Branch Node");
            var newEntry = SkillTreeNodeFactory.CreateBranchNode(newId, newPos);
            _data.AddBranchNode(newEntry, parentId);
            EditorUtility.SetDirty(_data);
            AssetDatabase.SaveAssets();
            _serializedData.Update();
            RebuildNodeLabels();

            _lastBranchPreviewSettings = _branchPreviewSettings;
            _selectedNodeIndex = _data.Nodes.Count - 1;
            _branchPreviewActive = false;
            _branchPreviewParentIndex = -1;
            _activeTab = 1;
            Repaint();
        }

        internal static int HitTestNode(Vector2 mousePos, Vector2 origin, IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes, float unitSize, float nodeSize, float zoom)
        {
            float scaledUnit = unitSize * zoom;
            float halfNode = nodeSize * zoom * 0.5f;
            float halfNodeSq = halfNode * halfNode;

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                Vector2 nodeScreenPos = origin + nodes[i].position * scaledUnit;
                float distSq = (mousePos - nodeScreenPos).sqrMagnitude;
                if (distSq <= halfNodeSq)
                    return i;
            }
            return -1;
        }

        private void RebuildNodeLabels()
        {
            if (_data == null || _data.Nodes.Count == 0)
            {
                _nodeLabels = null;
                return;
            }

            _nodeLabels = new string[_data.Nodes.Count];
            for (int i = 0; i < _data.Nodes.Count; i++)
                _nodeLabels[i] = _data.Nodes[i].id.ToString();
        }
    }
}
