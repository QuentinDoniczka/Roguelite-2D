# Project Structure
Generated: 2026-04-23

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
    MedievalFantasyCharacters/  (28 files)
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
  Materials/  (2 files)
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
        ProceduralGroundSprite.cs
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
      Local/  (empty)
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
    Characters/  (345 files)
    Effects/  (55 files)
    Environment/  (11 files)
    Items/  (116 files)
    UI/  (3 files)
  Tests/
    EditMode/
      Tests.EditMode.asmdef
      TestUtils/
        StubScreen.cs
      AttackSlotRegistryTests.cs
      CombatStatsBreakdownTests.cs
      CombatStatsDamageEventTests.cs
      CombatStatsTests.cs
      EditorBuildSettingsSceneTests.cs
      FormationLayoutTests.cs
      GoldFormatterTests.cs
      NewGameSceneBuilderTests.cs
      ProceduralGroundSpriteTests.cs
      RecalculateFormationTests.cs
      SkillTreeDataAssetSpacingTests.cs
      SkillTreeDataTests.cs
      SkillTreeProgressTests.cs
      SkillTreeScreenBuilderTests.cs
      StatBreakdownDataTests.cs
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
      CombatStatsRegenTests.cs
      CombatWorldVisibilityNavTests.cs
      DamageNumberServiceTests.cs
      DamageNumberTests.cs
      DefeatHandlerRosterTests.cs
      FormationRecalculationTests.cs
      GameBootstrapTests.cs
      GoldBadgeControllerTests.cs
      GoldWalletTests.cs
      HealthBarTrailTests.cs
      LevelManagerDefeatResetTests.cs
      LevelManagerDefeatTests.cs
      LevelManagerEventTests.cs
      LevelManagerReviveOnLevelTests.cs
      LevelManagerStepTransitionTests.cs
      LevelManagerTerrainFallbackTests.cs
      LevelManagerTotalLevelsTests.cs
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
  TextMesh Pro/  (366 files -- TMP package: fonts, shaders, examples)
  UI/
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
