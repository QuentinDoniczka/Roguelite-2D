# Plan Issue #180 — UI Toolkit Main Layout + Navigation System

## Architecture Decisions
- **Namespace**: `RogueliteAutoBattler.UI.Toolkit` (avoids conflicts with old uGUI classes)
- **Pattern**: Hybrid — `NavigationHost` (MonoBehaviour shell) + `NavigationManager` (pure C#)
- **Screens**: `IScreen` interface with VisualElement (replaces UIScreen MonoBehaviour)
- **Layout**: Single MainLayout.uxml with empty named containers
- **Show/Hide**: USS class toggle `.hidden { display: none }`
- **Old files**: Kept until Issue #186 cleanup

## Sub-tasks
1. IScreen interface + tests
2. Toolkit ScreenStack + tests
3. USS stylesheet (MainStyle.uss)
4. UXML layout (MainLayout.uxml)
5. Toolkit NavigationManager + tests
6. NavigationHost MonoBehaviour + tests
7. NavigationHostBuilder editor script + PanelSettings
8. GameBootstrap modifications
9. NewGameSceneBuilder modifications
10. Test helpers (TestToolkitFactory)

## New Files
- `Assets/Scripts/UI/Toolkit/IScreen.cs`
- `Assets/Scripts/UI/Toolkit/ScreenStack.cs`
- `Assets/Scripts/UI/Toolkit/NavigationManager.cs`
- `Assets/Scripts/UI/Toolkit/NavigationHost.cs`
- `Assets/UI/Layouts/MainLayout.uxml`
- `Assets/UI/Styles/MainStyle.uss`
- `Assets/UI/MainPanelSettings.asset` (created by builder)
- `Assets/Scripts/Editor/Builders/NavigationHostBuilder.cs`

## Modified Files
- `Assets/Scripts/Core/GameBootstrap.cs`
- `Assets/Scripts/Editor/Builders/NewGameSceneBuilder.cs`

## Technical Debt
- `CombatWorldVisibility` still subscribes to old NavigationManager — needs update in #181-184
