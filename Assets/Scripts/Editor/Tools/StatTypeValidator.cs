using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Editor.Tools
{
    public static class StatTypeValidator
    {
        public readonly struct ValidationReport
        {
            public readonly int ScannedAssets;
            public readonly int ScannedNodes;
            public readonly List<string> Issues;

            public ValidationReport(int scannedAssets, int scannedNodes, List<string> issues)
            {
                ScannedAssets = scannedAssets;
                ScannedNodes = scannedNodes;
                Issues = issues;
            }
        }

        [MenuItem("Tools/Roguelite/Validate StatType Indices")]
        public static void Validate()
        {
            var assets = LoadAllSkillTreeAssets();
            var report = Run(assets);
            LogReport(report);
        }

        public static ValidationReport Run(IEnumerable<SkillTreeData> assets)
        {
            var issues = new List<string>();

            int scannedAssets = 0;
            int scannedNodes = 0;

            foreach (var asset in assets)
            {
                scannedAssets++;
                foreach (var node in asset.Nodes)
                {
                    scannedNodes++;
                    if (!Enum.IsDefined(typeof(StatType), node.statModifierType))
                        issues.Add($"Node {node.id} in {asset.name} has invalid statModifierType={(int)node.statModifierType}");
                    if (!Enum.IsDefined(typeof(SkillTreeData.StatModifierMode), node.statModifierMode))
                        issues.Add($"Node {node.id} in {asset.name} has invalid statModifierMode={(int)node.statModifierMode}");
                }
            }

            return new ValidationReport(scannedAssets, scannedNodes, issues);
        }

        private static List<SkillTreeData> LoadAllSkillTreeAssets()
        {
            var guids = AssetDatabase.FindAssets("t:SkillTreeData");
            var list = new List<SkillTreeData>(guids.Length);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(path);
                if (asset != null) list.Add(asset);
            }
            return list;
        }

        private static void LogReport(ValidationReport report)
        {
            if (report.Issues.Count == 0)
            {
                Debug.Log($"[StatTypeValidator] OK — scanned {report.ScannedAssets} assets, {report.ScannedNodes} nodes, 0 issues");
                return;
            }

            foreach (var issue in report.Issues)
                Debug.LogWarning(issue);
            Debug.LogError($"[StatTypeValidator] {report.Issues.Count} issue(s) found across {report.ScannedAssets} assets, {report.ScannedNodes} nodes");
        }
    }
}
