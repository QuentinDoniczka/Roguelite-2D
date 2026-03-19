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
Generated: 2026-03-19

Assets/
├── Animations/  (empty)
├── Audio/  (empty)
├── Data/
│   ├── Adventurers/  (empty)
│   ├── Buildings/  (empty)
│   ├── Enemies/  (empty)
│   └── LootTables/  (empty)
├── doc/
│   ├── architecture-ui.md
│   └── premier-jet-roguelite.html
├── Fonts/  (empty)
├── Prefabs/
│   ├── Characters/  (empty)
│   ├── Effects/  (empty)
│   └── UI/  (empty)
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── Adventurers/  (empty)
│   ├── Combat/  (empty)
│   ├── Core/  (empty)
│   ├── Items/  (empty)
│   ├── ScriptableObjects/  (empty)
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
│   │   │   │   └── CombatScreen.cs
│   │   │   ├── Guild/
│   │   │   │   └── GuildScreen.cs
│   │   │   ├── Shop/
│   │   │   │   └── ShopScreen.cs
│   │   │   ├── SkillTree/
│   │   │   │   └── SkillTreeScreen.cs
│   │   │   └── Village/
│   │   │       └── VillageScreen.cs
│   │   └── Widgets/  (empty)
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
└── Sprites/
    ├── Characters/  (empty)
    ├── Environment/  (empty)
    ├── Items/  (empty)
    └── UI/  (empty)

ProjectSettings/  (Unity defaults)
