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

            if ((int)StatType.Hp != 0) issues.Add($"StatType.Hp expected index 0, got {(int)StatType.Hp}");
            if ((int)StatType.RegenHp != 1) issues.Add($"StatType.RegenHp expected index 1, got {(int)StatType.RegenHp}");
            if ((int)StatType.Atk != 2) issues.Add($"StatType.Atk expected index 2, got {(int)StatType.Atk}");
            if ((int)StatType.Def != 3) issues.Add($"StatType.Def expected index 3, got {(int)StatType.Def}");
            if ((int)StatType.Mana != 4) issues.Add($"StatType.Mana expected index 4, got {(int)StatType.Mana}");
            if ((int)StatType.Power != 5) issues.Add($"StatType.Power expected index 5, got {(int)StatType.Power}");
            if ((int)StatType.AttackSpeed != 6) issues.Add($"StatType.AttackSpeed expected index 6, got {(int)StatType.AttackSpeed}");
            if ((int)StatType.CritRate != 7) issues.Add($"StatType.CritRate expected index 7, got {(int)StatType.CritRate}");

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
