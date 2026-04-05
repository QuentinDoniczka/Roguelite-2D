# Project Structure
Generated: 2026-04-05 (updated feature/154-skill-tree-edge-rendering)

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
    plan-level-scroll-transition.md
    premier-jet-roguelite.html
    techtreeidea.png
  Fonts/  (empty)
  Materials/  (1 file)
  MedievalFantasyCharacters/  (asset store package)
  Prefabs/
    Characters/  (5 prefabs)
    Effects/  (empty)
    UI/  (empty)
  Scenes/
    GameScene.unity
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
    Editor/
      RogueliteAutoBattler.Editor.asmdef
      EditorUIFactory.cs
      Builders/
        BootstrapSceneBuilder.cs
        CombatHudBuilder.cs
        CombatWorldBuilder.cs
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
          SkillTreeInputHandler.cs
          SkillTreeNode.cs
          SkillTreeNodeManager.cs
          SkillTreeScreen.cs
        Village/
          VillageScreen.cs
      Widgets/
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
    UI/  (empty -- circle_white.png generated at build time by SkillTreeBuilder)
  Tests/
    EditMode/
      Tests.EditMode.asmdef
      AttackSlotRegistryTests.cs
      CombatStatsDamageEventTests.cs
      CombatStatsTests.cs
      FormationLayoutTests.cs
      GoldFormatterTests.cs
      RecalculateFormationTests.cs
      SkillTreeDataTests.cs
      TargetFinderTests.cs
    PlayMode/
      Tests.PlayMode.asmdef
      TestUtils/
        PlayModeTestBase.cs
        TestCharacterFactory.cs
      AnimationEventRelayTests.cs
      BattleIndicatorBadgeTests.cs
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
      GoldHudBadgeTests.cs
      GoldWalletTests.cs
      HealthBarTrailTests.cs
      LevelManagerDefeatResetTests.cs
      LevelManagerDefeatTests.cs
      LevelManagerEventTests.cs
      LevelManagerStepTransitionTests.cs
      LevelManagerTotalLevelsTests.cs
      NavigationManagerTests.cs
      ScreenStackTests.cs
      SelectionOutlineTests.cs
      SkillTreeInputHandlerTests.cs
      SkillTreeNodeManagerTests.cs
      SkillTreeNodeTests.cs
      SkillTreeScreenTests.cs
      StepProgressBarTests.cs
      UIScreenTests.cs
      UnitSelectionManagerTests.cs
      VisualEquipmentTestLoopTests.cs
      WorldConveyorTests.cs
  TextMesh Pro/  (173 files -- TMP package: fonts, shaders, examples)

ProjectSettings/  (Unity defaults)
