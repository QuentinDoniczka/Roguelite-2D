# Issue #40 — Separer monde 2D et HUD Canvas

## Probleme actuel

Tout (contenu de jeu + HUD) est dans un seul Canvas Screen Space Overlay (1080x1920 pixels) :
- Echelle x100 par rapport aux unites monde Unity → les persos devraient etre scales x100
- Pas de Sorting Layers → impossible de gerer la profondeur 2D
- Le contenu de jeu (combat, village) est traite comme du UI alors que c'est du monde 2D

## Approche choisie : Option C — Hybride

**Seul CombatWorld vit dans le monde 2D.** Les autres onglets (Village, Shop, Arbre, Guilde) restent des panneaux Canvas opaques. Pas de WorldLayerManager, pas de modification de NavigationManager ou des runtime scripts.

### Pourquoi cette approche

- **Minimal** : seul le combat a besoin du monde 2D (sprites, physique, sorting layers). Village/Shop/etc. sont du pur UI (listes, boutons, texte).
- **Pas de code runtime modifie** : uniquement le script Editor de setup change. Aucun risque de regression.
- **Idempotent** : re-lancer le setup remplace tout proprement (cleanup CombatWorld + Canvas + EventSystem).
- **Progressif** : si un jour Village a besoin d'un monde 2D, on l'ajoute sans casser l'existant.

### Alternatives evaluees et rejetees

- **Option A (full extraction)** : extraire tous les onglets dans le monde 2D, creer un WorldLayerManager. Over-engineering — Village/Shop n'ont pas besoin de SpriteRenderers.
- **Option B (tout Canvas)** : garder tout dans le Canvas. Probleme d'echelle x100 pour les sprites de combat, pas de Sorting Layers.

## Architecture cible

### Monde 2D (camera orthographique)

CombatWorld est toujours actif — le combat continue en fond.

```
CombatWorld/                  (monde 2D, toujours actif)
  Background                  (SpriteRenderer, Sorting Layer: Background)
  Characters/                 (conteneur, Sorting Layer: Characters)
  Effects/                    (conteneur, Sorting Layer: Effects)
```

### Canvas Screen Space Overlay (HUD uniquement)

```
UICanvas/ (Screen Space Overlay, 1080x1920)
  GameArea/ (ancres 40%-100%)
    CombatPanel               (transparent — revele CombatWorld derriere)
    VillagePanel               (opaque, cache)
    SkillTreePanel             (opaque, cache)
    AutrePanel                 (opaque, cache)
    GuildePanel                (opaque, cache)
    ShopPanel                  (opaque, cache)
  InfoArea/ (ancres 8%-40%)
    CombatInfo                 (visible par defaut)
    VillageInfo, ArbreInfo...  (caches)
  NavBar/ (ancres 0%-8%)
  ModalLayer/
  NavigationManager
```

### Camera

- Orthographique, size 5.4 (~10.8 unites de haut visible)
- Position (0, 0, -10)
- Background color noir
- Le Canvas Overlay se dessine par-dessus automatiquement

### Sorting Layers (ajoutes par le setup)

| Ordre | Nom          | Usage                    |
|-------|-------------|--------------------------|
| 0     | Background  | Fond champ de bataille   |
| 1     | Characters  | Aventuriers et ennemis   |
| 2     | Effects     | Particules, VFX          |

### CombatPanel transparent

- Pas d'Image component (pas de fond colore)
- CanvasGroup (alpha=1, blocksRaycasts=true, interactable=true)
- CombatScreen component conserve
- Label "COMBAT" en petit (size 24), ancre au top center comme indicateur debug

## Code impacte

### Fichier modifie

- **`SetupNavigationSceneEditor.cs`** — Ajout de :
  - `EnsureSortingLayers()` : ajoute Background/Characters/Effects dans TagManager
  - `ConfigureMainCamera()` : configure la camera ortho (size 5.4, pos 0,0,-10)
  - `CreateCombatWorld()` : cree la hierarchie monde 2D avec SpriteRenderers
  - `CreateOrLoadPlaceholderSprite()` : cree un placeholder blanc 4x4 sauvegarde comme asset
  - Modification de `SetupNavigationUI()` : cleanup CombatWorld, appel des nouvelles methodes
  - Modification de `CreateCombatPanel()` : suppression Image, label reduit et ancre top center

### Fichiers NON modifies (aucun changement runtime)

- `NavigationManager.cs` — inchange
- `UIScreen.cs` — inchange
- `ScreenStack.cs` — inchange
- `TabButton.cs` — inchange
- `CombatScreen.cs` — inchange
- Tous les screen implementations — inchanges

### Assets crees

- `Assets/Sprites/Environment/placeholder_white.png` — texture 4x4 blanche, importee comme Sprite

## Regles

- **Client uniquement** — pas d'impact serveur
- **2D uniquement** — SpriteRenderer, Sorting Layers, camera orthographique
- **Pas de scale x100** — tout en unites monde normales
- **Combat toujours actif** — CombatWorld jamais desactive
- **Setup idempotent** — re-lancer remplace tout proprement
