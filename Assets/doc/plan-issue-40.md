# Issue #40 — Separer monde 2D et HUD Canvas

## Probleme actuel

Tout (contenu de jeu + HUD) est dans un seul Canvas Screen Space Overlay (1080x1920 pixels) :
- Echelle x100 par rapport aux unites monde Unity → les persos devraient etre scales x100
- Pas de Sorting Layers → impossible de gerer la profondeur 2D
- Le contenu de jeu (combat, village) est traite comme du UI alors que c'est du monde 2D

## Architecture cible : separation Layer-Based

### Monde 2D (camera orthographique)

Rendu par la camera ortho, a echelle normale (persos ~1 unite).

```
WorldContent/
  CombatWorld/          (TOUJOURS actif — le combat continue en fond)
    Background          (SpriteRenderer, Sorting Layer: Background)
    Characters/         (SpriteRenderers, Sorting Layer: Characters)
    Effects/            (Sorting Layer: Effects)
  VillageWorld/         (active uniquement quand tab Village selectionnee)
    VillageBackground
    Buildings/
  SkillTreeWorld/       (idem)
  GuildWorld/           (idem)
  ShopWorld/            (idem)
```

Gere par un nouveau **WorldLayerManager** qui ecoute `NavigationManager.OnTabChanged` et fait `SetActive(true/false)` sur les roots monde.

### Canvas Screen Space Overlay (HUD uniquement)

Reste en pixels (1080x1920), uniquement pour les elements UI qui se superposent au monde.

```
UICanvas/ (Screen Space Overlay)
  InfoArea/             (30% du bas — panneaux info par tab)
    CombatInfo          (UIScreen + CanvasGroup)
    VillageInfo
    ...
  NavBar/               (8% du bas — 5 boutons tab)
  ModalLayer/
  NavigationManager
```

### Camera

- Orthographique, size ~5.4
- Le monde s'etend derriere le HUD (pas de viewport crop)
- Le Canvas Overlay se dessine par-dessus automatiquement

### Sorting Layers (a configurer)

| Ordre | Nom          | Usage                    |
|-------|-------------|--------------------------|
| 0     | Background  | Fonds de chaque ecran    |
| 1     | World       | Elements de decor        |
| 2     | Characters  | Persos et ennemis        |
| 3     | Effects     | Particules, VFX          |

### Echelle

- Personnages : ~1 unite de haut
- Camera ortho size : ~5.4 (10.8 unites de haut visible)
- Zone visible au-dessus du HUD : ~6.5 unites
- Sprites 128px → ~1 unite (bonne densite pixel sur mobile)

## Code impacte

### Nouveau fichier

- **`WorldLayerManager.cs`** (~30 lignes)
  - MonoBehaviour, ecoute `NavigationManager.OnTabChanged`
  - Array de roots monde (`GameObject[]`)
  - `SetActive(true/false)` selon le tab actif
  - Le combat reste toujours actif (jamais desactive)

### Fichiers modifies

- **`NavigationManager.cs`** — Retirer `_rootScreens` / `_defaultScreen` (plus de panneaux monde dans le Canvas). Garder `_infoScreens`, `_defaultInfoScreen`, `_tabButtons`. L'event `OnTabChanged` reste le point d'integration.
- **`SetupNavigationSceneEditor.cs`** — Refaire : creer les roots monde (WorldContent + enfants) + le Canvas HUD (sans GameArea). Configurer les Sorting Layers.
- **`UIScreen.cs`** — Aucun changement. Reste HUD-only avec CanvasGroup.
- **`ScreenStack.cs`** — Aucun changement.
- **`TabButton.cs`** — Aucun changement.

### Fichiers potentiellement supprimes

- Les screen implementations vides (CombatScreen.cs, VillageScreen.cs, etc.) si elles ne servent plus cote Canvas. A evaluer — elles pourraient devenir des composants sur les roots monde.

## TODO supplementaire : Mettre a jour l'agent dev-ux-unity

L'agent `dev-ux-unity` (qui genere les scripts Editor interactifs pour le setup scene) doit etre mis a jour pour :
- Connaitre la nouvelle architecture monde 2D + HUD Canvas
- Generer des setups qui respectent la separation (pas tout dans le Canvas)
- Faire des recherches internet sur les best practices Unity 2D pour :
  - Setup orthographic camera optimal pour mobile portrait
  - Sorting Layers et order in layer patterns
  - World-space vs Screen-space pour les jeux 2D mobiles
  - Organisation hierarchie scene Unity 2D
- Devenir vraiment performant pour generer des scenes 2D propres

L'objectif est que quand on lance le setup, la scene soit prete avec :
- Camera bien configuree
- Sorting Layers definis
- Monde 2D structure
- Canvas HUD minimal par-dessus
- Tout cable et fonctionnel

## Regles

- **Client uniquement** — pas d'impact serveur
- **2D uniquement** — Rigidbody2D, Collider2D, SpriteRenderer, Physics2D
- **Pas de scale x100** — tout en unites monde normales
- **Combat toujours actif** — ne jamais desactiver le monde combat
