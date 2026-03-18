---
name: refacto
description: Liste les problèmes de refactoring du plus critique au moins critique
---

Analyse le code et liste TOUS les problèmes du plus critique au moins critique.

## Contexte projet

Ce projet est un **Roguelite Auto-Battler 2D** avec architecture client/serveur :
- **Client Unity 2D** : tout doit etre en 2D (Rigidbody2D, Collider2D, SpriteRenderer, Physics2D)
- **Serveur ASP.NET Core** : logique critique (combat, loot, progression) validee cote serveur
- **Mobile + PC** : performance mobile a prendre en compte

## Critères à analyser

### Composants 3D dans un projet 2D
- Rigidbody au lieu de Rigidbody2D → remplacer
- Collider (BoxCollider, SphereCollider) au lieu de Collider2D → remplacer
- Physics.Raycast au lieu de Physics2D.Raycast → remplacer
- MeshRenderer au lieu de SpriteRenderer → remplacer
- NavMeshAgent ou NavMesh → pas applicable en 2D, utiliser des alternatives

### Frontière client/serveur
- Logique de damage/loot/progression calculee uniquement cote client → doit etre validee serveur
- DTOs melanges avec les MonoBehaviours → separer
- Donnees sensibles (tables de drop, formules) dans le code client → deplacer cote serveur
- Appels API sans gestion d'erreur → ajouter

### Comparaisons null en Unity
- `== null` utilise l'opérateur surchargé Unity (vérifie les objets détruits)
- `is null` vérifie uniquement la référence C# (ignore l'état Unity)
- Toujours préférer `== null` pour les UnityEngine.Object

### Code vs Éditeur Unity
Identifie le code qui devrait plutôt être configuré dans l'éditeur :
- Sprites ou hierarchies statiques creees en code → prefab ou asset dans l'editeur
- Hiérarchies de GameObjects fixes → prefab
- Valeurs hardcodées → champs sérialisés dans l'Inspector
- Chaînes de GetComponent/Find évitables → références assignées dans l'éditeur

### Réinvention de la roue (Overthinking)
Identifie le code qui refait manuellement ce que Unity/C# fait déjà nativement :
- Detection de collision manuelle → Collider2D + OnTriggerEnter2D/OnCollisionEnter2D
- Calcul de distance/angle manuel pour zones → CircleCollider2D, BoxCollider2D
- Gestion manuelle de timers → Coroutines ou Invoke
- Pools d'objets custom simples → ObjectPool<T> (Unity 2021+)
- Sérialisation JSON custom → JsonUtility ou PlayerPrefs
- Machine à états avec switch/enum → Animator avec StateMachineBehaviour
- Tri de sprites manuel → Sorting Layers + Order in Layer
- Camera follow custom simple → Cinemachine 2D

Priorité :
- HAUTE : Code complexe (10+ lignes) qui remplace une feature native Unity
- MOYENNE : Code moyen (5-10 lignes) évitable avec un composant/système existant
- BASSE : Micro-optimisation inutile ou abstraction prématurée

### Extraction de classes
Identifie les portions de code qui peuvent être extraites dans une classe séparée :
- Méthodes qui forment un groupe logique cohérent
- Code dupliqué qui mérite sa propre classe
- Responsabilités distinctes mélangées dans une même classe (violation Single Responsibility)

Priorité extraction :
- HAUTE : 80+ lignes extractibles ou responsabilité clairement séparée
- MOYENNE : 40-80 lignes ou groupe de 3+ méthodes liées
- BASSE : < 40 lignes mais améliorerait la lisibilité

Rester KISS : on extrait seulement si ça simplifie réellement le code, pas pour le plaisir d'abstraire.

### Performance dans les boucles Update/FixedUpdate/LateUpdate
Identifie le code coûteux exécuté chaque frame :

**Comparaisons null répétées sur UnityEngine.Object**
- `== null` et `!= null` sur UnityEngine.Object sont coûteux (opérateur surchargé Unity)
- Chaque comparaison vérifie si l'objet natif C++ existe encore
- Solution : cacher le résultat dans un bool lors d'événements (Awake, OnEnable, etc.)

**Appels de méthodes coûteux chaque frame**
- `base.Update()` / `base.FixedUpdate()` → vérifier si la classe parente est lourde
- Accès répétés à des singletons (`Manager.Instance.Property`) → cacher la référence
- `GetComponent<T>()`, `Find()`, `FindObjectOfType()` → cacher dans Awake
- Allocations dans Update (new, LINQ avec ToList/ToArray, string concat)

**Polling vs Event-driven**
- Vérifier un état chaque frame alors qu'un événement existe (Input callbacks, OnTriggerEnter2D, etc.)
- Solution : s'abonner aux événements et maintenir un état local

Priorité :
- CRITIQUE : Code coûteux dans Update avec impact mesurable sur les fps (surtout mobile)
- HAUTE : Multiples null checks sur UnityEngine.Object par frame
- MOYENNE : Singleton access répétés ou checks redondants
- BASSE : Micro-optimisations avec impact négligeable

### Principes SOLID et architecture
- **Single Responsibility** : Une classe fait trop de choses différentes
- **Open/Closed** : Code qui nécessite modification pour extension (vs héritage/composition)
- **Dependency Inversion** : Dépendances hardcodées vs injection/interfaces
- Couplage fort entre systèmes qui devraient être indépendants

### Code mort et dette technique
- Variables/méthodes non utilisées
- Code commenté laissé en place
- TODO/FIXME/HACK anciens non résolus
- Imports/using inutilisés

## Format de sortie

IMPORTANT : Format texte simple uniquement. PAS de tableau markdown, PAS de colonnes, PAS de syntaxe `|`. Juste des listes numérotées.

### CRITIQUE
1. `Fichier.cs:ligne` - Description du problème
2. ...

### HAUTE
1. `Fichier.cs:ligne` - Description du problème
2. ...

### MOYENNE
1. `Fichier.cs:ligne` - Description du problème
2. ...

### BASSE
1. `Fichier.cs:ligne` - Description du problème
2. ...

Règles :
- Si une section est vide, ne pas l'afficher
- Ne jamais utiliser de tableau markdown
- Garder le format simple : numéro, fichier, description
- Toujours proposer une solution concrète pour chaque problème

$ARGUMENTS
