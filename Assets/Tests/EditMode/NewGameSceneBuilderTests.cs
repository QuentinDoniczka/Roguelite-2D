using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class NewGameSceneBuilderTests
    {
        private const string SetupMenuItemPath = "Roguelite/Setup New Game Scene";
        private const string ExpectedMainLayoutAssetName = "MainLayout";
        private const string InfoPanelRootName = "info-panel-root";
        private const string HudOverlayName = "hud-overlay";
        private const string NavBarName = "nav-bar";

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void SetupNewGameScene_CreatesCombatWorldGameObject()
        {
            bool executed = EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            Assert.IsTrue(executed, $"MenuItem '{SetupMenuItemPath}' must be registered and executable.");
            GameObject combatWorld = GameObject.Find(GameBootstrap.CombatWorldName);
            Assert.IsNotNull(combatWorld, $"Expected GameObject named '{GameBootstrap.CombatWorldName}' to exist after setup.");
        }

        [Test]
        public void SetupNewGameScene_CreatesEventSystemWithInputSystemUIInputModule()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            Assert.IsNotNull(eventSystem, "Expected an EventSystem in the scene after setup.");
            Assert.IsNotNull(
                eventSystem.GetComponent<InputSystemUIInputModule>(),
                "Expected the EventSystem GameObject to carry InputSystemUIInputModule.");
        }

        [Test]
        public void SetupNewGameScene_CreatesNavigationHost()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            NavigationHost navigationHost = Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);
            Assert.IsNotNull(navigationHost, "Expected a NavigationHost component in the scene after setup.");
        }

        [Test]
        public void SetupNewGameScene_NavigationHostGameObjectHasUIDocument()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            NavigationHost navigationHost = Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);
            Assert.IsNotNull(navigationHost, "NavigationHost must exist before this assertion runs.");

            UIDocument uiDocument = navigationHost.GetComponent<UIDocument>();
            Assert.IsNotNull(uiDocument, "Expected a UIDocument component on the NavigationHost GameObject.");
        }

        [Test]
        public void SetupNewGameScene_UIDocumentReferencesMainLayoutVisualTreeAsset()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            UIDocument uiDocument = FindNavigationHostUIDocument();

            Assert.IsNotNull(uiDocument.visualTreeAsset, "UIDocument.visualTreeAsset must be assigned to drive the UI.");
            StringAssert.Contains(
                ExpectedMainLayoutAssetName,
                uiDocument.visualTreeAsset.name,
                $"Expected visualTreeAsset to reference '{ExpectedMainLayoutAssetName}', got '{uiDocument.visualTreeAsset.name}'.");
        }

        [Test]
        public void SetupNewGameScene_UIDocumentReferencesPanelSettings()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            UIDocument uiDocument = FindNavigationHostUIDocument();

            Assert.IsNotNull(uiDocument.panelSettings, "UIDocument.panelSettings must be assigned for the UI to render.");
        }

        [Test]
        public void SetupNewGameScene_PanelSettingsUsesReferenceResolution1080x1920()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            UIDocument uiDocument = FindNavigationHostUIDocument();
            PanelSettings panelSettings = uiDocument.panelSettings;

            Assert.IsNotNull(panelSettings, "PanelSettings must exist to inspect its reference resolution.");
            Assert.AreEqual(1080, panelSettings.referenceResolution.x, "Reference resolution X should be 1080 for mobile portrait scaling.");
            Assert.AreEqual(1920, panelSettings.referenceResolution.y, "Reference resolution Y should be 1920 for mobile portrait scaling.");
        }

        [Test]
        public void SetupNewGameScene_MainLayoutContainsExpectedStructuralElements()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            UIDocument uiDocument = FindNavigationHostUIDocument();
            VisualTreeAsset mainLayout = uiDocument.visualTreeAsset;
            Assert.IsNotNull(mainLayout, "visualTreeAsset must be set to inspect structure.");

            TemplateContainer cloned = mainLayout.Instantiate();

            Assert.IsNotNull(cloned.Q<VisualElement>(InfoPanelRootName), $"Expected UXML to contain '{InfoPanelRootName}'.");
            Assert.IsNotNull(cloned.Q<VisualElement>(HudOverlayName), $"Expected UXML to contain '{HudOverlayName}'.");
            Assert.IsNotNull(cloned.Q<VisualElement>(NavBarName), $"Expected UXML to contain '{NavBarName}'.");
        }

        [Test]
        public void SetupNewGameScene_CreatesOrthographicMainCamera()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);

            Assert.IsNotNull(mainCamera, "Expected a Camera in the scene after setup.");
            Assert.IsTrue(mainCamera.orthographic, "Main camera must be orthographic for this 2D project.");
        }

        [Test]
        public void SetupNewGameScene_TwiceInSameScene_DoesNotThrow()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            Assert.DoesNotThrow(() => EditorApplication.ExecuteMenuItem(SetupMenuItemPath));
            Assert.IsNotNull(GameObject.Find(GameBootstrap.CombatWorldName), "CombatWorld must still exist after re-running setup.");
        }

        [Test]
        public void SetupNewGameScene_CombatWorldHasTeamRoster()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            GameObject combatWorld = GameObject.Find(GameBootstrap.CombatWorldName);
            Assert.IsNotNull(combatWorld, $"'{GameBootstrap.CombatWorldName}' must exist before checking for TeamRoster.");
            Assert.IsNotNull(
                combatWorld.GetComponent<TeamRoster>(),
                $"CombatWorld root must carry a TeamRoster component so runtime code can discover it via GetComponent<TeamRoster>().");
        }

        [Test]
        public void SetupNewGameScene_CreatesGoldWallet()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            GoldWallet[] goldWallets = Object.FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, goldWallets.Length,
                "Exactly one GoldWallet must exist in the scene after setup (#215).");
        }

        [Test]
        public void SetupNewGameScene_TwiceInSameScene_DoesNotDuplicateGoldWallet()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            GoldWallet[] goldWallets = Object.FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, goldWallets.Length,
                "Running setup twice must not duplicate the GoldWallet (#215 idempotency).");
        }

        [Test]
        public void SetupNewGameScene_CombatHudController_HasGoldWalletAssigned()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            CombatHudController combatHud = Object.FindFirstObjectByType<CombatHudController>(FindObjectsInactive.Include);
            Assert.IsNotNull(combatHud, "CombatHudController must exist in the scene after setup.");

            var serializedHud = new SerializedObject(combatHud);
            SerializedProperty goldWalletProperty = serializedHud.FindProperty("_goldWallet");
            Assert.IsNotNull(goldWalletProperty, "CombatHudController must expose a serialized _goldWallet field.");
            Assert.IsNotNull(goldWalletProperty.objectReferenceValue,
                "CombatHudController._goldWallet must be wired after setup (#215).");
            Assert.IsInstanceOf<GoldWallet>(goldWalletProperty.objectReferenceValue,
                "CombatHudController._goldWallet must reference a GoldWallet component.");
        }

        private static UIDocument FindNavigationHostUIDocument()
        {
            NavigationHost navigationHost = Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);
            Assert.IsNotNull(navigationHost, "NavigationHost must exist before querying its UIDocument.");
            UIDocument uiDocument = navigationHost.GetComponent<UIDocument>();
            Assert.IsNotNull(uiDocument, "UIDocument must exist on the NavigationHost GameObject.");
            return uiDocument;
        }
    }
}
