using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using ToolkitHost = RogueliteAutoBattler.UI.Toolkit.NavigationHost;

namespace RogueliteAutoBattler.Core
{
    public static class GameBootstrap
    {
        internal const string CombatWorldName = "CombatWorld";

        public static Canvas Canvas { get; private set; }
        public static Transform CombatWorld { get; private set; }
        public static NavigationManager NavigationManager { get; private set; }
        public static ToolkitHost NavigationHost { get; private set; }
        public static Camera MainCamera { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void Initialize()
        {
            MainCamera = Camera.main;
            Canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            NavigationManager = Object.FindFirstObjectByType<NavigationManager>(FindObjectsInactive.Include);
            NavigationHost = Object.FindFirstObjectByType<ToolkitHost>(FindObjectsInactive.Include);

            var combatWorldGo = GameObject.Find(CombatWorldName);
            CombatWorld = combatWorldGo != null ? combatWorldGo.transform : null;

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
            if (NavigationManager == null && NavigationHost == null)
                Debug.LogError("[GameBootstrap] No navigation system found in scene (neither NavigationManager nor NavigationHost).");
            if (MainCamera == null)
                Debug.LogError("[GameBootstrap] Main Camera not found in scene.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            Canvas = null;
            CombatWorld = null;
            NavigationManager = null;
            NavigationHost = null;
            MainCamera = null;
        }

        internal static void ResetForTest() => ResetOnDomainReload();
    }
}
