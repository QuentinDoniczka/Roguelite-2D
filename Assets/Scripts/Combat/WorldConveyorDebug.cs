using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Simple debug MonoBehaviour attached to CombatWorld.
    /// Provides an OnGUI panel to trigger a horizontal scroll of the world
    /// and verify that the tiled ground moves correctly.
    /// </summary>
    public class WorldConveyorDebug : MonoBehaviour
    {
        [SerializeField] private float _scrollDistance = 10f;
        [SerializeField] private float _scrollSpeed = 5f;

        private float _targetX;
        private bool _isScrolling;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _targetX = transform.position.x;
        }

        private void Update()
        {
            if (!_isScrolling)
                return;

            Vector3 pos = transform.position;
            float newX = Mathf.MoveTowards(pos.x, _targetX, _scrollSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, pos.y, pos.z);

            if (Mathf.Abs(newX - _targetX) < 0.01f)
            {
                transform.position = new Vector3(_targetX, pos.y, pos.z);
                _isScrolling = false;
            }
        }

        // ------------------------------------------------------------------ GUI

        private void OnGUI()
        {
            const int PanelW = 500;
            const int PanelH = 300;
            const int FontSize = 24;
            const int BtnHeight = 60;

            float px = (Screen.width - PanelW) * 0.5f;
            float py = (Screen.height - PanelH) * 0.5f;
            Rect panelRect = new Rect(px, py, PanelW, PanelH);

            // Semi-transparent background
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(panelRect);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = FontSize,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = FontSize,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Space(16f);

            // Position readout
            GUILayout.Label($"Position X: {transform.position.x:F2}", labelStyle);

            GUILayout.Space(8f);

            // Distance field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance:", labelStyle, GUILayout.Width(160f));
            string distStr = GUILayout.TextField(_scrollDistance.ToString("F1"), labelStyle, GUILayout.Width(100f));
            if (float.TryParse(distStr, out float parsed))
                _scrollDistance = parsed;
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);

            // Scroll button
            if (GUILayout.Button(">> SCROLL >>", btnStyle, GUILayout.Height(BtnHeight)))
            {
                _targetX = transform.position.x - _scrollDistance;
                _isScrolling = true;
            }

            GUILayout.EndArea();
        }
    }
}
