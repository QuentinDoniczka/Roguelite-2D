# Plan : Transition scroll entre les levels

## Contexte
Quand un level se termine (tous les ennemis morts), le level suivant demarrait immediatement (`LevelManager.OnLevelComplete` appelait `StartLevel`). On veut une transition visuelle entre les levels.

## Objectif
Quand un level est termine :
1. Les allies retournent a leur TeamHomeAnchor (CharacterMover.HomeAnchor)
2. Le CombatWorld scroll vers la gauche de 5 unites (WorldConveyor.ScrollBy)
3. Les allies compensent le scroll en marchant vers leur HomeAnchor (ecran-absolu)
4. Scroll termine → les ennemis du level suivant spawnent
5. Le combat reprend

## Comportement attendu

```
Level N termine (tous ennemis morts)
    |
    v
1. ClearAllyTargets() — les allies n'ont plus de cible
   Allies retournent au TeamHomeAnchor (ecran-gauche)
   Attendre _returnDelay (1s)
    |
    v
2. CombatWorld commence a scroller vers la gauche (WorldConveyor.ScrollBy)
   → le terrain defile
   → les allies compensent le scroll en marchant vers leur HomeAnchor
   → maxSpeed (1.5) < ally moveSpeed (2) → pas de drift
    |
    v
3. Scroll termine (WaitUntil !IsScrolling)
   → spawn des ennemis du Level N+1
   → assigner les targets → combat reprend
```

## Implementation

### Fichier modifie : LevelManager.cs

**Parametres ajoutes (SerializeField) :**
- `_scrollDistance = 5f` — distance du scroll entre levels
- `_scrollMaxSpeed = 1.5f` — vitesse max du scroll (doit etre < ally moveSpeed)
- `_scrollAcceleration = 2f` — acceleration/deceleration du scroll
- `_returnDelay = 1f` — delai avant le scroll pour laisser les allies revenir

**Reference ajoutee :**
- `WorldConveyor _conveyor` — cache via `GetComponent<WorldConveyor>()` dans Start()

**OnLevelComplete() modifie :**
1. `ClearAllyTargets()` — met toutes les cibles allies a null
2. `_allyRetargetWired = false` — reset pour le prochain level
3. Lance `LevelTransitionCoroutine()` au lieu de `StartLevel()` direct

**LevelTransitionCoroutine() :**
1. `WaitForSeconds(_returnDelay)` — attente retour allies
2. `_conveyor.ScrollBy(...)` — lance le scroll
3. `WaitUntil(!_conveyor.IsScrolling)` — attend fin du scroll
4. `StartLevel(_currentLevelIndex)` — spawn ennemis level suivant

**ClearAllyTargets() :**
- Parcourt `_teamContainer`, met `CombatController.Target = null` sur chaque allie

### Fichiers NON modifies (deja fonctionnels)
- `WorldConveyor.cs` — ScrollBy + OnScrollComplete deja prets
- `CharacterMover.cs` — HomeAnchor return deja implemente
- `CombatController.cs` — HandleTargetDied gere deja le retour sans cible
- `CombatScrollManager.cs` — reste desactive pendant le combat

## Contrainte vitesse
Le `_scrollMaxSpeed` (1.5) DOIT rester inferieur a la vitesse de marche alliee (2.0).
Si l'allie est plus lent que le scroll, il derive hors ecran a gauche.
