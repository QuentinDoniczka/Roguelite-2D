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
        private const float MinBranchAngleDegrees = 0f;
        private const float MaxBranchAngleDegrees = 360f;
        private const float BranchPreviewDottedSegmentSize = 6f;
        private const int CostPreviewMaxRows = 10;
        private const float MinWindowWidth = 800f;
        private const float MinWindowHeight = 500f;
        private const int LeftMouseButton = 0;
        private const int MiddleMouseButton = 2;
        private const int TabIndexSkillTree = 0;
        private const int TabIndexNode = 1;
        private const int TabIndexBranch = 2;
        private const float SectionSpacingSmall = 8f;
        private const float SectionSpacingMedium = 12f;
        private const float SectionSpacingLarge = 16f;
        private const float CrosshairLineThickness = 1f;
        private const float CrosshairHalfPixelOffset = 0.5f;

        private static readonly Color CanvasBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color CrosshairColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color BranchPreviewTintColor = new Color(1f, 1f, 1f, 0.4f);
        private static readonly Color MirrorAxisLineColor = new Color(1f, 0.92f, 0.016f, 0.6f);
        private static readonly Color MirrorPreviewTintColor = new Color(0f, 1f, 1f, 0.4f);

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

        private const string AngleSliderLabel = "Angle (deg, 0=N, 90=E)";
        private const float MinMirrorAxisDegrees = 0f;
        private const float MaxMirrorAxisDegrees = 360f;
        private const string MirrorEnabledLabel = "Mirror";
        private const string MirrorAxisSliderLabel = "Mirror Axis (deg, 0=vertical)";

        private const float MinSnapThresholdUnits = 0f;
        private const float MaxSnapThresholdUnits = 2f;
        private const float CoordLabelOffsetXPixels = 4f;
        private const float CoordLabelOffsetYPixels = 8f;
        private const float CoordLabelWidthPixels = 120f;
        private const float CoordLabelHeightPixels = 16f;
        private static readonly Color SnapGuideLineColor = new Color(0.4f, 0.85f, 1f, 0.7f);
        private const string SnapEnabledLabel = "Snap to Nearby Node";
        private const string SnapThresholdLabel = "Snap Threshold (units)";

        private const string SelectedAssetGuidEditorPrefKey = "SkillTreeDesigner.SelectedAssetGuid";
        private const string ActiveLabelPrefix = "Active: ";
        private const string NoActiveLabel = "<none>";

        private const string DefaultNewTreeFileName = "NewSkillTree";
        private const string AssetExtension = "asset";
        private const string DialogTitleCreateTree = "Create Skill Tree";
        private const string DialogTitleInvalidPath = "Invalid Path";
        private const string DialogTitleDeleteTree = "Delete Skill Tree";
        private const string DialogConfirmOk = "OK";
        private const string DialogConfirmDelete = "Delete";
        private const string DialogConfirmCancel = "Cancel";
        private const string ButtonLabelSetActive = "Set as Active";
        private const string ButtonLabelNew = "New";
        private const string ButtonLabelDuplicate = "Duplicate";
        private const string ButtonLabelDelete = "Delete";
        private const string DisabledTooltipNoSelection = "No skill tree selected.";
        private const string DisabledTooltipAlreadyActive = "This tree is already active.";
        private const string DisabledTooltipCannotDeleteActive = "Cannot delete the active skill tree. Switch to another tree first.";
        private const string DisabledTooltipCannotDeleteLast = "Cannot delete the last skill tree.";
        private const string DisabledTooltipDeleteEnabled = "Delete this skill tree.";
        private const int MinimumRetainedTreeCount = 1;

        private static readonly string[] TabLabelsWithoutBranch = { "Skill Tree", "Node" };
        private static readonly string[] TabLabelsWithBranch = { "Skill Tree", "Node", "Branch" };

        private IReadOnlyList<SkillTreesEnumerator.TreeEntry> _treeEntries;
        private string[] _treeDisplayNames;
        private int _selectedTreeIndex = -1;
        private SkillTreeData _activePointerTarget;

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
        private NodeDragController.DragState _dragState = NodeDragController.DragState.Inactive;
        private NodeSnapEngine.SnapResult _lastSnapResult;
        private const float DragStartThresholdPx = 4f;
        private bool _pendingDragArm;
        private Vector2 _pendingDragMousePx;
        private int _pendingDragNodeIndex = -1;
        private int _pendingDragMirrorPartnerIndex = -1;
        private Vector2 _pendingDragMirrorPartnerStartPositionUnits;
        private static GUIStyle _coordLabelStyle;
        private int _activeTab;
        private bool _branchPreviewActive;
        private int _branchPreviewParentIndex = -1;
        private int _branchPreviewPreviousTab;
        private BranchPreviewSettings _branchPreviewSettings = BranchPreviewSettings.Defaults;
        private BranchPreviewSettings _lastBranchPreviewSettings = BranchPreviewSettings.Defaults;
        private string _lastMirrorWarning;
        private bool _connectionsFoldoutOpen = true;

        private static void LogError(string message)
        {
            Debug.LogError($"[{nameof(SkillTreeDesignerWindow)}] {message}");
        }

        private static void DrawToolbarButton(string label, bool enabled, string disabledTooltip, System.Action onClick)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                var content = enabled ? new GUIContent(label) : new GUIContent(label, disabledTooltip);
                if (GUILayout.Button(content))
                    onClick();
            }
        }

        [MenuItem("Roguelite/Skill Tree Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<SkillTreeDesignerWindow>("Skill Tree Designer");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshTreeList();
            var initialEntry = ResolveInitialTreeEntry();
            BindAsset(initialEntry);
            RefreshActivePointerCache();
            MirrorAxisPersistence.ApplyTo(ref _branchPreviewSettings);
            MirrorAxisPersistence.ApplyTo(ref _lastBranchPreviewSettings);
            SnapSettingsPersistence.ApplyTo(ref _branchPreviewSettings);
            SnapSettingsPersistence.ApplyTo(ref _lastBranchPreviewSettings);
        }

        private void RefreshTreeList()
        {
            _treeEntries = SkillTreesEnumerator.Enumerate(EditorPaths.SkillTreesFolder);
            _treeDisplayNames = new string[_treeEntries.Count];
            for (int i = 0; i < _treeEntries.Count; i++)
                _treeDisplayNames[i] = _treeEntries[i].DisplayName;
        }

        private SkillTreesEnumerator.TreeEntry? ResolveInitialTreeEntry()
        {
            if (_treeEntries == null || _treeEntries.Count == 0) return null;

            var savedGuid = EditorPrefs.GetString(SelectedAssetGuidEditorPrefKey, string.Empty);
            if (!string.IsNullOrEmpty(savedGuid))
            {
                var idx = IndexOfGuid(savedGuid);
                if (idx >= 0) return _treeEntries[idx];
            }

            var active = ActiveSkillTreeResolver.GetActive();
            if (active != null)
            {
                var activePath = AssetDatabase.GetAssetPath(active);
                var activeGuid = AssetDatabase.AssetPathToGUID(activePath);
                var idx = IndexOfGuid(activeGuid);
                if (idx >= 0) return _treeEntries[idx];
            }

            return _treeEntries[0];
        }

        private int IndexOfGuid(string guid)
        {
            for (int i = 0; i < _treeEntries.Count; i++)
                if (_treeEntries[i].Guid == guid) return i;
            return -1;
        }

        private void BindAsset(SkillTreesEnumerator.TreeEntry? entry)
        {
            _lastMirrorWarning = null;
            if (!entry.HasValue)
            {
                _data = null;
                _serializedData = null;
                _selectedTreeIndex = -1;
                _nodeLabels = null;
                return;
            }
            var resolvedEntry = entry.Value;
            _selectedTreeIndex = IndexOfGuid(resolvedEntry.Guid);
            _data = resolvedEntry.Asset;
            _serializedData = new SerializedObject(_data);
            CacheSerializedProperties();
            RebuildNodeLabels();
            EditorPrefs.SetString(SelectedAssetGuidEditorPrefKey, resolvedEntry.Guid);
        }

        private void RefreshActivePointerCache()
        {
            var pointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(EditorPaths.ActiveSkillTreePointerAsset);
            _activePointerTarget = pointer != null ? pointer.Target : null;
        }

        private void SetActivePointerToCurrent()
        {
            if (_data == null) return;
            if (!SkillTreesEnumerator.SetActivePointer(EditorPaths.ActiveSkillTreePointerAsset, _data))
            {
                LogError($"Active pointer asset missing at {EditorPaths.ActiveSkillTreePointerAsset}");
                return;
            }
            RefreshActivePointerCache();
        }

        private void CreateNewTree()
        {
            var path = EditorUtility.SaveFilePanel(DialogTitleCreateTree, EditorPaths.SkillTreesFolder, DefaultNewTreeFileName, AssetExtension);
            if (string.IsNullOrEmpty(path)) return;
            if (!SkillTreesEnumerator.IsPathUnderSkillTreesFolder(path, EditorPaths.SkillTreesFolder))
            {
                EditorUtility.DisplayDialog(DialogTitleInvalidPath, $"Skill trees must live under {EditorPaths.SkillTreesFolder}.", DialogConfirmOk);
                return;
            }
            var assetRelative = SkillTreesEnumerator.ConvertAbsoluteToAssetRelative(path);
            var asset = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(asset, assetRelative);
            AssetDatabase.SaveAssets();
            var guid = AssetDatabase.AssetPathToGUID(assetRelative);
            RefreshTreeList();
            BindAsset(FindEntryByGuid(guid));
        }

        private void DuplicateCurrentTree()
        {
            if (_data == null) return;
            var sourcePath = AssetDatabase.GetAssetPath(_data);
            var duplicatePath = SkillTreesEnumerator.MakeUniqueDuplicatePath(sourcePath);
            if (string.IsNullOrEmpty(duplicatePath)) return;
            if (!AssetDatabase.CopyAsset(sourcePath, duplicatePath))
            {
                LogError($"Failed to duplicate {sourcePath} to {duplicatePath}");
                return;
            }
            AssetDatabase.SaveAssets();
            var guid = AssetDatabase.AssetPathToGUID(duplicatePath);
            RefreshTreeList();
            BindAsset(FindEntryByGuid(guid));
        }

        private void DeleteCurrentTree()
        {
            if (_data == null) return;
            if (_treeEntries.Count <= MinimumRetainedTreeCount) return;
            if (_data == _activePointerTarget) return;
            var assetName = _data.name;
            if (!EditorUtility.DisplayDialog(DialogTitleDeleteTree, $"Delete '{assetName}'? This cannot be undone.", DialogConfirmDelete, DialogConfirmCancel))
                return;
            var path = AssetDatabase.GetAssetPath(_data);
            if (!AssetDatabase.DeleteAsset(path))
            {
                LogError($"Failed to delete {path}");
                return;
            }
            AssetDatabase.SaveAssets();
            RefreshTreeList();
            BindAsset(_treeEntries.Count > 0 ? _treeEntries[0] : (SkillTreesEnumerator.TreeEntry?)null);
        }

        private SkillTreesEnumerator.TreeEntry? FindEntryByGuid(string guid)
        {
            var idx = IndexOfGuid(guid);
            return idx >= 0 ? _treeEntries[idx] : (SkillTreesEnumerator.TreeEntry?)null;
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

            EditorGUI.DrawRect(new Rect(origin.x - CrosshairHalfPixelOffset, 0, CrosshairLineThickness, canvasRect.height), CrosshairColor);
            EditorGUI.DrawRect(new Rect(0, origin.y - CrosshairHalfPixelOffset, canvasRect.width, CrosshairLineThickness), CrosshairColor);

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

                if (_dragState.IsActive && i == _dragState.NodeIndex)
                {
                    if (_coordLabelStyle == null)
                    {
                        _coordLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            normal = { textColor = Color.white }
                        };
                    }
                    string coordText = $"({entry.position.x:F2}, {entry.position.y:F2})";
                    Rect coordRect = new Rect(
                        screenPos.x + halfNode + CoordLabelOffsetXPixels,
                        screenPos.y - CoordLabelOffsetYPixels,
                        CoordLabelWidthPixels,
                        CoordLabelHeightPixels);
                    GUI.Label(coordRect, coordText, _coordLabelStyle);
                }
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

                if (_branchPreviewSettings.mirrorEnabled)
                {
                    float halfSpan = Mathf.Max(canvasRect.width, canvasRect.height);
                    MirrorAxisGeometry.ComputeAxisEndpoints(origin, _branchPreviewSettings.mirrorAxisDegrees, halfSpan, out var axisStart, out var axisEnd);
                    Handles.color = MirrorAxisLineColor;
                    Handles.DrawDottedLine(new Vector3(axisStart.x, axisStart.y, 0f), new Vector3(axisEnd.x, axisEnd.y, 0f), BranchPreviewDottedSegmentSize);

                    float mirrorAngleDegrees = BranchPlacement.MirrorAngle(_branchPreviewSettings.angleDegrees, _branchPreviewSettings.mirrorAxisDegrees);
                    Vector2 mirrorPos = BranchPlacement.ComputeBranchPosition(parentPos, _branchPreviewSettings.distance, mirrorAngleDegrees);
                    Vector2 mirrorScreen = origin + mirrorPos * scaledUnit;
                    Handles.color = MirrorPreviewTintColor;
                    Handles.DrawDottedLine(
                        new Vector3(parentScreen.x, parentScreen.y, 0f),
                        new Vector3(mirrorScreen.x, mirrorScreen.y, 0f),
                        BranchPreviewDottedSegmentSize);
                    Handles.DrawWireDisc(new Vector3(mirrorScreen.x, mirrorScreen.y, 0f), Vector3.forward, halfNode);
                }
            }
            if (_dragState.IsActive && _lastSnapResult.SnappedAxis != NodeSnapEngine.SnapAxis.None)
            {
                Color prevColor = Handles.color;
                Handles.color = SnapGuideLineColor;
                if (_lastSnapResult.SnappedAxis == NodeSnapEngine.SnapAxis.X)
                {
                    float snapScreenX = origin.x + _data.Nodes[_lastSnapResult.TargetNodeIndex].position.x * scaledUnit;
                    Handles.DrawDottedLine(
                        new Vector3(snapScreenX, 0f, 0f),
                        new Vector3(snapScreenX, canvasRect.height, 0f),
                        BranchPreviewDottedSegmentSize);
                }
                else
                {
                    float snapScreenY = origin.y + _data.Nodes[_lastSnapResult.TargetNodeIndex].position.y * scaledUnit;
                    Handles.DrawDottedLine(
                        new Vector3(0f, snapScreenY, 0f),
                        new Vector3(canvasRect.width, snapScreenY, 0f),
                        BranchPreviewDottedSegmentSize);
                }
                Handles.color = prevColor;
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
                    _activeTab = hitIndex >= 0 ? TabIndexNode : TabIndexSkillTree;

                if (hitIndex >= 0 && _data.Nodes[hitIndex].id != SkillTreeData.CentralNodeId)
                {
                    _pendingDragArm = true;
                    _pendingDragMousePx = evt.mousePosition;
                    _pendingDragNodeIndex = hitIndex;
                    if (_branchPreviewSettings.mirrorEnabled)
                    {
                        _pendingDragMirrorPartnerIndex = MirrorPartnerFinder.FindPartnerIndex(
                            _data.Nodes, hitIndex, _branchPreviewSettings.mirrorAxisDegrees,
                            MirrorPartnerFinder.DefaultMatchToleranceUnits);
                        _pendingDragMirrorPartnerStartPositionUnits = _pendingDragMirrorPartnerIndex >= 0
                            ? _data.Nodes[_pendingDragMirrorPartnerIndex].position
                            : Vector2.zero;
                    }
                    else
                    {
                        _pendingDragMirrorPartnerIndex = -1;
                        _pendingDragMirrorPartnerStartPositionUnits = Vector2.zero;
                    }
                }

                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDrag && evt.button == LeftMouseButton && !evt.alt)
            {
                if (_pendingDragArm && (evt.mousePosition - _pendingDragMousePx).magnitude >= DragStartThresholdPx)
                {
                    Undo.RegisterCompleteObjectUndo(_data, "Move Node");
                    var startNode = _data.Nodes[_pendingDragNodeIndex];
                    _dragState = new NodeDragController.DragState(
                        _pendingDragNodeIndex,
                        startNode.position,
                        _pendingDragMousePx,
                        _pendingDragMirrorPartnerIndex,
                        _pendingDragMirrorPartnerStartPositionUnits);
                    _pendingDragArm = false;
                }

                if (_dragState.IsActive)
                {
                    Vector2 rawNewPos = NodeDragController.ComputeNewNodePosition(
                        _dragState, evt.mousePosition, _data.UnitSize, _canvasZoom);
                    bool snapAllowed = _branchPreviewSettings.snapEnabled && !evt.shift;
                    _lastSnapResult = snapAllowed
                        ? NodeSnapEngine.Resolve(rawNewPos, _dragState.NodeIndex, _data.Nodes, _branchPreviewSettings.snapThresholdUnits)
                        : NodeSnapEngine.SnapResult.NoSnap(rawNewPos);

                    var updated = _data.Nodes[_dragState.NodeIndex];
                    updated.position = _lastSnapResult.ResolvedPosition;
                    _data.SetNode(_dragState.NodeIndex, updated);

                    if (_dragState.MirrorPartnerIndex >= 0 && _dragState.MirrorPartnerIndex < _data.Nodes.Count)
                    {
                        Vector2 delta = _lastSnapResult.ResolvedPosition - _dragState.NodeStartPositionUnits;
                        Vector2 mirroredDelta = MirrorAxisGeometry.ReflectAcrossAxisThroughOrigin(delta, _branchPreviewSettings.mirrorAxisDegrees);
                        Vector2 partnerNew = _dragState.MirrorPartnerStartPositionUnits + mirroredDelta;
                        var partner = _data.Nodes[_dragState.MirrorPartnerIndex];
                        partner.position = partnerNew;
                        _data.SetNode(_dragState.MirrorPartnerIndex, partner);
                    }

                    _serializedData?.Update();
                    Repaint();
                }

                evt.Use();
            }

            if (evt.type == EventType.MouseUp && evt.button == LeftMouseButton)
            {
                bool wasDragging = _dragState.IsActive;
                if (wasDragging)
                {
                    _dragState = NodeDragController.DragState.Inactive;
                    _lastSnapResult = NodeSnapEngine.SnapResult.NoSnap(Vector2.zero);
                    EditorUtility.SetDirty(_data);
                    AssetDatabase.SaveAssets();
                    Repaint();
                }
                _pendingDragArm = false;
                if (wasDragging)
                    evt.Use();
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

            string[] tabLabels = _branchPreviewActive ? TabLabelsWithBranch : TabLabelsWithoutBranch;
            _activeTab = GUILayout.Toolbar(_activeTab, tabLabels);

            _configScrollPos = EditorGUILayout.BeginScrollView(_configScrollPos);

            if (_activeTab == TabIndexSkillTree)
                DrawSkillTreeTab();
            else if (_activeTab == TabIndexNode)
                DrawNodeTab();
            else
                DrawBranchTab();

            if (_serializedData.ApplyModifiedProperties())
                Repaint();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillTreeTab()
        {
            EditorGUILayout.Space(SectionSpacingSmall);

            if (_treeEntries == null || _treeEntries.Count == 0)
            {
                EditorGUILayout.HelpBox($"No SkillTreeData assets found under {EditorPaths.SkillTreesFolder}.", MessageType.Warning);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Skill Tree", _selectedTreeIndex, _treeDisplayNames);
                if (EditorGUI.EndChangeCheck() && newIndex != _selectedTreeIndex && newIndex >= 0 && newIndex < _treeEntries.Count)
                    BindAsset(_treeEntries[newIndex]);

                string activeName = _activePointerTarget != null ? _activePointerTarget.name : NoActiveLabel;
                EditorGUILayout.LabelField(ActiveLabelPrefix + activeName);

                EditorGUILayout.BeginHorizontal();
                bool isActive = _data == _activePointerTarget;
                bool canSetActive = _data != null && !isActive;
                string setActiveTooltip = _data == null ? DisabledTooltipNoSelection : DisabledTooltipAlreadyActive;
                DrawToolbarButton(ButtonLabelSetActive, canSetActive, setActiveTooltip, SetActivePointerToCurrent);

                DrawToolbarButton(ButtonLabelNew, true, null, CreateNewTree);

                DrawToolbarButton(ButtonLabelDuplicate, _data != null, DisabledTooltipNoSelection, DuplicateCurrentTree);

                bool canDelete = _data != null && _treeEntries != null && _treeEntries.Count > MinimumRetainedTreeCount && !isActive;
                string deleteTooltip = canDelete
                    ? DisabledTooltipDeleteEnabled
                    : (_data == null ? DisabledTooltipNoSelection
                        : isActive ? DisabledTooltipCannotDeleteActive
                        : DisabledTooltipCannotDeleteLast);
                DrawToolbarButton(ButtonLabelDelete, canDelete, deleteTooltip, DeleteCurrentTree);
                EditorGUILayout.EndHorizontal();
            }

            if (_data == null) return;

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propUnitSize, LabelUnitSize);
            EditorGUILayout.PropertyField(_propNodeSize, LabelNodeSize);
            EditorGUILayout.PropertyField(_propNodeColor, LabelNodeColor);
            EditorGUILayout.PropertyField(_propBorderNormalColor, LabelBorderNormal);
            EditorGUILayout.PropertyField(_propBorderSelectedColor, LabelBorderSelected);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Cost Defaults (applied on Generate)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("cost(0) = base, cost(n) = floor(prev \u00d7 mult) + add\nmult alternates: odd/even", MessageType.None);
            EditorGUILayout.PropertyField(_propBaseCost, LabelBaseCost);
            EditorGUILayout.PropertyField(_propCostMultiplierOdd, LabelCostMultiplierOdd);
            EditorGUILayout.PropertyField(_propCostMultiplierEven, LabelCostMultiplierEven);
            EditorGUILayout.PropertyField(_propCostAdditivePerLevel, LabelCostAdditive);
            EditorGUILayout.PropertyField(_propDefaultGeneratedMaxLevel, LabelDefaultGeneratedMaxLevel);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Edge Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propEdgeColor, LabelEdgeColor);
            EditorGUILayout.PropertyField(_propEdgeThickness, LabelEdgeThickness);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Designer Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool newSnapEnabled = EditorGUILayout.Toggle(SnapEnabledLabel, _branchPreviewSettings.snapEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                _branchPreviewSettings.snapEnabled = newSnapEnabled;
                SnapSettingsPersistence.SaveEnabled(newSnapEnabled);
                Repaint();
            }

            using (new EditorGUI.DisabledScope(!_branchPreviewSettings.snapEnabled))
            {
                EditorGUI.BeginChangeCheck();
                float newThreshold = EditorGUILayout.Slider(SnapThresholdLabel,
                    _branchPreviewSettings.snapThresholdUnits, MinSnapThresholdUnits, MaxSnapThresholdUnits);
                if (EditorGUI.EndChangeCheck())
                {
                    _branchPreviewSettings.snapThresholdUnits = newThreshold;
                    SnapSettingsPersistence.SaveThreshold(newThreshold);
                    Repaint();
                }
            }

            EditorGUILayout.Space(SectionSpacingLarge);

            EditorGUILayout.HelpBox(
                $"{_data.Nodes.Count} node(s). Select a node and use the Branch tab to add new nodes.",
                MessageType.Info);
        }

        private void DrawNodeTab()
        {
            EditorGUILayout.Space(SectionSpacingSmall);

            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= _data.Nodes.Count)
            {
                EditorGUILayout.HelpBox("Select a node on the canvas to edit its properties.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Node {_selectedNodeIndex}", EditorStyles.boldLabel);

            var nodeForPos = _data.Nodes[_selectedNodeIndex];
            bool isRootNode = nodeForPos.id == SkillTreeData.CentralNodeId;
            EditorGUI.BeginDisabledGroup(isRootNode);
            EditorGUI.BeginChangeCheck();
            float newX = EditorGUILayout.FloatField("Position X", nodeForPos.position.x);
            float newY = EditorGUILayout.FloatField("Position Y", nodeForPos.position.y);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_data, "Edit Node Position");
                var updatedPos = nodeForPos;
                updatedPos.position = new Vector2(newX, newY);
                _data.SetNode(_selectedNodeIndex, updatedPos);
                EditorUtility.SetDirty(_data);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Create Branch from Selected"))
            {
                _branchPreviewPreviousTab = _activeTab;
                _branchPreviewActive = true;
                _branchPreviewParentIndex = _selectedNodeIndex;
                _branchPreviewSettings = _lastBranchPreviewSettings;
                _branchPreviewSettings.angleDegrees = BranchPlacement.ComputeDefaultAngle(_data.Nodes[_selectedNodeIndex].position);
                _lastMirrorWarning = null;
                _activeTab = TabIndexBranch;
                Repaint();
            }

            EditorGUILayout.Space(SectionSpacingSmall);

            var node = _data.Nodes[_selectedNodeIndex];

            EditorGUI.BeginChangeCheck();

            var newCostType = (SkillTreeData.CostType)EditorGUILayout.EnumPopup("Cost Type", node.costType);
            int newMaxLevel = EditorGUILayout.IntField("Max Level (0=unlimited)", node.maxLevel);

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Cost Formula", EditorStyles.boldLabel);
            int newBaseCost = EditorGUILayout.IntField("Base Cost", node.baseCost);
            float newMultOdd = EditorGUILayout.FloatField("Multiplier (Odd)", node.costMultiplierOdd);
            float newMultEven = EditorGUILayout.FloatField("Multiplier (Even)", node.costMultiplierEven);
            int newAdditive = EditorGUILayout.IntField("Additive / Level", node.costAdditivePerLevel);

            EditorGUILayout.Space(SectionSpacingSmall);
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

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Cost Preview", EditorStyles.boldLabel);
            int previewLevels = node.maxLevel > 0 ? Mathf.Min(node.maxLevel, CostPreviewMaxRows) : CostPreviewMaxRows;
            for (int lvl = 0; lvl < previewLevels; lvl++)
            {
                int cost = SkillTreeData.ComputeNodeCost(node, lvl);
                EditorGUILayout.LabelField($"  Level {lvl} \u2192 {lvl + 1}", $"{cost} {node.costType}");
            }
            if (node.maxLevel == 0)
                EditorGUILayout.LabelField("  ...", "(unlimited)");

            EditorGUILayout.Space(SectionSpacingSmall);
            var connections = NodeConnectionsInspector.CollectConnections(_data, _selectedNodeIndex);
            _connectionsFoldoutOpen = EditorGUILayout.Foldout(
                _connectionsFoldoutOpen,
                $"Connections ({connections.Count})",
                toggleOnLabelClick: true,
                EditorStyles.foldoutHeader);
            if (_connectionsFoldoutOpen)
            {
                if (connections.Count == 0)
                {
                    EditorGUILayout.LabelField("No connections", EditorStyles.miniLabel);
                }
                else
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        foreach (var row in connections)
                        {
                            string arrow = row.IsOutgoing ? "→" : "←";
                            EditorGUILayout.LabelField(
                                $"{arrow} Node {row.OtherNodeId}",
                                $"{row.DistanceUnits:0.00} units");
                        }
                    }
                }
            }

            EditorGUILayout.Space(SectionSpacingLarge);
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
                _activeTab = TabIndexSkillTree;
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
            EditorGUILayout.Space(SectionSpacingSmall);

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
            _branchPreviewSettings.angleDegrees = EditorGUILayout.Slider(AngleSliderLabel, _branchPreviewSettings.angleDegrees, MinBranchAngleDegrees, MaxBranchAngleDegrees);
            if (EditorGUI.EndChangeCheck())
                Repaint();

            EditorGUI.BeginChangeCheck();
            _branchPreviewSettings.mirrorEnabled = EditorGUILayout.Toggle(MirrorEnabledLabel, _branchPreviewSettings.mirrorEnabled);
            if (EditorGUI.EndChangeCheck())
                Repaint();

            if (_branchPreviewSettings.mirrorEnabled)
            {
                EditorGUI.BeginChangeCheck();
                _branchPreviewSettings.mirrorAxisDegrees = EditorGUILayout.Slider(MirrorAxisSliderLabel, _branchPreviewSettings.mirrorAxisDegrees, MinMirrorAxisDegrees, MaxMirrorAxisDegrees);
                if (EditorGUI.EndChangeCheck())
                {
                    MirrorAxisPersistence.Save(_branchPreviewSettings.mirrorAxisDegrees);
                    Repaint();
                }
            }

            if (!string.IsNullOrEmpty(_lastMirrorWarning))
            {
                EditorGUILayout.HelpBox(_lastMirrorWarning, MessageType.Warning);
            }

            EditorGUILayout.Space(SectionSpacingSmall);
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
            _lastMirrorWarning = null;
            Repaint();
        }

        private void ExecuteGenerateBranch()
        {
            int parentIndex = _branchPreviewParentIndex;
            if (parentIndex < 0 || parentIndex >= _data.Nodes.Count) return;

            string undoLabel = _branchPreviewSettings.mirrorEnabled
                ? MirrorPairGenerator.UndoLabelMirroredPair
                : MirrorPairGenerator.UndoLabelSingleNode;
            Undo.RegisterCompleteObjectUndo(_data, undoLabel);

            var result = MirrorPairGenerator.TryGenerate(_data, parentIndex, _branchPreviewSettings);

            EditorUtility.SetDirty(_data);
            AssetDatabase.SaveAssets();
            _serializedData.Update();
            RebuildNodeLabels();

            _lastMirrorWarning = result.WarningMessage;
            _lastBranchPreviewSettings = _branchPreviewSettings;
            _selectedNodeIndex = _data.Nodes.Count - 1;
            _branchPreviewActive = false;
            _branchPreviewParentIndex = -1;
            _activeTab = TabIndexNode;
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
