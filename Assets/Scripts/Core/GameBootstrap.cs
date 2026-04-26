using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.Services;
using RogueliteAutoBattler.Services.Local;
using RogueliteAutoBattler.UI.Toolkit;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;

namespace RogueliteAutoBattler.Core
{
    public static class GameBootstrap
    {
        internal const string CombatWorldName = "CombatWorld";

        public static Transform CombatWorld { get; private set; }
        public static TeamRoster TeamRoster { get; private set; }
        public static NavigationHost NavigationHost { get; private set; }
        public static Camera MainCamera { get; private set; }
        internal static GoldWallet GoldWallet { get; private set; }
        internal static IPlayerProgressionLoader ProgressionLoader { get; private set; }
        internal static AllyStatBonusService AllyStatBonusService { get; private set; }

        internal static SkillTreeData SkillTreeDataAssetForTest;
        internal static SkillTreeProgress SkillTreeProgressAssetForTest;
        internal static GoldWallet GoldWalletForTest;
        internal static string ProgressionFilePathForTest;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void Initialize()
        {
            MainCamera = Camera.main;
            NavigationHost = Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);

            var combatWorldGo = GameObject.Find(CombatWorldName);
            CombatWorld = combatWorldGo != null ? combatWorldGo.transform : null;
            TeamRoster = combatWorldGo != null ? combatWorldGo.GetComponent<TeamRoster>() : null;

            GoldWallet = GoldWalletForTest != null
                ? GoldWalletForTest
                : Object.FindFirstObjectByType<GoldWallet>(FindObjectsInactive.Include);

            SkillTreeData skillTreeData = SkillTreeDataAssetForTest;
            SkillTreeProgress skillTreeProgress = SkillTreeProgressAssetForTest;
            if (skillTreeData == null || skillTreeProgress == null)
            {
                var skillTreeController = Object.FindFirstObjectByType<SkillTreeScreenController>(FindObjectsInactive.Include);
                if (skillTreeController != null)
                {
                    if (skillTreeData == null) skillTreeData = skillTreeController.Data;
                    if (skillTreeProgress == null) skillTreeProgress = skillTreeController.Progress;
                }
            }

            if (GoldWallet != null && skillTreeProgress != null)
            {
                ProgressionLoader = new LocalPlayerProgressionLoader(skillTreeProgress, GoldWallet, ProgressionFilePathForTest);
                ProgressionLoader.Load();
            }

            if (TeamRoster != null && skillTreeData != null && skillTreeProgress != null)
            {
                AllyStatBonusService = new AllyStatBonusService(TeamRoster, skillTreeData, skillTreeProgress);
            }

            ConfigurePhysicsLayers();

            var selectionGo = new GameObject("UnitSelectionManager");
            selectionGo.AddComponent<UnitSelectionManager>();

            ValidateRefs();
        }

        private static void ConfigurePhysicsLayers()
        {
            Physics2D.IgnoreLayerCollision(PhysicsLayers.AllyLayer, PhysicsLayers.EnemyLayer, true);

            int selectionLayer = PhysicsLayers.SelectionLayer;
            if (selectionLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(selectionLayer, selectionLayer, true);
                Physics2D.IgnoreLayerCollision(selectionLayer, PhysicsLayers.AllyLayer, true);
                Physics2D.IgnoreLayerCollision(selectionLayer, PhysicsLayers.EnemyLayer, true);
                Physics2D.IgnoreLayerCollision(selectionLayer, 0, true);
            }
        }

        private static void ValidateRefs()
        {
            if (CombatWorld == null)
                Debug.LogError("[GameBootstrap] CombatWorld not found in scene.");
            if (TeamRoster == null)
                Debug.LogError("[GameBootstrap] TeamRoster not found on CombatWorld root.");
            if (NavigationHost == null)
                Debug.LogError("[GameBootstrap] No navigation system found in scene (NavigationHost missing).");
            if (MainCamera == null)
                Debug.LogError("[GameBootstrap] Main Camera not found in scene.");
            if (GoldWallet == null)
                Debug.LogError("[GameBootstrap] GoldWallet not found in scene.");
            if (ProgressionLoader == null)
                Debug.LogError("[GameBootstrap] ProgressionLoader not initialized (missing GoldWallet or SkillTreeProgress).");
            if (AllyStatBonusService == null)
                Debug.LogError("[GameBootstrap] AllyStatBonusService not initialized (missing TeamRoster/SkillTreeData/SkillTreeProgress).");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            AllyStatBonusService?.Dispose();
            AllyStatBonusService = null;
            if (ProgressionLoader is System.IDisposable disposable) disposable.Dispose();
            ProgressionLoader = null;
            GoldWallet = null;
            SkillTreeDataAssetForTest = null;
            SkillTreeProgressAssetForTest = null;
            GoldWalletForTest = null;
            ProgressionFilePathForTest = null;

            CombatWorld = null;
            TeamRoster = null;
            NavigationHost = null;
            MainCamera = null;
        }

        internal static void ResetForTest() => ResetOnDomainReload();
    }
}
