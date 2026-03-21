using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Debug tool for testing CombatWorld scrolling with acceleration/deceleration.
    /// OnGUI panel with editable fields for distance, max speed, and acceleration.
    /// Deceleration mirrors acceleration so the motion feels symmetric.
    /// </summary>
    public class WorldConveyorDebug : MonoBehaviour
    {
        private float _targetX;
        private float _currentSpeed;
        private bool _isScrolling;

        // GUI input strings
        private string _distanceStr = "10";
        private string _maxSpeedStr = "5";
        private string _accelStr = "3";

        // Parsed values
        private float _distance = 10f;
        private float _maxSpeed = 5f;
        private float _acceleration = 3f;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _targetX = transform.position.x;
        }

        private void Update()
        {
            if (!_isScrolling)
                return;

            float posX = transform.position.x;
            float remaining = Mathf.Abs(_targetX - posX);

            // Braking distance: v² / (2 * a)
            float brakingDist = (_currentSpeed * _currentSpeed) / (2f * _acceleration);

            if (remaining <= brakingDist)
            {
                // Decelerate
                _currentSpeed -= _acceleration * Time.deltaTime;
                if (_currentSpeed < 0.01f)
                    _currentSpeed = 0.01f;
            }
            else if (_currentSpeed < _maxSpeed)
            {
                // Accelerate
                _currentSpeed += _acceleration * Time.deltaTime;
                if (_currentSpeed > _maxSpeed)
                    _currentSpeed = _maxSpeed;
            }

            float direction = Mathf.Sign(_targetX - posX);
            float step = _currentSpeed * Time.deltaTime;

            if (step >= remaining)
            {
                transform.position = new Vector3(_targetX, transform.position.y, transform.position.z);
                _currentSpeed = 0f;
                _isScrolling = false;
            }
            else
            {
                float newX = posX + direction * step;
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
        }

        // ------------------------------------------------------------------ GUI

        private void OnGUI()
        {
            const int PanelW = 520;
            const int PanelH = 380;
            const int FontSize = 22;
            const int BtnHeight = 60;
            const int FieldW = 120;
            const int LabelW = 200;

            float px = (Screen.width - PanelW) * 0.5f;
            float py = (Screen.height - PanelH) * 0.5f;
            Rect panelRect = new Rect(px, py, PanelW, PanelH);

            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(px + 16f, py + 12f, PanelW - 32f, PanelH - 24f));

            GUIStyle label = new GUIStyle(GUI.skin.label)
            {
                fontSize = FontSize,
                normal = { textColor = Color.white }
            };

            GUIStyle labelYellow = new GUIStyle(label)
            {
                normal = { textColor = Color.yellow },
                fontStyle = FontStyle.Bold
            };

            GUIStyle field = new GUIStyle(GUI.skin.textField) { fontSize = FontSize };
            GUIStyle btn = new GUIStyle(GUI.skin.button) { fontSize = FontSize, fontStyle = FontStyle.Bold };

            // Status
            GUILayout.Label($"Position X: {transform.position.x:F2}   Speed: {_currentSpeed:F1}", labelYellow);
            GUILayout.Space(10f);

            // Distance
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance:", label, GUILayout.Width(LabelW));
            _distanceStr = GUILayout.TextField(_distanceStr, field, GUILayout.Width(FieldW), GUILayout.Height(32f));
            float.TryParse(_distanceStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _distance);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            // Max speed
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Speed:", label, GUILayout.Width(LabelW));
            _maxSpeedStr = GUILayout.TextField(_maxSpeedStr, field, GUILayout.Width(FieldW), GUILayout.Height(32f));
            float.TryParse(_maxSpeedStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _maxSpeed);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            // Acceleration (= deceleration)
            GUILayout.BeginHorizontal();
            GUILayout.Label("Accel / Decel:", label, GUILayout.Width(LabelW));
            _accelStr = GUILayout.TextField(_accelStr, field, GUILayout.Width(FieldW), GUILayout.Height(32f));
            float.TryParse(_accelStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _acceleration);
            GUILayout.EndHorizontal();

            GUILayout.Space(14f);

            // Scroll button
            if (GUILayout.Button(">> SCROLL >>", btn, GUILayout.Height(BtnHeight)))
            {
                if (_distance > 0f && _acceleration > 0f && _maxSpeed > 0f)
                {
                    _targetX = transform.position.x - _distance;
                    _currentSpeed = 0f;
                    _isScrolling = true;
                }
            }

            GUILayout.EndArea();
        }
    }
}
