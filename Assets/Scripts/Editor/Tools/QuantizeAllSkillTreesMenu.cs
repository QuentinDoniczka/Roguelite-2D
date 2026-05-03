using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class QuantizeAllSkillTreesMenu
    {
        private const string MenuPath = "Tools/Skill Tree/Quantize All Positions";
        private const string LogTag = "[QuantizeAllSkillTrees]";

        [MenuItem(MenuPath)]
        public static void QuantizeAllPositions()
        {
            var entries = SkillTreesEnumerator.Enumerate(EditorPaths.SkillTreesFolder);
            int touchedNodes = 0;
            int touchedAssets = 0;

            foreach (var entry in entries)
            {
                var asset = entry.Asset;
                if (asset == null) continue;

                bool assetDirty = false;
                var nodes = asset.Nodes;

                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    var quantized = SkillTreeGrid.Quantize(node.position);

                    bool xDrifted = Mathf.Abs(node.position.x - quantized.x) > SkillTreeGrid.DriftEpsilon;
                    bool yDrifted = Mathf.Abs(node.position.y - quantized.y) > SkillTreeGrid.DriftEpsilon;

                    if (!xDrifted && !yDrifted) continue;

                    var corrected = node;
                    corrected.position = quantized;
                    asset.SetNode(i, corrected);
                    touchedNodes++;
                    assetDirty = true;
                }

                if (!assetDirty) continue;

                EditorUtility.SetDirty(asset);
                touchedAssets++;
            }

            if (touchedNodes > 0)
                AssetDatabase.SaveAssets();

            Debug.Log($"{LogTag} {touchedNodes} nodes quantized across {touchedAssets} asset(s). ({entries.Count} asset(s) scanned)");
        }
    }
}
