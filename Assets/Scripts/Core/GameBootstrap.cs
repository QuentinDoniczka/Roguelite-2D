using RogueliteAutoBattler.UI.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Core
{
    public static class GameBootstrap
    {
        public static Canvas Canvas { get; private set; }
        public static Transform CombatWorld { get; private set; }
        public static NavigationManager NavigationManager { get; private set; }
        public static Camera MainCamera { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void Initialize()
        {
            MainCamera = Camera.main;
            Canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            NavigationManager = Object.FindFirstObjectByType<NavigationManager>(FindObjectsInactive.Include);

            var combatWorldGo = GameObject.Find("CombatWorld");
            CombatWorld = combatWorldGo != null ? combatWorldGo.transform : null;

            if (Canvas != null)
                ValidateRefs();
        }

        private static void ValidateRefs()
        {
            if (CombatWorld == null)
                Debug.LogError("[GameBootstrap] CombatWorld not found in scene.");
            if (NavigationManager == null)
                Debug.LogError("[GameBootstrap] NavigationManager not found in scene.");
            if (MainCamera == null)
                Debug.LogError("[GameBootstrap] Main Camera not found in scene.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            Canvas = null;
            CombatWorld = null;
            NavigationManager = null;
            MainCamera = null;
        }

        internal static void ResetForTest() => ResetOnDomainReload();
    }
}
