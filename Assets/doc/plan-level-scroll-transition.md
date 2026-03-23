# Plan : Transition scroll entre les levels

## Contexte
Actuellement, quand un level se termine (tous les ennemis morts), le level suivant demarre immediatement (`LevelManager.OnLevelComplete` appelle `StartLevel`). On veut une transition visuelle entre les levels.

## Objectif
Quand un level est termine :
1. Les allies retournent a leur TeamHomeAnchor (deja implemente)
2. Le CombatWorld scroll vers la gauche de X unites (WorldConveyor existe deja)
3. Les ennemis du level suivant spawnent **hors ecran a droite** sur le CombatWorld
4. Une fois le scroll termine, le combat reprend

## Comportement attendu

```
Level N termine (tous ennemis morts)
    |
    v
1. Allies retournent au TeamHomeAnchor (ecran-gauche)
   Attendre qu'ils soient arrives (ou court delai)
    |
    v
2. CombatWorld commence a scroller vers la gauche (WorldConveyor.ScrollBy)
   → le terrain defile
   → les allies compensent le scroll en marchant vers leur HomeAnchor
    |
    v
3. Quand le scroll COMMENCE A RALENTIR (phase deceleration) :
   → spawn des ennemis du Level N+1 a droite, hors ecran, sur le CombatWorld
   → ils "arrivent" naturellement avec la fin du scroll
    |
    v
4. Scroll termine → assigner les targets → combat reprend
```

## Points techniques

### Timing du spawn ennemis
- WorldConveyor a deja une phase deceleration (brakingDist = v^2 / 2a)
- Il faut un event/callback quand le scroll entre en phase deceleration
- A ce moment : spawner les ennemis hors ecran a droite sur le CombatWorld
- Position = suffisamment a droite pour etre hors ecran pendant la deceleration
- Quand le scroll finit, ils sont positionnes prets au combat

### Scroll
- `WorldConveyor` existe deja et gere le deplacement du CombatWorld
- Ajouter un event `OnDecelerationStarted` dans WorldConveyor
- `LevelManager` ecoute cet event pour spawner les ennemis
- Parametres : distance, vitesse max, acceleration (deja dans WorldConveyor)

### Allies fixes pendant le scroll
- Les allies sont enfants de CombatWorld donc ils scrollent avec
- MAIS leur HomeAnchor est en position absolue ecran
- Quand ils n'ont pas de cible, CharacterMover les ramene au HomeAnchor
- Donc pendant le scroll, ils "marchent" pour rester au meme endroit a l'ecran
- Verifier que la vitesse de marche est suffisante pour compenser le scroll

### Sequence dans LevelManager.OnLevelComplete
1. Attendre que les allies soient revenus au HomeAnchor (ou court delai ~1s)
2. Lancer le scroll via WorldConveyor
3. WorldConveyor entre en deceleration → fire event
4. LevelManager recoit l'event → spawn ennemis hors ecran a droite
5. Scroll termine → assigner targets → combat reprend

### Risques / questions
- La vitesse de marche des allies doit etre >= vitesse de scroll, sinon ils "glissent" a gauche
- Le GroundFitter continue a fonctionner pendant le scroll (sol = 200 unites de large, OK)
- Bien calculer la position de spawn ennemis pour qu'ils soient hors ecran pendant la decel mais visibles juste apres

## Fichiers concernes
- `LevelManager.cs` — orchestrer la transition (delai → spawn hors ecran → scroll → combat)
- `WorldConveyor.cs` — deja pret, juste a appeler ScrollBy
- `CombatScrollManager.cs` — potentiellement a adapter ou bypasser (actuellement desactive pendant le combat)
- `CharacterMover.cs` — verifier que le retour au HomeAnchor compense bien le scroll
