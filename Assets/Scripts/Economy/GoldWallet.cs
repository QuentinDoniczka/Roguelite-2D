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

        public void ResetGold()
        {
            _gold = 0;
            OnGoldChanged?.Invoke(_gold);
        }
    }
}
