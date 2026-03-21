using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Editor utilities for bulk-assigning sorting layers to SpriteRenderers.
    ///
    /// Sorting layer order (configured in Project Settings > Tags and Layers):
    ///   Default    (0) — unassigned objects
    ///   Background (1) — terrain, sky, tiled ground
    ///   Ground     (2) — decorative ground details
    ///   Characters (3) — adventurers and enemies
    ///   Effects    (4) — VFX, projectiles, particles
    ///   UI         (5) — Canvas HUD elements (not SpriteRenderers)
    /// </summary>
    internal static class SortingLayerHelper
    {
        private const string LayerBackground = "Background";
        private const string LayerCharacters = "Characters";
        private const string LayerEffects    = "Effects";

        // ------------------------------------------------------------------
        // Menu: Set Character Sorting Layer
        // ------------------------------------------------------------------

        /// <summary>
        /// Assigns the "Characters" sorting layer to every SpriteRenderer on the
        /// selected GameObjects and all of their descendants.
        ///
        /// Usage: select one or more GameObjects in the Hierarchy, then run this menu item.
        /// All SpriteRenderer components in the subtree are updated in a single undoable operation.
        /// </summary>
        [MenuItem("Roguelite/Set Character Sorting Layer", false, 200)]
        private static void SetCharacterSortingLayer()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Set Character Sorting Layer",
                    "No GameObjects are selected. Select one or more objects in the Hierarchy first.",
                    "OK");
                return;
            }

            var renderers = CollectRenderersFromSelection(selected);
            if (renderers.Count == 0)
            {
                Debug.Log("[SortingLayerHelper] No SpriteRenderers found in the selected objects.");
                return;
            }

            // Guard: warn if the sorting layer does not exist in the project.
            if (!SortingLayerExists(LayerCharacters))
            {
                EditorUtility.DisplayDialog(
                    "Missing Sorting Layer",
                    $"The sorting layer \"{LayerCharacters}\" does not exist.\n\n" +
                    "Add it in Edit > Project Settings > Tags and Layers, then try again.",
                    "OK");
                return;
            }

            Undo.SetCurrentGroupName("Set Sorting Layer — Characters");
            int groupId = Undo.GetCurrentGroup();

            int changed = 0;
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr.sortingLayerName == LayerCharacters)
                    continue;

                Undo.RecordObject(sr, "Set Sorting Layer — Characters");
                sr.sortingLayerName = LayerCharacters;
                EditorUtility.SetDirty(sr);
                changed++;
            }

            Undo.CollapseUndoOperations(groupId);

            Debug.Log($"[SortingLayerHelper] Set sorting layer \"{LayerCharacters}\" " +
                      $"on {changed} SpriteRenderer(s) across {selected.Length} selected root(s). " +
                      $"({renderers.Count - changed} were already correct.)");
        }

        [MenuItem("Roguelite/Set Character Sorting Layer", true)]
        private static bool SetCharacterSortingLayerValidate()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        // ------------------------------------------------------------------
        // Menu: Fix All Sorting Layers
        // ------------------------------------------------------------------

        /// <summary>
        /// Scans ALL SpriteRenderers in the active scene and assigns sorting layers
        /// based on where each object lives in the CombatWorld hierarchy:
        ///
        ///   CombatWorld/Ground      → Background
        ///   CombatWorld/Characters  → Characters
        ///   CombatWorld/Effects     → Effects
        ///
        /// Objects outside those containers are left untouched.
        /// </summary>
        [MenuItem("Roguelite/Fix All Sorting Layers", false, 201)]
        private static void FixAllSortingLayers()
        {
            // Locate the CombatWorld containers.
            Transform groundContainer    = FindNamedTransform("CombatWorld/Ground");
            Transform charactersContainer = FindNamedTransform("CombatWorld/Characters");
            Transform effectsContainer   = FindNamedTransform("CombatWorld/Effects");

            bool missingContainers =
                groundContainer == null && charactersContainer == null && effectsContainer == null;

            if (missingContainers)
            {
                EditorUtility.DisplayDialog(
                    "Fix All Sorting Layers",
                    "None of the expected CombatWorld containers were found in the scene " +
                    "(CombatWorld/Ground, CombatWorld/Characters, CombatWorld/Effects).\n\n" +
                    "Run Roguelite > Setup Combat Scene first.",
                    "OK");
                return;
            }

            // Guard: ensure the sorting layers actually exist.
            string[] required = { LayerBackground, LayerCharacters, LayerEffects };
            foreach (string layer in required)
            {
                if (!SortingLayerExists(layer))
                {
                    EditorUtility.DisplayDialog(
                        "Missing Sorting Layer",
                        $"The sorting layer \"{layer}\" does not exist.\n\n" +
                        "Add it in Edit > Project Settings > Tags and Layers, then try again.",
                        "OK");
                    return;
                }
            }

            // Collect all SpriteRenderers in the scene (including inactive).
            SpriteRenderer[] allRenderers =
                Object.FindObjectsByType<SpriteRenderer>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);

            Undo.SetCurrentGroupName("Fix All Sorting Layers");
            int groupId = Undo.GetCurrentGroup();

            int bgCount = 0, charCount = 0, fxCount = 0, skipped = 0;

            foreach (SpriteRenderer sr in allRenderers)
            {
                Transform t = sr.transform;

                if (groundContainer    != null && IsDescendantOf(t, groundContainer))
                {
                    if (ApplySortingLayer(sr, LayerBackground)) bgCount++;
                }
                else if (charactersContainer != null && IsDescendantOf(t, charactersContainer))
                {
                    if (ApplySortingLayer(sr, LayerCharacters)) charCount++;
                }
                else if (effectsContainer != null && IsDescendantOf(t, effectsContainer))
                {
                    if (ApplySortingLayer(sr, LayerEffects)) fxCount++;
                }
                else
                {
                    skipped++;
                }
            }

            Undo.CollapseUndoOperations(groupId);

            Debug.Log(
                $"[SortingLayerHelper] Fix All Sorting Layers complete. " +
                $"Background={bgCount}, Characters={charCount}, Effects={fxCount}, " +
                $"skipped (outside known containers)={skipped}.");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns all SpriteRenderers found on the given roots and all their descendants,
        /// with duplicates removed (a child reachable via two selected roots is counted once).
        /// </summary>
        private static List<SpriteRenderer> CollectRenderersFromSelection(GameObject[] roots)
        {
            var seen = new HashSet<int>();         // instance IDs to deduplicate
            var result = new List<SpriteRenderer>();

            foreach (GameObject root in roots)
            {
                SpriteRenderer[] renderers =
                    root.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

                foreach (SpriteRenderer sr in renderers)
                {
                    if (seen.Add(sr.GetInstanceID()))
                        result.Add(sr);
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns <paramref name="layerName"/> to <paramref name="sr"/> if it differs.
        /// Returns true when a change was made.
        /// </summary>
        private static bool ApplySortingLayer(SpriteRenderer sr, string layerName)
        {
            if (sr.sortingLayerName == layerName)
                return false;

            Undo.RecordObject(sr, "Fix All Sorting Layers");
            sr.sortingLayerName = layerName;
            EditorUtility.SetDirty(sr);
            return true;
        }

        /// <summary>
        /// Returns true if <paramref name="t"/> is <paramref name="ancestor"/> itself
        /// or any of its descendants.
        /// </summary>
        private static bool IsDescendantOf(Transform t, Transform ancestor)
        {
            Transform current = t;
            while (current != null)
            {
                if (current == ancestor)
                    return true;
                current = current.parent;
            }
            return false;
        }

        /// <summary>
        /// Finds a Transform by path relative to the scene root using '/' as separator.
        /// Only the active scene is searched; inactive objects are included.
        /// Returns null if not found.
        /// </summary>
        private static Transform FindNamedTransform(string path)
        {
            // Split on '/' and walk the hierarchy.
            string[] parts = path.Split('/');
            if (parts.Length == 0)
                return null;

            // Find the root by name among all root GameObjects.
            GameObject root = null;
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go.transform.parent == null && go.name == parts[0])
                {
                    root = go;
                    break;
                }
            }

            if (root == null)
                return null;

            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                    return null;
            }

            return current;
        }

        /// <summary>
        /// Returns true when a sorting layer with the given name exists in the project.
        /// </summary>
        private static bool SortingLayerExists(string layerName)
        {
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.name == layerName)
                    return true;
            }
            return false;
        }
    }
}
