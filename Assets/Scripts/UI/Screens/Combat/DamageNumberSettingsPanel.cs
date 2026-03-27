using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.Combat
{
    public class DamageNumberSettingsPanel : MonoBehaviour
    {
        [SerializeField] private DamageNumberConfig _config;
        [SerializeField] private CanvasGroup _panelCanvasGroup;

        [Header("Sliders")]
        [SerializeField] private Slider _fontSizeSlider;
        [SerializeField] private Slider _lifetimeSlider;
        [SerializeField] private Slider _slideDistanceSlider;
        [SerializeField] private Slider _spawnOffsetYSlider;

        [Header("Slider Labels")]
        [SerializeField] private TMP_Text _fontSizeLabel;
        [SerializeField] private TMP_Text _lifetimeLabel;
        [SerializeField] private TMP_Text _slideDistanceLabel;
        [SerializeField] private TMP_Text _spawnOffsetYLabel;

        [Header("Color Buttons")]
        [SerializeField] private Button[] _allyColorButtons;
        [SerializeField] private Button[] _enemyColorButtons;

        [Header("Reset")]
        [SerializeField] private Button _resetButton;

        private static readonly Color[] ALLY_COLOR_PRESETS =
        {
            new Color(1f, 0.2f, 0.2f, 1f),
            new Color(1f, 0.5f, 0f, 1f),
            new Color(1f, 0.9f, 0.1f, 1f),
            new Color(1f, 0.1f, 0.6f, 1f),
            new Color(0.7f, 0.2f, 1f, 1f),
            new Color(0f, 0.9f, 1f, 1f),
            new Color(0.2f, 1f, 0.2f, 1f),
            Color.white
        };

        private static readonly Color[] ENEMY_COLOR_PRESETS =
        {
            Color.white,
            new Color(0.6f, 0.8f, 1f, 1f),
            new Color(0.6f, 1f, 0.6f, 1f),
            new Color(1f, 1f, 0.6f, 1f),
            new Color(1f, 0.7f, 0.8f, 1f),
            new Color(0.7f, 0.7f, 0.7f, 1f),
            new Color(1f, 0.84f, 0f, 1f),
            new Color(0f, 0.9f, 1f, 1f)
        };

        private const float COLOR_MATCH_TOLERANCE = 0.02f;
        private const float ACTIVE_BUTTON_SCALE = 1.15f;

        private bool _isOpen;

        private void Awake()
        {
            SetupSlider(_fontSizeSlider, 1f, 20f);
            SetupSlider(_lifetimeSlider, 0.2f, 3f);
            SetupSlider(_slideDistanceSlider, 0.1f, 2f);
            SetupSlider(_spawnOffsetYSlider, 0f, 1f);

            _fontSizeSlider.onValueChanged.AddListener(OnFontSizeChanged);
            _lifetimeSlider.onValueChanged.AddListener(OnLifetimeChanged);
            _slideDistanceSlider.onValueChanged.AddListener(OnSlideDistanceChanged);
            _spawnOffsetYSlider.onValueChanged.AddListener(OnSpawnOffsetYChanged);

            BindColorButtons(_allyColorButtons, ALLY_COLOR_PRESETS, true);
            BindColorButtons(_enemyColorButtons, ENEMY_COLOR_PRESETS, false);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);

            SyncUIFromConfig();
            SetPanelVisible(false);
        }

        public void Toggle()
        {
            _isOpen = !_isOpen;
            if (_isOpen)
                SyncUIFromConfig();
            SetPanelVisible(_isOpen);
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (_panelCanvasGroup == null)
                return;

            _panelCanvasGroup.alpha = isVisible ? 1f : 0f;
            _panelCanvasGroup.blocksRaycasts = isVisible;
            _panelCanvasGroup.interactable = isVisible;
        }

        private void SyncUIFromConfig()
        {
            if (_config == null)
                return;

            _fontSizeSlider.SetValueWithoutNotify(_config.FontSize);
            _lifetimeSlider.SetValueWithoutNotify(_config.Lifetime);
            _slideDistanceSlider.SetValueWithoutNotify(_config.SlideDistance);
            _spawnOffsetYSlider.SetValueWithoutNotify(_config.SpawnOffsetY);

            UpdateLabel(_fontSizeLabel, _config.FontSize);
            UpdateLabel(_lifetimeLabel, _config.Lifetime);
            UpdateLabel(_slideDistanceLabel, _config.SlideDistance);
            UpdateLabel(_spawnOffsetYLabel, _config.SpawnOffsetY);

            HighlightActiveColors();
        }

        private void OnFontSizeChanged(float value)
        {
            _config.FontSize = value;
            UpdateLabel(_fontSizeLabel, value);
            Save();
        }

        private void OnLifetimeChanged(float value)
        {
            _config.Lifetime = value;
            UpdateLabel(_lifetimeLabel, value);
            Save();
        }

        private void OnSlideDistanceChanged(float value)
        {
            _config.SlideDistance = value;
            UpdateLabel(_slideDistanceLabel, value);
            Save();
        }

        private void OnSpawnOffsetYChanged(float value)
        {
            _config.SpawnOffsetY = value;
            UpdateLabel(_spawnOffsetYLabel, value);
            Save();
        }

        private void BindColorButtons(Button[] buttons, Color[] presets, bool isAllyRow)
        {
            for (int i = 0; i < buttons.Length && i < presets.Length; i++)
            {
                int index = i;
                buttons[i].onClick.AddListener(() =>
                {
                    if (isAllyRow)
                        _config.AllyDamageColor = presets[index];
                    else
                        _config.EnemyDamageColor = presets[index];

                    HighlightActiveColors();
                    Save();
                });
            }
        }

        private void OnResetClicked()
        {
            _config.RestoreDefaults();
            DamageNumberSettingsPersistence.DeleteAll();
            SyncUIFromConfig();
        }

        private void Save()
        {
            DamageNumberSettingsPersistence.Save(_config);
        }

        private static void SetupSlider(Slider slider, float min, float max)
        {
            if (slider == null)
                return;

            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
        }

        private static void UpdateLabel(TMP_Text label, float value)
        {
            if (label != null)
                label.text = value.ToString("F1");
        }

        private void HighlightActiveColors()
        {
            HighlightRow(_allyColorButtons, ALLY_COLOR_PRESETS, _config.AllyDamageColor);
            HighlightRow(_enemyColorButtons, ENEMY_COLOR_PRESETS, _config.EnemyDamageColor);
        }

        private static void HighlightRow(Button[] buttons, Color[] presets, Color currentColor)
        {
            for (int i = 0; i < buttons.Length && i < presets.Length; i++)
            {
                bool isActive = ColorsApproxEqual(presets[i], currentColor);
                var image = buttons[i].GetComponent<Image>();
                if (image != null)
                {
                    Color displayColor = presets[i];
                    displayColor.a = isActive ? 1f : 0.4f;
                    image.color = displayColor;
                }
                buttons[i].transform.localScale = isActive ? Vector3.one * ACTIVE_BUTTON_SCALE : Vector3.one;
            }
        }

        private static bool ColorsApproxEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < COLOR_MATCH_TOLERANCE
                && Mathf.Abs(a.g - b.g) < COLOR_MATCH_TOLERANCE
                && Mathf.Abs(a.b - b.b) < COLOR_MATCH_TOLERANCE
                && Mathf.Abs(a.a - b.a) < COLOR_MATCH_TOLERANCE;
        }
    }
}
