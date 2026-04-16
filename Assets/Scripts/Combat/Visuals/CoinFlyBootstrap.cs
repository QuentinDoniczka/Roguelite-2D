using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public class CoinFlyBootstrap : MonoBehaviour
    {
        [SerializeField] private RectTransform _coinContainer;
        [SerializeField] private RectTransform _targetBadge;
        [SerializeField] private Sprite _coinSprite;

        private void Awake()
        {
            if (_coinContainer == null)
            {
                Debug.LogError("[CoinFlyBootstrap] _coinContainer is not assigned. CoinFlyService will not initialize.");
                return;
            }

            if (_coinSprite == null)
            {
                Debug.LogError("[CoinFlyBootstrap] _coinSprite is not assigned. CoinFlyService will not initialize.");
                return;
            }

            Camera mainCamera = Camera.main;
            Canvas canvas = GetComponentInParent<Canvas>();

            CoinFlyService.Initialize(_coinContainer, _targetBadge, mainCamera, canvas, _coinSprite);
        }
    }
}
