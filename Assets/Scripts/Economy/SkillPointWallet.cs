using UnityEngine;

namespace RogueliteAutoBattler.Economy
{
    public class SkillPointWallet : MonoBehaviour
    {
        private int _points;

        public int Points => _points;

        public event System.Action<int> OnPointsChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _points += amount;
            OnPointsChanged?.Invoke(_points);
        }

        public bool CanAfford(int cost)
        {
            return _points >= cost;
        }

        public bool Spend(int cost)
        {
            if (cost <= 0 || _points < cost) return false;
            _points -= cost;
            OnPointsChanged?.Invoke(_points);
            return true;
        }

        public void ResetPoints()
        {
            _points = 0;
            OnPointsChanged?.Invoke(_points);
        }
    }
}
