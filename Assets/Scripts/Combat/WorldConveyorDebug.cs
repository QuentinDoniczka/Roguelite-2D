using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Debug tool for testing CombatWorld scrolling with acceleration/deceleration.
    /// Creates a Canvas-based UI at runtime with editable fields and a scroll button.
    /// Works with the New Input System (no OnGUI dependency).
    /// </summary>
    public class WorldConveyorDebug : MonoBehaviour
    {
        private float _targetX;
        private float _currentSpeed;
        private bool _isScrolling;

        private float _distance = 10f;
        private float _maxSpeed = 5f;
        private float _acceleration = 3f;

        private TMP_InputField _distanceField;
        private TMP_InputField _maxSpeedField;
        private TMP_InputField _accelField;
        private TextMeshProUGUI _statusLabel;

        private void Awake()
        {
            _targetX = transform.position.x;
            BuildDebugUI();
        }

        private void Update()
        {
            // Update status label
            if (_statusLabel != null)
            {
                string state = _isScrolling ? $"SCROLLING  v={_currentSpeed:F1}" : "IDLE";
                _statusLabel.text = $"X={transform.position.x:F2}  |  {state}";
            }

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
                transform.position = new Vector3(posX + direction * step, transform.position.y, transform.position.z);
            }
        }

        private void OnScrollClicked()
        {
            ParseFields();
            if (_distance <= 0f || _acceleration <= 0.01f || _maxSpeed <= 0.01f)
            {
                Debug.LogWarning("[WorldConveyorDebug] Values must be > 0");
                return;
            }

            _targetX = transform.position.x - _distance;
            _currentSpeed = 0f;
            _isScrolling = true;
            Debug.Log($"[WorldConveyorDebug] Scroll: dist={_distance}, speed={_maxSpeed}, accel={_acceleration}, target={_targetX:F2}");
        }

        private void ParseFields()
        {
            if (_distanceField != null)
                float.TryParse(_distanceField.text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _distance);
            if (_maxSpeedField != null)
                float.TryParse(_maxSpeedField.text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _maxSpeed);
            if (_accelField != null)
                float.TryParse(_accelField.text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _acceleration);
        }

        // ------------------------------------------------------------------ UI builder

        private void BuildDebugUI()
        {
            var canvasGo = new GameObject("DebugCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGo.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Panel background
            var panelGo = new GameObject("DebugPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            RectTransform panelRT = panelGo.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(700, 500);
            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.8f);

            VerticalLayoutGroup vLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(20, 20, 16, 16);
            vLayout.spacing = 8f;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;

            // Status
            _statusLabel = CreateLabel(panelGo.transform, "X=0  |  IDLE", 28, Color.yellow);

            // Fields
            _distanceField = CreateFieldRow(panelGo.transform, "Distance:", _distance.ToString("F1"));
            _maxSpeedField = CreateFieldRow(panelGo.transform, "Max Speed:", _maxSpeed.ToString("F1"));
            _accelField = CreateFieldRow(panelGo.transform, "Accel/Decel:", _acceleration.ToString("F1"));

            // Spacer
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(panelGo.transform, false);
            spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 8);
            spacer.AddComponent<LayoutElement>().preferredHeight = 8f;

            // Button
            var btnGo = new GameObject("ScrollButton");
            btnGo.transform.SetParent(panelGo.transform, false);
            btnGo.AddComponent<RectTransform>();
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.2f, 1f);
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(OnScrollClicked);
            LayoutElement btnLE = btnGo.AddComponent<LayoutElement>();
            btnLE.preferredHeight = 80f;

            TextMeshProUGUI btnText = CreateLabel(btnGo.transform, ">> SCROLL >>", 32, Color.white);
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private TMP_InputField CreateFieldRow(Transform parent, string label, string defaultValue)
        {
            var rowGo = new GameObject("Row_" + label);
            rowGo.transform.SetParent(parent, false);
            rowGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup hLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 10f;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            LayoutElement rowLE = rowGo.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 50f;

            // Label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            labelGo.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = labelGo.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 280f;

            // Input field
            var fieldGo = new GameObject("InputField");
            fieldGo.transform.SetParent(rowGo.transform, false);
            fieldGo.AddComponent<RectTransform>();
            Image fieldBg = fieldGo.AddComponent<Image>();
            fieldBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            LayoutElement fieldLE = fieldGo.AddComponent<LayoutElement>();
            fieldLE.flexibleWidth = 1f;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(fieldGo.transform, false);
            RectTransform textRT = textGo.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(8, 4);
            textRT.offsetMax = new Vector2(-8, -4);
            TextMeshProUGUI fieldTmp = textGo.AddComponent<TextMeshProUGUI>();
            fieldTmp.fontSize = 26;
            fieldTmp.color = Color.white;

            TMP_InputField inputField = fieldGo.AddComponent<TMP_InputField>();
            inputField.textComponent = fieldTmp;
            inputField.textViewport = textRT;
            inputField.text = defaultValue;
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;

            return inputField;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, int fontSize, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;
            return tmp;
        }
    }
}
