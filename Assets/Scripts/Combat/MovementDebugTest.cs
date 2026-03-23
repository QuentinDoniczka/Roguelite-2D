using System.Collections;
using System.IO;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Auto-runs movement tests when the scene loads in Play Mode.
    /// Writes results to Assets/test-results.txt and logs to console.
    /// </summary>
    public class MovementDebugTest : MonoBehaviour
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";
        private const string ResultPath = "Assets/test-results.txt";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoRun()
        {
            var go = new GameObject("__MovementTest__");
            go.AddComponent<MovementDebugTest>();
        }

        private void Start()
        {
            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            yield return null; // wait 1 frame for scene to settle

            var prefab = Resources.Load<GameObject>("sampleCharacterHuman");
            if (prefab == null)
            {
                // Try loading via path
                #if UNITY_EDITOR
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                #endif
            }

            if (prefab == null)
            {
                Log("FATAL: Cannot load prefab. Aborting tests.");
                WriteResults();
                Quit();
                yield break;
            }

            // TEST 1: Raw transform.position change
            {
                var go = Instantiate(prefab, new Vector3(-5f, 2f, 0f), Quaternion.identity);
                go.name = "Test1";
                Vector3 before = go.transform.position;
                go.transform.position += Vector3.right;
                Vector3 after = go.transform.position;
                Log($"TEST1 raw_transform: before={before.x:F2} after={after.x:F2} MOVED={!Mathf.Approximately(before.x, after.x)}");
                Destroy(go);
            }

            // TEST 2: Animator disabled + transform
            {
                var go = Instantiate(prefab, new Vector3(-5f, 1f, 0f), Quaternion.identity);
                go.name = "Test2";
                var anim = go.GetComponent<Animator>();
                if (anim) anim.enabled = false;
                go.transform.position += Vector3.right;
                yield return null; // wait 1 frame
                Vector3 pos = go.transform.position;
                Log($"TEST2 animator_disabled_transform: pos={pos.x:F2} (expected -4) MOVED={pos.x > -4.5f}");
                Destroy(go);
            }

            // TEST 3: Animator enabled + transform, check after 1 frame if animator resets
            {
                var go = Instantiate(prefab, new Vector3(-5f, 0f, 0f), Quaternion.identity);
                go.name = "Test3";
                var anim = go.GetComponent<Animator>();
                if (anim) anim.applyRootMotion = false;
                go.transform.position = new Vector3(-3f, 0f, 0f); // move it
                Log($"TEST3 animator_enabled: set pos to -3");
                yield return null; // wait 1 frame for animator to evaluate
                Vector3 posAfter = go.transform.position;
                Log($"TEST3 animator_enabled: after 1 frame pos={posAfter.x:F2} STAYED={Mathf.Approximately(posAfter.x, -3f)} (if False, animator reset it!)");
                Destroy(go);
            }

            // TEST 4: RB2D + Animator disabled + velocity
            {
                var go = Instantiate(prefab, new Vector3(-5f, -1f, 0f), Quaternion.identity);
                go.name = "Test4";
                var anim = go.GetComponent<Animator>();
                if (anim) anim.enabled = false;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(3f, 0f);
                Log($"TEST4 rb_no_animator: vel=(3,0) simulated={rb.simulated} bodyType={rb.bodyType}");
                yield return new WaitForSeconds(0.5f);
                Vector3 posAfter = go.transform.position;
                Log($"TEST4 rb_no_animator: after 0.5s pos={posAfter.x:F2} MOVED={posAfter.x > -4.5f}");
                Destroy(go);
            }

            // TEST 5: RB2D + Animator enabled + velocity
            {
                var go = Instantiate(prefab, new Vector3(-5f, -2f, 0f), Quaternion.identity);
                go.name = "Test5";
                var anim = go.GetComponent<Animator>();
                if (anim) anim.applyRootMotion = false;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(3f, 0f);
                Log($"TEST5 rb_with_animator: vel=(3,0) simulated={rb.simulated}");
                yield return new WaitForSeconds(0.5f);
                Vector3 posAfter = go.transform.position;
                Log($"TEST5 rb_with_animator: after 0.5s pos={posAfter.x:F2} MOVED={posAfter.x > -4.5f}");
                Destroy(go);
            }

            Log("ALL TESTS DONE");
            WriteResults();
            Quit();
        }

        private static readonly System.Text.StringBuilder _results = new();

        private static void Log(string msg)
        {
            Debug.Log($"[MovementTest] {msg}");
            _results.AppendLine(msg);
        }

        private static void WriteResults()
        {
            File.WriteAllText(ResultPath, _results.ToString());
            Debug.Log($"[MovementTest] Results written to {ResultPath}");
        }

        private static void Quit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
