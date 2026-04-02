# Project Structure
Generated: 2026-04-02 (updated feature/129-level-designer-auto-builder)

.github/
└── workflows/
    ├── protect-dev.yml
    └── protect-main.yml

Assets/
├── Animations/  (23 files: .anim + .controller)
├── Audio/  (empty)
├── Data/
│   ├── Adventurers/  (empty)
│   ├── Buildings/  (empty)
│   ├── Enemies/  (empty)
│   ├── LootTables/  (empty)
│   ├── DamageNumberConfig.asset
│   ├── LevelDatabase.asset
│   └── TeamDatabase.asset
├── doc/
│   ├── MedievalFantasyCharacters/  (14 files)
│   ├── architecture-ui.md
│   ├── combat.jpeg
│   ├── exemple_HUD.jpg
│   ├── plan-issue-6.md
│   ├── plan-issue-31.md
│   ├── plan-issue-40.md
│   ├── plan-issue-59.md
│   ├── plan-issue-80.md
│   ├── plan-level-scroll-transition.md
│   └── premier-jet-roguelite.html
├── Fonts/  (empty)
├── MedievalFantasyCharacters/  (empty)
├── Prefabs/
│   ├── Characters/  (5 prefabs)
│   ├── Effects/  (empty)
│   └── UI/  (empty)
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── RogueliteAutoBattler.Runtime.asmdef
│   ├── AssemblyInfo.cs
│   ├── Adventurers/  (empty)
│   ├── Combat/
│   │   ├── Core/
│   │   │   ├── AnimHashes.cs
│   │   │   ├── AnimationEventRelay.cs
│   │   │   ├── AttackSlotRegistry.cs
│   │   │   ├── CharacterMover.cs
│   │   │   ├── CombatController.cs
│   │   │   ├── CombatSetupHelper.cs
│   │   │   ├── CombatSpawnManager.cs
│   │   │   ├── CombatStats.cs
│   │   │   ├── FormationLayout.cs
│   │   │   └── TargetFinder.cs
│   │   ├── Environment/
│   │   │   ├── GroundFitter.cs
│   │   │   ├── ScreenAnchor.cs
│   │   │   └── WorldConveyor.cs
│   │   ├── Levels/
│   │   │   └── LevelManager.cs
│   │   └── Visuals/
│   │       ├── CharacterAppearance.cs
│   │       ├── CoinFly.cs
│   │       ├── CoinFlyBootstrap.cs
│   │       ├── CoinFlyService.cs
│   │       ├── DamageNumber.cs
│   │       ├── DamageNumberBootstrap.cs
│   │       ├── DamageNumberService.cs
│   │       ├── DamageNumberSettingsPersistence.cs
│   │       ├── HealthBar.cs
│   │       └── VisualEquipmentTestLoop.cs
│   ├── Common/
│   │   └── SortingLayers.cs
│   ├── Core/
│   │   ├── CanvasFactory.cs
│   │   └── GameBootstrap.cs
│   ├── Economy/
│   │   ├── GoldFormatter.cs
│   │   └── GoldWallet.cs
│   ├── Editor/
│   │   ├── RogueliteAutoBattler.Editor.asmdef
│   │   ├── EditorUIFactory.cs
│   │   ├── Builders/
│   │   │   ├── BootstrapSceneBuilder.cs
│   │   │   ├── CombatHudBuilder.cs
│   │   │   ├── CombatWorldBuilder.cs
│   │   │   └── SetupNavigationSceneEditor.cs
│   │   └── Windows/
│   │       ├── GameDesignerWindow.cs
│   │       ├── LevelDesignerTab.cs
│   │       ├── SettingsWindow.cs
│   │       └── TeamBuilderTab.cs
│   ├── Items/  (empty)
│   ├── ScriptableObjects/
│   │   ├── DamageNumberConfig.cs
│   │   ├── LevelDataTypes.cs
│   │   ├── LevelDatabase.cs
│   │   └── TeamDatabase.cs
│   ├── Services/
│   │   └── Local/  (empty)
│   ├── UI/
│   │   ├── Core/
│   │   │   ├── NavigationManager.cs
│   │   │   ├── ScreenStack.cs
│   │   │   ├── TabButton.cs
│   │   │   └── UIScreen.cs
│   │   ├── Screens/
│   │   │   ├── Combat/
│   │   │   │   ├── CombatScreen.cs
│   │   │   │   └── DamageNumberSettingsPanel.cs
│   │   │   ├── Guild/
│   │   │   │   └── GuildScreen.cs
│   │   │   ├── Shop/
│   │   │   │   └── ShopScreen.cs
│   │   │   ├── SkillTree/
│   │   │   │   └── SkillTreeScreen.cs
│   │   │   └── Village/
│   │   │       └── VillageScreen.cs
│   │   └── Widgets/
│   │       ├── BattleIndicatorBadge.cs
│   │       ├── GoldHudBadge.cs
│   │       └── StepProgressBar.cs
│   └── Village/  (empty)
├── Settings/
│   ├── DefaultVolumeProfile.asset
│   ├── InputSystem_Actions.inputactions
│   ├── Lit2DSceneTemplate.scenetemplate
│   ├── Renderer2D.asset
│   ├── Scenes/
│   │   └── URP2DSceneTemplate.unity
│   ├── UniversalRenderPipelineGlobalSettings.asset
│   └── UniversalRP.asset
├── Sprites/
│   ├── Characters/  (155 files)
│   ├── Effects/  (25 files)
│   ├── Environment/
│   │   ├── backgroundtest.png
│   │   ├── grid_ground.png
│   │   ├── grid_ground_blue.png
│   │   ├── map.png
│   │   └── placeholder_white.png
│   ├── Items/  (53 files)
│   └── UI/  (empty)
├── Tests/
│   ├── EditMode/
│   │   ├── Tests.EditMode.asmdef
│   │   ├── EditModeTestBase.cs
│   │   ├── CombatStatsDamageEventTests.cs
│   │   ├── CombatStatsTests.cs
│   │   ├── FormationLayoutTests.cs
│   │   ├── RecalculateFormationTests.cs
│   │   ├── GoldFormatterTests.cs
│   │   └── TargetFinderTests.cs
│   └── PlayMode/
│       ├── Tests.PlayMode.asmdef
│       ├── TestUtils/
│       │   ├── PlayModeTestBase.cs
│       │   └── TestCharacterFactory.cs
│       ├── BattleIndicatorBadgeTests.cs
│       ├── CanvasFactoryTests.cs
│       ├── CharacterAppearanceTests.cs
│       ├── CharacterMoverTests.cs
│       ├── CoinFlyServiceTests.cs
│       ├── CoinFlyTests.cs
│       ├── CombatControllerTests.cs
│       ├── CombatSpawnManagerTests.cs
│       ├── CombatStatsRegenTests.cs
│       ├── DamageNumberServiceTests.cs
│       ├── DamageNumberTests.cs
│       ├── FormationRecalculationTests.cs
│       ├── GameBootstrapTests.cs
│       ├── GoldHudBadgeTests.cs
│       ├── GoldWalletTests.cs
│       ├── HealthBarTrailTests.cs
│       ├── LevelManagerTotalLevelsTests.cs
│       ├── StepProgressBarTests.cs
│       ├── LevelManagerDefeatResetTests.cs
│       ├── LevelManagerDefeatTests.cs
│       ├── LevelManagerEventTests.cs
│       ├── LevelManagerStepTransitionTests.cs
│       ├── VisualEquipmentTestLoopTests.cs
│       └── WorldConveyorTests.cs
├── _Recovery/  (1 file)
└── TextMesh Pro/  (173 files — TMP package: fonts, shaders, examples)

ProjectSettings/  (Unity defaults)
