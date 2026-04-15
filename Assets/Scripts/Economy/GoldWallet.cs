using UnityEngine;

namespace RogueliteAutoBattler.Economy
{
    public class GoldWallet : MonoBehaviour
    {
        private int _gold;

        public int Gold => _gold;

        public event System.Action<int> OnGoldChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

        public bool CanAfford(int cost)
        {
            return _gold >= cost;
        }

        public bool Spend(int cost)
        {
            if (cost <= 0 || _gold < cost) return false;
            _gold -= cost;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        public void ResetGold()
        {
            _gold = 0;
            OnGoldChanged?.Invoke(_gold);
        }
    }
}
