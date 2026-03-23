using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Debug test: spawns a character and tries to move it, logging results.
    /// Tests each layer independently to find what blocks movement.
    /// </summary>
    public static class MovementDebugTest
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        [MenuItem("Roguelite/Debug/Test Movement (Play Mode Only)")]
        private static void RunTest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[MovementDebugTest] Must be in Play Mode!");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[MovementDebugTest] Prefab not found at {PrefabPath}");
                return;
            }

            // Test 1: Raw transform movement, no parent
            TestRawTransform(prefab);

            // Test 2: With parent (like our spawn setup)
            TestWithParent(prefab);

            // Test 3: Disable Animator, move with transform
            TestWithoutAnimator(prefab);

            // Test 4: With Animator disabled + Rigidbody2D velocity
            TestRigidbodyNoAnimator(prefab);

            // Test 5: With Animator enabled + Rigidbody2D velocity
            TestRigidbodyWithAnimator(prefab);
        }

        private static void TestRawTransform(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, new Vector3(-5f, 2f, 0f), Quaternion.identity);
            go.name = "Test1_RawTransform";

            Vector3 before = go.transform.position;
            go.transform.position += Vector3.right * 0.5f;
            Vector3 after = go.transform.position;

            bool moved = !Mathf.Approximately(before.x, after.x);
            Debug.Log($"[TEST 1] Raw transform, no parent: before={before.x:F2} after={after.x:F2} MOVED={moved}");

            Object.Destroy(go, 2f);
        }

        private static void TestWithParent(GameObject prefab)
        {
            var parent = new GameObject("TestParent");
            parent.transform.position = Vector3.zero;

            var go = Object.Instantiate(prefab, new Vector3(-5f, 1f, 0f), Quaternion.identity, parent.transform);
            go.name = "Test2_WithParent";

            Vector3 before = go.transform.position;
            go.transform.position += Vector3.right * 0.5f;
            Vector3 after = go.transform.position;

            bool moved = !Mathf.Approximately(before.x, after.x);
            Debug.Log($"[TEST 2] With parent: before={before.x:F2} after={after.x:F2} MOVED={moved}");

            Object.Destroy(parent, 2f);
        }

        private static void TestWithoutAnimator(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, new Vector3(-5f, 0f, 0f), Quaternion.identity);
            go.name = "Test3_NoAnimator";

            var animator = go.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;

            Vector3 before = go.transform.position;
            go.transform.position += Vector3.right * 0.5f;
            Vector3 after = go.transform.position;

            bool moved = !Mathf.Approximately(before.x, after.x);
            Debug.Log($"[TEST 3] Animator DISABLED: before={before.x:F2} after={after.x:F2} MOVED={moved}");

            Object.Destroy(go, 2f);
        }

        private static void TestRigidbodyNoAnimator(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, new Vector3(-5f, -1f, 0f), Quaternion.identity);
            go.name = "Test4_RB_NoAnimator";

            var animator = go.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(2f, 0f);

            Debug.Log($"[TEST 4] RB2D + Animator DISABLED: velocity set to (2,0). Watch if Test4_RB_NoAnimator moves right. rb.simulated={rb.simulated}");

            Object.Destroy(go, 3f);
        }

        private static void TestRigidbodyWithAnimator(GameObject prefab)
        {
            var go = Object.Instantiate(prefab, new Vector3(-5f, -2f, 0f), Quaternion.identity);
            go.name = "Test5_RB_WithAnimator";

            var animator = go.GetComponent<Animator>();
            if (animator != null)
                animator.applyRootMotion = false;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(2f, 0f);

            Debug.Log($"[TEST 5] RB2D + Animator ENABLED: velocity set to (2,0). Watch if Test5_RB_WithAnimator moves right. rb.simulated={rb.simulated}");

            Object.Destroy(go, 3f);
        }
    }
}
