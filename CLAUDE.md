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
Generated: 2026-04-17

.github/
  workflows/
    protect-dev.yml
    protect-main.yml

Assets/
  Animations/  (23 files: .anim + .controller)
  Audio/  (empty)
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
  Materials/  (1 file)
  Prefabs/
    Characters/  (5 prefabs)
    Effects/  (empty)
    UI/  (empty)
  Scenes/
    GameScene.unity
    NewGameScene.unity
  Scripts/
    RogueliteAutoBattler.Runtime.asmdef
    AssemblyInfo.cs
    Adventurers/  (empty)
    Combat/
      Core/
        AnimHashes.cs
        AnimationEventRelay.cs
        AttackSlotRegistry.cs
        CharacterMover.cs
        CombatController.cs
        CombatSetupHelper.cs
        CombatSpawnManager.cs
        CombatStats.cs
        FormationLayout.cs
        StatBreakdownData.cs
        StatModifierEntry.cs
        StatType.cs
        TargetFinder.cs
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
        SelectionOutline.cs
        VisualEquipmentTestLoop.cs
    Common/
      PhysicsLayers.cs
      SortingLayers.cs
      StaticPool.cs
    Core/
      CanvasFactory.cs
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
        BootstrapSceneBuilder.cs
        CombatHudBuilder.cs
        CombatInfoBuilder.cs
        CombatWorldBuilder.cs
        NavigationHostBuilder.cs
        NewGameSceneBuilder.cs
        RoundedRectSpriteGenerator.cs
        SetupNavigationSceneEditor.cs
        SkillTreeBuilder.cs
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
    Services/
      Local/  (empty)
    UI/
      Core/
        NavigationManager.cs
        ScreenStack.cs
        TabButton.cs
        UIScreen.cs
      Screens/
        Combat/
          CombatScreen.cs
          DamageNumberSettingsPanel.cs
        Guild/
          GuildScreen.cs
        Shop/
          ShopScreen.cs
        SkillTree/
          SkillTreeDetailPanel.cs
          SkillTreeInputHandler.cs
          SkillTreeNode.cs
          SkillTreeNodeManager.cs
          SkillTreeScreen.cs
        Village/
          VillageScreen.cs
      Toolkit/
        AllyStatsPanelController.cs
        BattleIndicatorController.cs
        CombatHudController.cs
        GoldBadgeController.cs
        IScreen.cs
        NavigationHost.cs
        NavigationManager.cs
        ScreenStack.cs
        StepProgressBarController.cs
      Widgets/
        AllyStatsPanel.cs
        BattleIndicatorBadge.cs
        GoldHudBadge.cs
        StepProgressBar.cs
    Village/  (empty)
  Settings/
    DefaultVolumeProfile.asset
    InputSystem_Actions.inputactions
    Lit2DSceneTemplate.scenetemplate
    Renderer2D.asset
    Scenes/
      URP2DSceneTemplate.unity
    UniversalRenderPipelineGlobalSettings.asset
    UniversalRP.asset
  Shaders/
    SpriteOutline2D.shader
    SpriteSilhouette2D.shader
  Sprites/
    Characters/  (155 files)
    Effects/  (25 files)
    Environment/
      backgroundtest.png
      grid_ground.png
      grid_ground_blue.png
      map.png
      placeholder_white.png
    Items/  (53 files)
    UI/  (1 file)
  Tests/
    EditMode/
      Tests.EditMode.asmdef
      TestUtils/
        StubScreen.cs
      AttackSlotRegistryTests.cs
      CombatStatsBreakdownTests.cs
      CombatStatsDamageEventTests.cs
      CombatStatsTests.cs
      FormationLayoutTests.cs
      GoldFormatterTests.cs
      RecalculateFormationTests.cs
      SkillTreeDataTests.cs
      SkillTreeProgressTests.cs
      StatBreakdownDataTests.cs
      TargetFinderTests.cs
      ToolkitIScreenTests.cs
      ToolkitNavigationManagerTests.cs
      ToolkitScreenStackTests.cs
    PlayMode/
      Tests.PlayMode.asmdef
      TestUtils/
        AllyStatsPanelTestFixture.cs
        PlayModeTestBase.cs
        TestCharacterFactory.cs
      AllyStatsPanelControllerTests.cs
      AllyStatsPanelTabTests.cs
      AllyStatsPanelTests.cs
      AnimationEventRelayTests.cs
      BattleIndicatorBadgeTests.cs
      BattleIndicatorControllerTests.cs
      CanvasFactoryTests.cs
      CharacterAppearanceTests.cs
      CharacterMoverTests.cs
      CoinFlyServiceTests.cs
      CoinFlyTests.cs
      CombatControllerTests.cs
      CombatSetupHelperTests.cs
      CombatSpawnManagerTests.cs
      CombatStatsRegenTests.cs
      DamageNumberServiceTests.cs
      DamageNumberTests.cs
      FormationRecalculationTests.cs
      GameBootstrapTests.cs
      GoldBadgeControllerTests.cs
      GoldHudBadgeTests.cs
      GoldWalletTests.cs
      HealthBarTrailTests.cs
      LevelManagerDefeatResetTests.cs
      LevelManagerDefeatTests.cs
      LevelManagerEventTests.cs
      LevelManagerStepTransitionTests.cs
      LevelManagerTotalLevelsTests.cs
      NavigationHostTests.cs
      NavigationManagerTests.cs
      ScreenStackTests.cs
      SelectionOutlineTests.cs
      SkillPointWalletTests.cs
      SkillTreeDetailPanelTests.cs
      SkillTreeInputHandlerTests.cs
      SkillTreeNodeManagerTests.cs
      SkillTreeNodeTests.cs
      SkillTreeScreenTests.cs
      StepProgressBarControllerTests.cs
      StepProgressBarTests.cs
      UIScreenTests.cs
      UnitSelectionManagerTests.cs
      VisualEquipmentTestLoopTests.cs
      WorldConveyorTests.cs
  TextMesh Pro/  (173 files -- TMP package: fonts, shaders, examples)
  UI/
    Layouts/
      InfoPanel.uxml
      MainLayout.uxml
    MainPanelSettings.asset
    Styles/
      MainStyle.uss
  UI Toolkit/
    UnityThemes/
      UnityDefaultRuntimeTheme.tss

ProjectSettings/  (Unity defaults)
