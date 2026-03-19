# Architecture UI / HUD — Roguelite Auto-Battler 2D

## Contexte

Le projet utilise Unity 6000.3.6 avec URP 2D. L'UI couvre 5 onglets principaux accessibles via une barre de navigation fixe en bas de l'ecran. L'architecture doit supporter mobile (touch) + PC (souris).

---

## Decisions d'architecture

### 1. uGUI (Canvas) — pas UI Toolkit

**Choix : uGUI (Canvas + RectTransform + TextMeshPro)**

UI Toolkit est l'avenir pour les outils Editor et les apps data-heavy, mais pour un HUD de jeu mobile 2D en 2026, uGUI reste le choix production-ready.

Raisons :
- **Touch mobile** : `ScrollRect` gere nativement le scroll inertiel et le swipe. UI Toolkit necessite des `Manipulator` custom.
- **Animations** : uGUI fonctionne avec DOTween/Animator. UI Toolkit n'a que des transitions USS limitees (pas de sequences, pas de bounce).
- **Communaute** : ordres de grandeur plus de tutoriels, exemples, et reponses pour uGUI.
- **Mix sprites + HUD** : Canvas en `Screen Space - Overlay` s'affiche au-dessus du battlefield 2D sans configuration complexe.
- **Maturite runtime mobile** : uGUI est battle-tested sur des milliers de jeux mobiles. UI Toolkit runtime est encore jeune.

### 2. Scene unique avec panel switching

**Choix : 1 seule scene (`GameScene`), 5 panels via `CanvasGroup`**

Pas de chargement de scene entre les onglets. Tout est dans une seule scene.

Raisons :
- **Switch instantane** : zero latence entre onglets (show/hide via `CanvasGroup.alpha`)
- **Etat preserve** : quitter Village pour Combat et revenir = tout est intact
- **Combat en fond** : les entites 2D (aventuriers, ennemis) continuent pendant qu'on browse Village
- **Memoire faible** : les onglets sont du pur UI (texte, boutons, images), pas de geometrie lourde
- **Simplicite** : pas de gestion cross-scene, pas d'event bus inter-scenes

### 3. Navigation stack push/pop pour les sous-ecrans

**Choix : chaque onglet a son propre `ScreenStack`**

```
Tab Village : [VillageScreen] → push → [EntrepotScreen] → "Retour" → pop
Tab Combat  : [CombatScreen] (toggle interne Stats/Inventaire, pas de stack)
Tab Arbre   : [SkillTreeScreen] (ecran unique)
Tab Guilde  : [GuildScreen] (ecran unique)
Tab Shop    : [ShopScreen] (ecran unique)
```

Quand on change d'onglet, le stack de l'onglet precedent est preserve. Revenir sur Village montre le dernier sous-ecran visite.

---

## Onglets du jeu

| # | Onglet | Description | Sous-ecrans |
|---|--------|-------------|-------------|
| 0 | Village | Gestion des batiments | 6 batiments (Recrutement, Caserne, Entrepot, Forge, Temple, Entrainement) |
| 1 | Arbre | Arbre de competences permanent | Aucun |
| 2 | **COMBAT** | Battlefield 2/3 + HUD 1/3 (bouton central, plus grand) | Toggle Stats/Inventaire (pas de stack) |
| 3 | Guilde | Social, classements | Aucun |
| 4 | Shop | Monetisation, cosmetiques | Aucun |

---

## Hierarchie Canvas (scene unique)

```
GameScene
  +-- [Main Camera] (Orthographique, pour sprites 2D battlefield)
  +-- [Battlefield] (GameObjects 2D : aventuriers, ennemis — SpriteRenderers)
  +-- [UICanvas] (Screen Space - Overlay, CanvasScaler 1080x1920)
  |     +-- [TopBar] (Palier, Or, Timer reset — toujours visible)
  |     +-- [ContentArea] (s'etire entre TopBar et BottomNav)
  |     |     +-- [VillagePanel] (CanvasGroup, cache par defaut)
  |     |     |     +-- VillageScreen
  |     |     |     +-- RecrutementScreen (cache)
  |     |     |     +-- EntrepotScreen (cache)
  |     |     |     +-- ForgeScreen (cache)
  |     |     |     +-- TempleScreen (cache)
  |     |     |     +-- CaserneScreen (cache)
  |     |     |     +-- EntrainementScreen (cache)
  |     |     +-- [ArbrePanel] (CanvasGroup, cache)
  |     |     +-- [CombatPanel] (CanvasGroup, visible par defaut)
  |     |     |     +-- BattlefieldOverlay (skills flottants sur le champ de bataille)
  |     |     |     +-- HUDArea (bas 1/3)
  |     |     |           +-- ToggleBar (Stats | Inventaire)
  |     |     |           +-- StatsPanel
  |     |     |           +-- InventoryPanel (cache)
  |     |     +-- [GuildePanel] (CanvasGroup, cache)
  |     |     +-- [ShopPanel] (CanvasGroup, cache)
  |     +-- [BottomNav] (5 boutons onglet — toujours visible, par-dessus)
  |     +-- [ModalLayer] (popups/confirmations — cache)
```

---

## Structure des scripts

```
Assets/Scripts/UI/
  Core/
    UIScreen.cs              — Classe de base pour tous les ecrans
    NavigationManager.cs     — Gere les 5 onglets + bottom nav bar
    ScreenStack.cs           — Push/pop navigation intra-onglet
    TabButton.cs             — Comportement d'un bouton onglet
  Screens/
    Combat/
      CombatScreen.cs        — HUD combat (battlefield overlay + toggle)
      StatsPanel.cs          — Vue stats aventurier selectionne
      InventoryPanel.cs      — Vue inventaire / loot
      SkillBarPanel.cs       — Boutons skills manuels
    Village/
      VillageScreen.cs       — Liste des 6 batiments
      BuildingRowUI.cs       — Widget pour une ligne batiment
      RecrutementScreen.cs   — Sous-ecran : recrutement
      EntrepotScreen.cs      — Sous-ecran : inventaire / auto-sell
      ForgeScreen.cs         — Sous-ecran : amelioration items
      TempleScreen.cs        — Sous-ecran : benedictions
      CaserneScreen.cs       — Sous-ecran : caserne
      EntrainementScreen.cs  — Sous-ecran : entrainement IA
    SkillTree/
      SkillTreeScreen.cs     — Arbre de competences
    Guild/
      GuildScreen.cs         — Social / classements
    Shop/
      ShopScreen.cs          — Monetisation
  Widgets/
    ItemSlotUI.cs            — Slot d'inventaire reutilisable
    StatRowUI.cs             — Ligne stat (label + valeur)
    ProgressBarUI.cs         — Barre de progression reutilisable
    ConfirmDialog.cs         — Popup de confirmation modale
```

---

## Classes principales

### UIScreen (classe de base)

Tous les ecrans heritent de `UIScreen`. Gere le cycle de vie via `CanvasGroup`.

```
Methodes :
  OnShow()   → alpha = 1, blocksRaycasts = true, interactable = true
  OnHide()   → alpha = 0, blocksRaycasts = false, interactable = false
  OnPush()   → appele quand un sous-ecran est pousse par-dessus (masquer sans detruire)
  OnPop()    → appele quand on revient sur cet ecran apres un pop
```

### NavigationManager

Singleton central. Possede les 5 `TabButton` et les 5 `ScreenStack`.

```
Methodes :
  SwitchTab(int index)           → cache l'onglet courant, montre le nouveau
  PushScreen(UIScreen screen)    → push dans le stack de l'onglet actif
  PopScreen()                    → pop du stack de l'onglet actif
  CurrentTab                     → index de l'onglet actif
```

### ScreenStack

Pile de navigation par onglet.

```
Proprietes :
  Current    → ecran en haut de la pile
  Count      → nombre d'ecrans dans la pile

Methodes :
  Push(UIScreen screen)   → cache le courant, montre le nouveau, ajoute a la pile
  Pop() → UIScreen        → cache le courant, montre le precedent, retire de la pile
  Clear()                 → pop tout, montre la racine
```

### TabButton

```
Proprietes :
  TabIndex       → index de l'onglet (0-4)
  IsSelected     → etat visuel actif/inactif

Methodes :
  Select()       → met a jour l'etat visuel (icone highlight, couleur)
  Deselect()     → etat visuel inactif
```

---

## Navigation — flux detaille

### Changement d'onglet
```
1. Utilisateur clique sur TabButton[2] (Combat)
2. NavigationManager.SwitchTab(2)
3. TabButton[ancien].Deselect()
4. ScreenStack[ancien].Current.OnHide()
5. TabButton[2].Select()
6. ScreenStack[2].Current.OnShow()
```

### Push sous-ecran (Village → Entrepot)
```
1. Utilisateur clique "ENTRER" sur batiment Entrepot
2. VillageScreen appelle NavigationManager.PushScreen(entrepotScreen)
3. ScreenStack[0].Push(entrepotScreen)
4. VillageScreen.OnPush() → se masque
5. EntrepotScreen.OnShow() → s'affiche
```

### Pop sous-ecran (Entrepot → Village)
```
1. Utilisateur clique "Retour Village"
2. EntrepotScreen appelle NavigationManager.PopScreen()
3. ScreenStack[0].Pop()
4. EntrepotScreen.OnHide() → se masque
5. VillageScreen.OnPop() → se reaffiche
```

### Retour Android (hardware back button)
```
1. Si ScreenStack[onglet actif].Count > 1 → PopScreen()
2. Sinon → afficher ConfirmDialog("Quitter le jeu ?")
```

---

## Configuration Canvas Scaler

| Parametre | Valeur |
|-----------|--------|
| UI Scale Mode | Scale With Screen Size |
| Reference Resolution | 1080 x 1920 (portrait mobile 9:16) |
| Screen Match Mode | Match Width Or Height |
| Match | 0.5 |

---

## Dependances

| Package | Usage | Status |
|---------|-------|--------|
| com.unity.ugui 2.0.0 | Canvas, RectTransform, uGUI | Installe |
| TextMeshPro | Texte net a toute resolution (SDF) | Inclus dans ugui 2.0.0 |
| com.unity.inputsystem 1.18.0 | Input touch + souris + gamepad | Installe |
| DOTween (gratuit) | Animations UI (fade, slide, bounce) | A installer |

---

## Travail manuel requis (Unity Editor)

Ces etapes ne peuvent pas etre automatisees par le code :

1. **Canvas Scaler** — verifier visuellement le rendu avec la reference 1080x1920
2. **Fonts** — importer un fichier .ttf/.otf et creer un TMP Font Asset via Window > TextMeshPro > Font Asset Creator
3. **Sprites / icones** — importer les assets graphiques (onglets, batiments, items). Placeholders Unity built-in en attendant.
4. **Build Settings** — ajouter `GameScene` dans File > Build Settings
5. **Safe Area** — tester sur appareils avec notch (codable via `Screen.safeArea`)

---

## Phases d'implementation

### Phase 1 : Squelette de navigation (Issue initiale)
- Scene `GameScene` + Canvas + EventSystem
- `UIScreen`, `ScreenStack`, `NavigationManager`, `TabButton`
- 5 panels placeholder (couleurs differentes) + bottom nav fonctionnelle
- Verification : cliquer sur les onglets switch les panels

### Phase 2 : Ecran Combat (V1)
- `CombatScreen` + toggle Stats/Inventaire
- `StatsPanel`, `InventoryPanel`, `SkillBarPanel`
- `TopBar` (palier, or, timer)

### Phase 3 : Ecran Village (V1 partiel)
- `VillageScreen` + 6 lignes batiment
- Push/pop des sous-ecrans
- Sous-ecrans au fur et a mesure du roadmap

### Phase 4 : Onglets restants
- `SkillTreeScreen` (V4)
- `GuildScreen` (V5+)
- `ShopScreen` (V5)

---

## Architecture reseau / backend

### Pas de multiplayer temps reel

Ce jeu est un **single-player avec backend cloud**, pas un multiplayer temps reel. Aucune feature (V1 a V5) ne necessite de synchronisation d'etat en temps reel entre joueurs.

**Fishnet, Mirage, Mirror, Netcode for GameObjects** = overkill. Ces librairies synchronisent l'etat du jeu en temps reel entre clients (FPS, MOBA, co-op). Ici, personne d'autre n'est dans la meme "partie".

| Feature | Communication | Temps reel ? |
|---------|--------------|-------------|
| Auth / login | REST API | Non |
| Sauvegarde progression | REST API | Non |
| Validation combat (anti-triche) | REST API | Non |
| Generation loot | REST API | Non |
| Simulation offline | REST API (calcul serveur) | Non |
| Leaderboard hebdo | REST API (lecture) | Non |
| PvP indirect (V5+) | REST API (compo sauvegardee) | Non |
| Guilde / classements (V5+) | REST API | Non |
| Chat guilde (futur V5+) | WebSocket (SignalR) | Oui |

### Stack reseau

```
Unity Client (C#)                     ASP.NET Core API (C#)
+-----------------+                   +------------------+
| ApiClient       |  ← REST/JSON →   | Controllers      |
|  (UnityWebReq)  |                   |  AuthController  |
|                 |                   |  CombatController|
| DTOs partages   |                   |  LootController  |
|  LoginRequest   |                   |  ProgressController|
|  CombatResult   |                   |                  |
|  LootDrop       |                   | Services         |
+-----------------+                   |  CombatService   |
                                      |  LootService     |
                                      +------------------+
                                              ↓
                                      +------------------+
                                      | PostgreSQL       |
                                      +------------------+
```

### Pourquoi un backend separe (et pas tout dans Unity) ?

| Besoin | Pourquoi pas Unity seul |
|--------|------------------------|
| **Anti-triche** | Client modifiable (Cheat Engine, saves). Le serveur valide. |
| **Persistance** | PlayerPrefs pas securise, pas cross-device. Il faut PostgreSQL. |
| **Auth / comptes** | JWT, hashing mdp, refresh tokens = travail serveur. |
| **Simulation offline** | Quand le joueur ferme le jeu, le serveur continue. |
| **Leaderboard** | Agreger les scores de tous les joueurs = requete BDD. |
| **Transactions** | Achats in-app valides cote serveur. |

### Pourquoi ASP.NET Core ?

- **Meme langage (C#)** des deux cotes → DTOs partageables, meme mental model
- **Gratuit, open-source, ultra-performant** (top 3 benchmarks web)
- **Entity Framework Core** pour PostgreSQL (ORM C#)
- **Ecosysteme integre** : Identity (auth), validation, logging, DI

### Strategie Mock — Client-Only (V1-V4)

**V1 a V4 : pas de backend.** Toutes les donnees sont gerees localement dans Unity. Le backend reel (ASP.NET Core + PostgreSQL) sera implemente en V5.

| Donnee | Strategie V1-V4 | V5 (backend reel) |
|--------|----------------|-------------------|
| Loot / drops | ScriptableObjects avec tables de loot en dur | LootService API |
| Stats ennemis | ScriptableObjects avec stats par palier | BDD, scaling serveur |
| Progression | Donnees locales (fichier JSON ou PlayerPrefs) | ProgressService API |
| Auth / comptes | Pas d'auth, profil local unique | AuthService API (JWT) |
| Combat validation | Pas de validation, client fait foi | CombatService API (serveur autoritaire) |
| Farm offline | Calcul local au retour (temps ecoule x stats) | OfflineSimulator API |
| Leaderboard | Pas de leaderboard | LeaderboardService API |
| Monnaie / gems | Compteurs locaux | Transactions validees serveur |

**Approche technique** : les services Unity (ex: `ILootProvider`, `IProgressProvider`) utilisent une interface. V1-V4 = implementation locale (`LocalLootProvider`). V5 = implementation API (`ApiLootProvider`). Swap par injection de dependances.

### Repos separes

```
Roguelite-Auto-Battler-2D/     ← repo Unity (client)
  Assets/
  Packages/

Roguelite-API/                  ← repo ASP.NET Core (serveur) — a creer en V5
  Controllers/
  Services/
  Models/
  DTOs/
```

**V1 a V4 : 100% local** avec donnees mockees (voir section "Strategie Mock" ci-dessus). Le backend sera cree en V5.

### Couche client reseau (Unity)

```
Assets/Scripts/
  Network/
    ApiClient.cs          — Wrapper UnityWebRequest (GET/POST/PUT/DELETE, auth headers)
    ApiEndpoints.cs       — Constantes URL des endpoints
    AuthService.cs        — Login, register, refresh token
    ProgressService.cs    — Sync progression, save/load
    CombatService.cs      — Envoyer resultats combat, recevoir validation
    LootService.cs        — Recevoir drops valides par le serveur
  DTOs/
    Auth/
      LoginRequest.cs
      LoginResponse.cs
      RegisterRequest.cs
    Combat/
      CombatResultDTO.cs
      CombatValidationDTO.cs
    Loot/
      LootDropDTO.cs
    Progress/
      ProgressSaveDTO.cs
      LeaderboardEntryDTO.cs
```

### Dependances reseau

| Package | Usage | Status |
|---------|-------|--------|
| UnityWebRequest | Appels HTTP REST | Installe (natif) |
| Newtonsoft.Json | Serialisation JSON | A installer (`com.unity.nuget.newtonsoft-json`) |
| NativeWebSocket | WebSocket futur (chat guilde V5+) | Pas avant V5 |
| SignalR (serveur) | WebSocket serveur futur | Pas avant V5 |
