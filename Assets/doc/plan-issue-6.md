# Issue #6 — Guerrier auto-combat — Plan

## Objectif

Premier combat fonctionnel : un allié et un ennemi spawnés, l'allié se déplace vers l'ennemi.

## Prefab

Utiliser `Assets/Prefabs/Characters/sampleCharacterHuman.prefab` pour les deux (allié ET ennemi).
Le prefab a déjà un Animator avec Idle, Walk, ChopAttack, Run.

## Ce qu'on implémente (par ordre)

### 1. Spawn Manager

Un `CombatSpawnManager` MonoBehaviour sur CombatWorld qui :
- Spawne un allié (instancie le prefab) dans `CombatWorld/Team/` — côté gauche de l'écran
- Spawne un ennemi (instancie le même prefab) dans `CombatWorld/Enemies/` — côté droit
- L'ennemi est flippé (localScale.x = -1) pour regarder vers la gauche
- Positions configurables dans l'Inspector (spawnX allié, spawnX ennemi, spawnY)

### 2. Classe de déplacement

Un `CharacterMover` MonoBehaviour ajouté à chaque personnage spawné :
- Champ `target` (Transform) — la cible vers laquelle se déplacer
- Champ `moveSpeed` (float) — vitesse de déplacement
- Champ `stoppingDistance` (float) — distance à laquelle s'arrêter devant la cible
- Dans Update : si target assigné et distance > stoppingDistance → avancer en X vers target
- Quand il arrive à stoppingDistance → s'arrêter
- Gère l'animation : IsMoving = true pendant le déplacement, false sinon

### 3. Wiring

Le `CombatSpawnManager` après le spawn :
- Récupère le `CharacterMover` sur l'allié
- Lui assigne l'ennemi comme `target`
- L'allié commence à marcher vers l'ennemi automatiquement

## Ce qu'on N'implémente PAS encore

- Stats (HP, ATK, etc.) — prochaine étape
- Attaque / dégâts — prochaine étape
- Mort / respawn — prochaine étape
- Lien avec le convoyeur (CombatScrollManager) — prochaine étape
- ScriptableObject stats — prochaine étape

## Hiérarchie scène attendue

```
CombatWorld (WorldConveyor, CombatScrollManager)
├── Ground (SpriteRenderer, GroundFitter)
├── Team/
│   └── Warrior (sampleCharacterHuman instance + CharacterMover)
├── Enemies/
│   └── Enemy (sampleCharacterHuman instance flippé + CharacterMover)
└── Effects/
```

## Fichiers à créer

| Fichier | Type | Rôle |
|---------|------|------|
| `Assets/Scripts/Combat/CharacterMover.cs` | MonoBehaviour | Déplacement vers une cible + animation |
| `Assets/Scripts/Combat/CombatSpawnManager.cs` | MonoBehaviour | Spawn allié + ennemi, wire le target |

## Fichiers à modifier

| Fichier | Changement |
|---------|------------|
| `Assets/Scripts/Editor/CombatWorldBuilder.cs` | Ajouter containers Team/ et Enemies/, ajouter CombatSpawnManager |

## Notes

- Pas de Rigidbody2D — déplacement par transform.position, Apply Root Motion = OFF
- Sorting layer "Characters" sur tous les SpriteRenderers des personnages
- Le CharacterMover est générique : utilisable sur allié ou ennemi (pour plus tard quand l'ennemi aussi bougera)
