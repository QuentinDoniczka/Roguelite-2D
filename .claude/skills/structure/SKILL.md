---
name: structure
description: Affiche la structure du projet dans le contexte actuel
---

Lis et affiche le contenu de `.claude/STRUCTURE.md` du projet actuel.

## Instructions

1. Lire le fichier `.claude/STRUCTURE.md` du projet courant
2. Afficher son contenu intégralement dans la conversation
3. Si le fichier n'existe pas, informer l'utilisateur : "Structure non trouvée. Utilise `/update-structure` pour la générer."

## But

Injecter la structure du projet dans le contexte pour :
- Comprendre rapidement l'organisation du code
- Savoir où chercher sans scanner
- Identifier les responsabilités par le nommage des dossiers/fichiers

$ARGUMENTS
