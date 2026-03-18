---
name: update-structure
description: Scanne et génère la structure du projet dans .claude/STRUCTURE.md
---

Scanne la structure du projet actuel et génère/met à jour le fichier `.claude/STRUCTURE.md`.

## Règles de scan

### Dossiers à EXCLURE complètement
- `.git/`
- `node_modules/`
- `bin/`, `obj/`
- `Library/`, `Temp/`, `Logs/`, `Build/` (Unity)
- `Packages/` (Unity - packages externes)
- `.vs/`, `.idea/`
- `__pycache__/`

### Fichiers à EXCLURE
- `*.meta` (Unity)
- `*.csproj`, `*.sln`
- `.DS_Store`, `Thumbs.db`

### Profondeur intelligente

**Afficher TOUS les fichiers pour :**
- Code source : `*.cs`, `*.js`, `*.ts`, `*.py`, `*.lua`, `*.cpp`, `*.h`, `*.java`
- Config : `*.json`, `*.yaml`, `*.yml`, `*.xml`, `*.config`
- Documentation : `*.md`
- Shaders : `*.shader`, `*.hlsl`, `*.glsl`

**Afficher UNIQUEMENT le dossier (pas les fichiers) pour :**
- Sprites/Textures : dossiers contenant principalement `*.png`, `*.jpg`, `*.psd`, `*.tga`
- Audio : dossiers contenant principalement `*.wav`, `*.mp3`, `*.ogg`
- Models : dossiers contenant principalement `*.fbx`, `*.obj`, `*.blend`
- Materials : dossiers contenant principalement `*.mat`
- Fonts : dossiers contenant principalement `*.ttf`, `*.otf`
- Animations : dossiers contenant principalement `*.anim`, `*.controller`

Pour ces dossiers d'assets, afficher : `Sprites/  (42 files)` avec le compte de fichiers.

## Format de sortie

```
# Project Structure
Generated: YYYY-MM-DD HH:MM

Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   └── EventSystem.cs
│   ├── Combat/
│   │   ├── CombatManager.cs
│   │   └── AutoBattle.cs
│   └── UI/
│       └── UIManager.cs
├── Prefabs/
│   ├── Adventurers/
│   └── Enemies/
├── Sprites/  (127 files)
├── Audio/  (34 files)
└── Materials/  (12 files)
```

## Instructions

1. Scanner le projet depuis la racine (working directory actuel)
2. Construire l'arborescence selon les règles ci-dessus
3. Créer le dossier `.claude/` s'il n'existe pas
4. Écrire/remplacer `.claude/STRUCTURE.md`
5. Dans `CLAUDE.md` (racine du projet), remplacer UNIQUEMENT la section `# Project Structure` et tout ce qui suit jusqu'à la fin du fichier. **NE PAS toucher au contenu au-dessus de `# Project Structure`** (le header contient le contexte du projet).
6. Confirmer avec le nombre de dossiers/fichiers scannés

$ARGUMENTS
