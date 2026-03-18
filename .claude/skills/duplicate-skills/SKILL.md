---
name: duplicate-skills
description: Copie les commandes globales (user) vers le projet actuel pour les versionner
---

Copie toutes les commandes de `~/.claude/skills/` vers `.claude/skills/` du projet actuel.

## Instructions

1. Lister tous les dossiers dans `~/.claude/skills/`
2. Pour chaque skill trouvée :
   - Créer le dossier `.claude/skills/<nom>/` dans le projet actuel
   - Copier le fichier `SKILL.md` (et autres fichiers si présents)
3. NE PAS copier la commande `duplicate-skills` elle-même (éviter la récursion inutile)
4. Afficher un résumé des commandes copiées

## Format de sortie

```
Skills copiées vers .claude/skills/ :
- /refacto
- /update-structure
- /structure

Total : 3 commandes
Prêtes à être versionnées sur git.
```

## But

Permettre de versionner les commandes personnelles sur git avec le projet, pour :
- Backup des commandes
- Partage avec l'équipe
- Restauration sur un nouveau PC

$ARGUMENTS
