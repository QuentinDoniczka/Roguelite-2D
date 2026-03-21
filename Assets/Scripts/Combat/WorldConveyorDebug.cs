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

        // Parsed values (defaults)
        private float _distance = 10f;
        private float _maxSpeed = 5f;
        private float _acceleration = 3f;

        // GUI input strings
        private string _distanceStr;
        private string _maxSpeedStr;
        private string _accelStr;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _targetX = transform.position.x;
            _distanceStr = _distance.ToString("F1");
            _maxSpeedStr = _maxSpeed.ToString("F1");
            _accelStr = _acceleration.ToString("F1");
        }

        private void Update()
        {
            if (!_isScrolling)
                return;

            float posX = transform.position.x;
            float remaining = Mathf.Abs(_targetX - posX);

            if (remaining < 0.01f)
            {
                transform.position = new Vector3(_targetX, transform.position.y, transform.position.z);
                _currentSpeed = 0f;
                _isScrolling = false;
                Debug.Log($"[WorldConveyorDebug] Arrived at X={_targetX:F2}");
                return;
            }

            // Braking distance: v² / (2 * a)
            float brakingDist = (_currentSpeed * _currentSpeed) / (2f * _acceleration);

            if (remaining <= brakingDist + 0.1f)
            {
                _currentSpeed -= _acceleration * Time.deltaTime;
                if (_currentSpeed < 0.1f)
                    _currentSpeed = 0.1f;
            }
            else if (_currentSpeed < _maxSpeed)
            {
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
                Debug.Log($"[WorldConveyorDebug] Arrived at X={_targetX:F2}");
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
            const float PanelW = 500f;
            const float PanelH = 360f;
            const float Pad = 14f;
            const float RowH = 34f;
            const float BtnH = 55f;
            const float LabelW = 190f;

            float px = (Screen.width - PanelW) * 0.5f;
            float py = (Screen.height - PanelH) * 0.5f;
            Rect panel = new Rect(px, py, PanelW, PanelH);

            // Background
            GUI.Box(panel, "");

            float y = panel.y + Pad;
            float contentW = PanelW - Pad * 2f;
            float fieldX = panel.x + Pad + LabelW + 8f;
            float fieldW = contentW - LabelW - 8f;

            // Status line
            string status = _isScrolling ? $"SCROLLING  speed={_currentSpeed:F1}" : "IDLE";
            GUI.Label(new Rect(panel.x + Pad, y, contentW, RowH),
                $"X={transform.position.x:F2}  |  {status}");
            y += RowH + 6f;

            // Distance
            GUI.Label(new Rect(panel.x + Pad, y, LabelW, RowH), "Distance:");
            _distanceStr = GUI.TextField(new Rect(fieldX, y, fieldW, RowH), _distanceStr);
            float.TryParse(_distanceStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _distance);
            y += RowH + 4f;

            // Max speed
            GUI.Label(new Rect(panel.x + Pad, y, LabelW, RowH), "Max Speed:");
            _maxSpeedStr = GUI.TextField(new Rect(fieldX, y, fieldW, RowH), _maxSpeedStr);
            float.TryParse(_maxSpeedStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _maxSpeed);
            y += RowH + 4f;

            // Accel/Decel
            GUI.Label(new Rect(panel.x + Pad, y, LabelW, RowH), "Accel / Decel:");
            _accelStr = GUI.TextField(new Rect(fieldX, y, fieldW, RowH), _accelStr);
            float.TryParse(_accelStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _acceleration);
            y += RowH + 12f;

            // Scroll button
            if (GUI.Button(new Rect(panel.x + Pad, y, contentW, BtnH), ">> SCROLL >>"))
            {
                if (_distance > 0f && _acceleration > 0.01f && _maxSpeed > 0.01f)
                {
                    _targetX = transform.position.x - _distance;
                    _currentSpeed = 0f;
                    _isScrolling = true;
                    Debug.Log($"[WorldConveyorDebug] Scroll started: distance={_distance}, maxSpeed={_maxSpeed}, accel={_acceleration}, target={_targetX:F2}");
                }
                else
                {
                    Debug.LogWarning("[WorldConveyorDebug] Invalid values — distance, speed, and acceleration must be > 0");
                }
            }
        }
    }
}
