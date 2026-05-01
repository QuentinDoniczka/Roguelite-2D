# Roguelite Auto-Battler 2D

## Unity Version
**Unity 6000.3.6** — All code, APIs, and solutions must target this version.
- Input System: `UnityEngine.InputSystem` package
- Use `FindObjectsByType<T>()` instead of deprecated `FindObjectsOfType<T>()`

## Project Type
**Jeu 2D mobile + PC** — Roguelite auto-battler avec architecture client/serveur.

## Architecture
- **Client** : Unity 2D (C#) — rendu, UI, inputs, visualisation combat
- **Serveur** : ASP.NET Core Web API — auth, validation, loot, offline sim, anti-triche
- **BDD** : PostgreSQL — comptes, progression, runs, leaderboard
- **Communication** : REST API (JSON DTOs)
- **Serveur autoritaire** : toute logique critique validee cote serveur

## 2D Rules
- **Toujours 2D** : Rigidbody2D, Collider2D, SpriteRenderer, Physics2D — jamais les equivalents 3D
- Sorting Layers + Order in Layer pour l'ordre de rendu
- Camera orthographique
- Sprite atlases pour les performances mobile

## Game Concept
Doc detaille : `Assets/doc/premier-jet-roguelite.html`

# Project Structure
Generated: 2026-05-01

.github/
  workflows/
    protect-dev.yml
    protect-main.yml

Assets/
  Animations/  (23 files)
  Audio/  (1 file)
  Data/
    Adventurers/  (empty)
    Buildings/  (empty)
    Enemies/  (empty)
    LootTables/  (empty)
    DamageNumberConfig.asset
    LevelDatabase.asset
    SkillTreeData.asset
    SkillTreeProgress.asset
    TeamDatabase.asset
  doc/
    MedievalFantasyCharacters/  (14 files)
    architecture-ui.md
    combat.jpeg
    exemple_HUD.jpg
    plan-issue-6.md
    plan-issue-31.md
    plan-issue-40.md
    plan-issue-59.md
    plan-issue-80.md
    plan-issue-180.md
    plan-level-scroll-transition.md
    premier-jet-roguelite.html
    propositions-stats-inventaire.html
    techtreeidea.png
  Fonts/
    Bangers SDF.asset
    Bangers.ttf
  Materials/
    SpriteOutline2D.mat
  MedievalFantasyCharacters/  (empty)
  Prefabs/
    Characters/  (5 prefabs: Elk, Horse, Wildboar, Wolf, sampleCharacterHuman)
    Effects/  (empty)
    UI/  (empty)
  Scenes/
    NewGameScene.unity
  Scripts/
    RogueliteAutoBattler.Runtime.asmdef
    AssemblyInfo.cs
    Adventurers/  (empty)
    Combat/
      Core/
        AllyStatBonusService.cs
        AnimHashes.cs
        AnimationEventRelay.cs
        AttackSlotRegistry.cs
        CharacterMover.cs
        CombatController.cs
        CombatSetupHelper.cs
        CombatSpawnManager.cs
        CombatStats.cs
        FormationLayout.cs
        Modifier.cs
        ModifierSources.cs
        ModifierTier.cs
        StatBreakdownData.cs
        StatModifierEntry.cs
        StatType.cs
        TargetFinder.cs
        TeamMember.cs
        TeamRoster.cs
        UnitSelectionManager.cs
      Environment/
        GroundFitter.cs
        ScreenAnchor.cs
        WorldConveyor.cs
      Levels/
        AllyTargetManager.cs
        DefeatHandler.cs
        EnemySpawner.cs
        LevelManager.cs
      Visuals/
        CharacterAppearance.cs
        CoinFly.cs
        CoinFlyBootstrap.cs
        CoinFlyService.cs
        CombatWorldVisibility.cs
        DamageNumber.cs
        DamageNumberBootstrap.cs
        DamageNumberService.cs
        DamageNumberSettingsPersistence.cs
        HealthBar.cs
        LevelBackgroundApplier.cs
        SelectionOutline.cs
        VisualEquipmentTestLoop.cs
    Common/
      PhysicsLayers.cs
      SortingLayers.cs
      StaticPool.cs
    Core/
      GameBootstrap.cs
    Economy/
      GoldFormatter.cs
      GoldWallet.cs
      SkillPointWallet.cs
    Editor/
      RogueliteAutoBattler.Editor.asmdef
      AssemblyInfo.cs
      EditorUIFactory.cs
      Builders/
        CombatHudBuilder.cs
        CombatWorldBuilder.cs
        NavigationHostBuilder.cs
        NewGameSceneBuilder.cs
        RoundedRectSpriteGenerator.cs
        SkillTreeScreenBuilder.cs
        WalletsBuilder.cs
      Tools/
        BranchPlacement.cs
        BranchPreviewSettings.cs
        HudIconsImporter.cs
        LevelDatabaseBackgroundMigrator.cs
        NavIconsImporter.cs
        RebuildNewGameScene.cs
        ResetPlayerProgressMenu.cs
        SkillTreeNodeFactory.cs
        SkillTreeNodeIdAllocator.cs
        StatTypeValidator.cs
      Windows/
        GameDesignerWindow.cs
        LevelDesignerTab.cs
        SettingsWindow.cs
        SkillTreeDesignerWindow.cs
        TeamBuilderTab.cs
    Items/  (empty)
    Data/
      DamageNumberConfig.cs
      LevelDataTypes.cs
      LevelDatabase.cs
      SkillTreeData.cs
      SkillTreeProgress.cs
      TeamDatabase.cs
    ScriptableObjects/  (empty)
    Services/
      IPlayerProgressionLoader.cs
      Local/
        LocalPlayerProgressionLoader.cs
    UI/
      Toolkit/
        AllyStatsPanelController.cs
        BattleIndicatorController.cs
        CombatHudController.cs
        GoldBadgeController.cs
        IScreen.cs
        NavigationHost.cs
        NavigationManager.cs
        ScreenStack.cs
        SkillPointBadgeController.cs
        StepProgressBarController.cs
        SkillTree/
          SkillTreeDetailPanelController.cs
          SkillTreeEdgeLayer.cs
          SkillTreeNodeElement.cs
          SkillTreePanZoomManipulator.cs
          SkillTreeScreenController.cs
          SkillTreeStateEvaluator.cs
    Village/  (empty)
  Settings/
    DefaultVolumeProfile.asset
    InputSystem_Actions.inputactions
    Lit2DSceneTemplate.scenetemplate
    Renderer2D.asset
    Scenes/
      URP2DSceneTemplate.unity
    UniversalRP.asset
    UniversalRenderPipelineGlobalSettings.asset
  Shaders/
    SpriteOutline2D.shader
    SpriteSilhouette2D.shader
  Sprites/
    Characters/  (156 files)
    Effects/  (25 files)
    Environment/  (5 files)
    Items/  (54 files)
    UI/  (1 file)
  Tests/
    EditMode/
      Tests.EditMode.asmdef
      TestUtils/
        StubScreen.cs
      AllyStatBonusServiceResolverTests.cs
      AttackSlotRegistryTests.cs
      BranchPlacementTests.cs
      HudIconAssetTests.cs
      NavBarIconsTests.cs
      SkillTreeDesignerBranchTests.cs
      SkillTreeNodeFactoryTests.cs
      SkillTreeNodeIdAllocatorTests.cs
      CombatStatsBreakdownAllStatsTests.cs
      CombatStatsBreakdownTests.cs
      CombatStatsDamageEventTests.cs
      CombatStatsModifierPipelineTests.cs
      CombatStatsTests.cs
      CombatWorldBuilderGroundTests.cs
      EditorBuildSettingsSceneTests.cs
      FormationLayoutTests.cs
      GoldFormatterTests.cs
      LevelDataBackgroundTests.cs
      LevelDatabaseAssetIntegrityTests.cs
      LevelDatabaseDefaultBackgroundTests.cs
      LevelDesignerTabPropertiesTests.cs
      LocalPlayerProgressionLoaderTests.cs
      ModifierSourcesTests.cs
      ModifierStructTests.cs
      ModifierTierTests.cs
      NewGameSceneBuilderTests.cs
      RecalculateFormationTests.cs
      ResetPlayerProgressMenuTests.cs
      SkillTreeAssetIntegrityTests.cs
      SkillTreeDataAddNodeAddEdgeTests.cs
      SkillTreeDataCentralNodeTests.cs
      SkillTreeProgressTests.cs
      SkillTreeScreenBuilderTests.cs
      SkillTreeStateEvaluatorLockedTests.cs
      StageDataMigrationTests.cs
      StatBreakdownDataTests.cs
      StatModifierEntryTests.cs
      StatTypeIndicesTests.cs
      StatTypeValidatorTests.cs
      TargetFinderTests.cs
      TeamMemberTests.cs
      ToolkitIScreenTests.cs
      ToolkitNavigationManagerTests.cs
      ToolkitScreenStackTests.cs
      WalletsBuilderTests.cs
    PlayMode/
      Tests.PlayMode.asmdef
      TestUtils/
        PlayModeTestBase.cs
        TestCharacterFactory.cs
      AllyStatBonusServiceTests.cs
      AllyStatsPanelControllerTests.cs
      AllyStatsPanelScalingTests.cs
      AnimationEventRelayTests.cs
      BattleIndicatorControllerTests.cs
      CharacterAppearanceTests.cs
      CharacterMoverTests.cs
      CoinFlyServiceTests.cs
      CoinFlyTests.cs
      CombatControllerTests.cs
      CombatHudControllerTests.cs
      CombatSetupHelperTests.cs
      CombatSpawnManagerTests.cs
      CombatStatsHealToFullTests.cs
      CombatStatsRegenTests.cs
      CombatWorldVisibilityNavTests.cs
      DamageNumberServiceTests.cs
      DamageNumberTests.cs
      DefeatHandlerRosterTests.cs
      FormationRecalculationTests.cs
      GameBootstrapTests.cs
      GoldBadgeControllerTests.cs
      GoldWalletTests.cs
      HudIconResolvedStyleTests.cs
      GroundFitterFitModeTests.cs
      HealthBarTrailTests.cs
      LevelManagerDefeatResetTests.cs
      LevelManagerDefeatTests.cs
      LevelManagerBackgroundTests.cs
      LevelManagerEventTests.cs
      LevelManagerReviveOnLevelTests.cs
      LevelManagerStepTransitionTests.cs
      LevelManagerTotalLevelsTests.cs
      LevelManagerVisualSwapTests.cs
      NavigationHostInfoAreaToggleTests.cs
      NavigationHostTests.cs
      NewGameSceneSmokeTests.cs
      ScreenAnchorTests.cs
      SelectionOutlineTests.cs
      SkillPointWalletTests.cs
      SkillTreeDetailPanelControllerTests.cs
      SkillTreeEdgeLayerTests.cs
      SkillTreeLayoutTests.cs
      SkillTreeNodeElementTests.cs
      SkillTreeNodeStateStylesTests.cs
      SkillTreePanZoomManipulatorTests.cs
      SkillTreeScreenControllerTests.cs
      StepProgressBarControllerTests.cs
      TeamRosterTests.cs
      UnitSelectionManagerTests.cs
      VisualEquipmentTestLoopTests.cs
      WorldConveyorTests.cs
  TextMesh Pro/  (173 files -- TMP package: fonts, shaders, examples)
  UI/
    Icons/
      HUD/  (4 sprites: gold, diamant, warrior, arrow)
      Nav/  (6 sprites: village, skilltree, map, guilde, shop, remove)
    Layouts/
      MainLayout.uxml
    MainPanelSettings.asset
    Styles/
      MainStyle.uss
  UI Toolkit/
    UnityThemes/
      UnityDefaultRuntimeTheme.tss
  _Recovery/  (empty)

ProjectSettings/  (Unity defaults)
